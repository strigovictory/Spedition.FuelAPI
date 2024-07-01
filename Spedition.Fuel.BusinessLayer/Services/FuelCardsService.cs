using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NPOI.SS.Formula.Functions;
using Spedition.Fuel.BFF.Constants;
using Spedition.Fuel.BusinessLayer.Enums;
using Spedition.Fuel.BusinessLayer.Helpers;
using Spedition.Fuel.BusinessLayer.Services.BaseServices;
using Spedition.Fuel.DataAccess.Infrastructure.Repositories;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Shared;
using Spedition.Fuel.Shared.Entities;
using Spedition.Fuel.Shared.Enums;
using Spedition.Fuel.Shared.Settings;
using Spedition.Office.Shared.Entities;

namespace Spedition.Fuel.BusinessLayer.Services
{
    public class FuelCardsService : GroupActionBase<FuelCardShortResponse, FuelCardResponse, FuelCard>, IFuelCardsService
    {
        private readonly ITruckService truckService;
        private readonly IDivisionService divisionService;
        protected readonly IEventsTypeService eventsTypeService;
        protected readonly IEmployeeService employeeService;
        protected readonly FuelCardRepository fuelRepository;

        public FuelCardsService(
            FuelCardRepository fuelRepository,
            IWebHostEnvironment env,
            IConfiguration configuration,
            IMapper mapper,
            ITruckService truckService,
            IDivisionService divisionService,
            IEventsTypeService eventsTypeService,
            IEmployeeService employeeService)
            : base(env, configuration, mapper)
        {
            this.truckService = truckService;
            this.divisionService = divisionService;
            this.eventsTypeService = eventsTypeService;
            this.employeeService = employeeService;
            this.fuelRepository = fuelRepository;
        }

        #region Overriden
        public override async Task<bool> CheckIsInstanceExist(FuelCard checkedCard)
        {
            // В рамках одного подразделения номера карт д.б. уникальны
            var result = await fuelRepository?.AnyCard(
                card => card.Id != checkedCard.Id
                && card.DivisionID == checkedCard.DivisionID
                && card.Number == checkedCard.Number);

            if (result)
            {
                ExistedInstances?.Add(checkedCard);
            }

            return result;
        }
        #endregion

        #region Get
        public async Task<FuelCardResponse> GetCard(int cardId, CancellationToken token = default)
        {
            var dbCard = await GetCardInner(cardId);
            return mapper.Map<FuelCardResponse>(dbCard);
        }

        protected async Task<FuelCard> GetCardInner(int cardId, CancellationToken token = default)
        {
            return await fuelRepository.GetCard(cardId, token);
        }

        public async Task<List<FuelCardFullResponse>> GetCards(CancellationToken token = default)
        {
            var trucks = truckService?.GetTrucks()?.GetAwaiter().GetResult() ?? new();
            var divisions = divisionService?.GetDivisions()?.GetAwaiter().GetResult() ?? new();
            var employees = employeeService?.GetEmployees()?.GetAwaiter().GetResult() ?? new();

            return await fuelRepository.GetCards(divisions, trucks, employees, token);
        }

        public async Task<List<FuelCardNotFoundResponse>> GetNotFoundCards(CancellationToken token = default)
        {
            return await fuelRepository.GetNotFoundCards(token);
        }

        public async Task<List<FuelCardsAlternativeNumberResponse>> GetCardsAlternativeNumbers(int cardId, CancellationToken token = default)
        {
            return (await fuelRepository.FindCardsAlternativeNumbers(num => num.CardId == cardId, token))?
                .Select(alternativeItems => mapper?.Map<FuelCardsAlternativeNumberResponse>(alternativeItems)).ToList() ?? new();
        }

        #endregion

        #region Put
        protected FuelCard UpdateCard(FuelCard card)
        {
            var result = fuelRepository.UpdateCard(card);
            NotifyMessage += fuelRepository.NotifyMessage ?? string.Empty;
            return result;
        }

        public async Task UpdateCard(FuelCardRequest card, string user, CancellationToken token = default)
        {
            SuccessItems = new();
            ErrorItems = new();
            ExistedInstances = new();
            NotifyMessage = string.Empty;

            Func<FuelCardRequest, Task<FuelCard>> toUpdateCard = async (FuelCardRequest card) =>
            {
                FuelCard updatedCard = null;

                if ((card?.Id ?? 0) == 0 || string.IsNullOrEmpty(card.Number))
                {
                    NotifyMessage += string.IsNullOrEmpty(card.Number) ? "Номер топливной карты имеет пустое значение. " : "Топливная карта имеет пустое значение. ";
                    NotifyMessage.LogError(GetType().Name, nameof(toUpdateCard));
                    return updatedCard;
                }

                var dbCard = await fuelRepository?.GetCard(card.Id, token);
                if (dbCard == null) return updatedCard;

                var cardToUpdate = mapper.Map<FuelCard>(card);

                // 1 - Если изменился номер топливной карты, проверить, есть ли в БД идентичный
                var isNumberExist = dbCard.Number.ToLower() != cardToUpdate.Number.ToLower() && await CheckIsInstanceExist(cardToUpdate);
                if (isNumberExist)
                {
                    ExistedInstances?.Add(cardToUpdate);
                    return updatedCard;
                }

                var isNeedUpdateLastEvent = NeedUpdateLastFuelCardEvent(cardToUpdate, dbCard); // Проверить, нужно ли редактировать последнее событие по карте или нет

                // 2 - Сохранить отредактированную топливную карту в БД.
                if (cardToUpdate.IsArchived) // Если отмечен флаг «В архиве» - удалить ссылки на обьект
                {
                    cardToUpdate.CarId = null;
                    cardToUpdate.EmployeeId = null;
                }

                updatedCard = UpdateCard(cardToUpdate) ?? new();

                // 3 - Отредактировать последнее событие топливной карты - при необходимости
                if (isNeedUpdateLastEvent)
                {
                    await UpdateFuelCardEvent(user, updatedCard, token);
                }

                return updatedCard;
            };

            // Обновить в БД отредактированную топливную карту и добавить событие изменения ее статуса
            var updatedCard = card != null ? await toUpdateCard.Invoke(card) : default;
            if ((updatedCard?.Id ?? 0) > 0)
            {
                SuccessItems.Add(mapper.Map<FuelCardShortResponse>(updatedCard));
            }
            else
            {
                ErrorItems.Add(new NotSuccessResponseItemDetailed<FuelCardResponse>(mapper.Map<FuelCardResponse>(card), NotifyMessage));
            }
        }

        public async Task MoveCardsToArchive(List<FuelCardRequest> cards, string user, CancellationToken token = default)
        {
            SuccessItems = new();
            ErrorItems = new();
            NotifyMessage = string.Empty;

            Func<FuelCardRequest, Task<FuelCard>> toUpdateCard = async (FuelCardRequest card) =>
            {
                FuelCard updatedCard = null;

                if ((card?.Id ?? 0) == 0)
                {
                    NotifyMessage += "Топливная карта имеет пустое значение. ";
                    NotifyMessage.LogError(GetType().Name, nameof(toUpdateCard));
                    return updatedCard;
                }

                // 1 - Найти топливную карту в БД.
                var dbCard = await fuelRepository?.GetCard(card.Id, token);
                if (dbCard == null) return updatedCard;

                var cardToUpdate = mapper.Map<FuelCard>(card);

                // 2 - Сохранить отредактированную топливную карту в БД.
                cardToUpdate.IsArchived = true;
                cardToUpdate.CarId = null;
                cardToUpdate.EmployeeId = null;
                updatedCard = UpdateCard(cardToUpdate) ?? new();

                // 3 - Отредактировать последнее событие топливной карты в части даты окончания - при необходимости
                var lastEvent = await GetLastCardsEvent(card.Id, token);
                if (lastEvent != null)
                {
                    var eventsTypes = await GetKitEventTypes(token);
                    if (eventsTypes.Any())
                    {
                        SetFinishDateInPreviousEvent(lastEvent, DateTime.Today, user);
                    }
                }

                // 4 - Создать новое событие топливной карты АРХИВ
                await CreateFuelCardEvent(user, cardToUpdate, token);

                return updatedCard;
            };

            // Поочередно обновить в БД отредактированные топливные карты и добавить события изменения их статусов
            foreach(var card in cards ?? new())
            {
                var updatedCard = card != null ? await toUpdateCard.Invoke(card) : default;
                if ((updatedCard?.Id ?? 0) > 0)
                {
                    SuccessItems.Add(mapper.Map<FuelCardShortResponse>(updatedCard));
                }
                else
                {
                    ErrorItems.Add(new NotSuccessResponseItemDetailed<FuelCardResponse>(mapper.Map<FuelCardResponse>(card), NotifyMessage));
                }
            }
        }

        private async Task UpdateFuelCardEvent(string userName, FuelCard card, CancellationToken token = default)
        {
            if (card is null)
                return;

            var lastEvent = await GetLastCardsEvent(card.Id, token);

            if (lastEvent == null)
                return;

            var eventsTypes = await GetKitEventTypes(token);

            if (!eventsTypes.Any())
                return; // если типы событий пустые - будет ошибка внешнего ключа, поэтому возврат

            if (card.IsArchived) // 1 - Если установлен флаг «В архиве»
            {
                lastEvent.CarId = null;
                lastEvent.EmployeeId = null;
                lastEvent.DivisionID = card.DivisionID;
                lastEvent.EventTypeId = EventsTypeName.Архив.GetEventTypeId(eventsTypes);
                lastEvent.ModifiedOn = DateTime.Today;
                lastEvent.ModifiedBy = userName;
            }
            else if (card.CarId.HasValue && card.CarId.Value > 0) // 2 - Если флаг «В архиве» выключен и есть привязка к авто - то событие ТЯГАЧ
            {
                lastEvent.CarId = card.CarId;
                lastEvent.EmployeeId = null;
                lastEvent.DivisionID = card.DivisionID;
                lastEvent.EventTypeId = EventsTypeName.Тягач.GetEventTypeId(eventsTypes);
                lastEvent.ModifiedOn = DateTime.Today;
                lastEvent.ModifiedBy = userName;
            }
            else if (card.EmployeeId.HasValue && card.EmployeeId.Value > 0) // 3 - Если флаг «В архиве» выключен и есть привязка к волителю -то событие ВОДИТЕЛЬ
            {
                lastEvent.CarId = null;
                lastEvent.EmployeeId = card.EmployeeId;
                lastEvent.DivisionID = card.DivisionID;
                lastEvent.EventTypeId = EventsTypeName.Водитель.GetEventTypeId(eventsTypes);
                lastEvent.ModifiedOn = DateTime.Today;
                lastEvent.ModifiedBy = userName;
            }
            else // 4 - Если флаг «В архиве» выключен и нет привязки к авто / водителю - то событие СКЛАД
            {
                lastEvent.CarId = null;
                lastEvent.EmployeeId = null;
                lastEvent.DivisionID = card.DivisionID;
                lastEvent.EventTypeId = EventsTypeName.Склад.GetEventTypeId(eventsTypes);
                lastEvent.ModifiedOn = DateTime.Today;
                lastEvent.ModifiedBy = userName;
            }

            UpdateCardsEvent(lastEvent);
        }

        public async Task<FuelCardsAlternativeNumberResponse> UpdateCardsAlternativeNumber(
            FuelCardsAlternativeNumberRequest alternativeNumber, CancellationToken token = default)
        {
            FuelCardsAlternativeNumberResponse result = new();
            NotifyMessage = string.Empty;
            var toUpdate = mapper.Map<FuelCardsAlternativeNumber>(alternativeNumber);

            if (await fuelRepository.AnyAlternativeNumber(
                number => number.CardId == toUpdate.CardId
                && number.Id != toUpdate.Id
                && number.Number == toUpdate.Number,
                token))
            {
                NotifyMessage = "Аналогичный альтернативный номер топливной карты уже сохранен в БД. ";
                return result;
            }

            var resultUpdate = fuelRepository?.UpdateCardsAlternativeNumber(toUpdate);
            result = mapper.Map<FuelCardsAlternativeNumberResponse>(resultUpdate);
            NotifyMessage += fuelRepository.NotifyMessage ?? string.Empty;

            return result;
        }
        #endregion

        #region Post
        public async Task CreateCards(List<FuelCardRequest> cards, string user, CancellationToken token = default)
        {
            NotifyMessage = string.Empty;
            SuccessItems = new();
            ErrorItems = new();
            ExistedInstances = new();

            if ((cards?.Count ?? 0) == 0)
            {
                NotifyMessage += "Пустая коллекция топливных карт не подлежит сохранению в БД !";
                NotifyMessage.LogError(GetType().Name, nameof(CreateCards));
                return;
            }

            // 1 - Предварительно исключить из коллекции те карты, которые уже существуют в БД
            var trimmedCards = await TrimExistingFuelCards(cards);

            foreach (var card in trimmedCards)
            {
                FuelCard createdCard = new();

                // 2 - Сохранить новую топливную карту в БД
                createdCard = CreateCard(card);
                NotifyMessage = fuelRepository.NotifyMessage ?? string.Empty;

                if (createdCard?.Id > 0)
                {
                    SuccessItems?.Add(mapper?.Map<FuelCardShortResponse>(createdCard));
                    NotifyMessage += $"{createdCard?.ToString() ?? string.Empty} была сохранена !";
                }
                else
                {
                    NotifyMessage += $"{card?.ToString() ?? string.Empty} не была сохранена !";
                    ErrorItems?.Add(
                        new NotSuccessResponseItemDetailed<FuelCardResponse>
                        {
                            NotSuccessItem = mapper?.Map<FuelCardResponse>(card),
                            Reason = NotifyMessage,
                        });
                    continue;
                }

                // 3 -  Сформировать события изменения статуса топливной карты и сохранить их в БД
                await CreateFuelCardEvent(user, createdCard, token);
            }
        }

        public async Task<FuelCardsAlternativeNumberResponse> CreateCardsAlternativeNumber(
            FuelCardsAlternativeNumberRequest alternativeNumber, CancellationToken token = default)
        {
            NotifyMessage = string.Empty;
            FuelCardsAlternativeNumberResponse result = default;

            if (alternativeNumber == null 
                || string.IsNullOrEmpty(alternativeNumber.Number) 
                || string.IsNullOrWhiteSpace(alternativeNumber.Number))
            {
                NotifyMessage += "Алтернативный номер топливной карты не может быть пустым. ";
                return result;
            }

            if (await fuelRepository.AnyAlternativeNumber(
                number => number.CardId == alternativeNumber.CardId
                && number.Number.ToLower() == alternativeNumber.Number.ToLower(), token))
            {
                NotifyMessage = "Аналогичный альтернативный номер топливной карты уже сохранен в БД. ";
                return result;
            }

            var toAdd = mapper.Map<FuelCardsAlternativeNumber>(alternativeNumber);
            var added = fuelRepository.CreateCardsAlternativeNumber(toAdd);
            result = mapper.Map<FuelCardsAlternativeNumberResponse>(added);

            return result;
        }
        #endregion

        #region Delete

        /// <summary>
        /// Метод для удаления из БД коллекции топливных карт.
        /// </summary>
        /// <param name="cardsIds">Коллекция идентификатороов топливных карт, подлежащая удалению из БД.</param>
        /// <param name="token">Токен отмены запроса.</param>
        /// <returns>Успешность операции.</returns>
        public async Task<bool> DeleteCards(List<int> cardsIds, CancellationToken token = default)
        {
            NotifyMessage = string.Empty;

            bool resultDeleteTransactions = true;
            bool resultDeleteAlternativeNumbers = true;
            bool resultDeleteEvents = true;
            bool resultDeleteCards = true;

            if ((cardsIds?.Count ?? 0) == 0)
            {
                NotifyMessage = $"Коллекция карт, подлежащая удалению из БД, пустая !";
                return false;
            }

            // 1 - Delete OperationList
            var transToDel = await GetTransactions(cardsIds, token);

            if ((transToDel?.Count ?? 0) > 0)
            {
                resultDeleteTransactions = DeleteFuelTransactions(transToDel);
            }

            // 2 - Delete CardsAlternativeNumbers
            var alternativeNumbersToDel = await GetCardsAlternativeNumbers(cardsIds, token);

            if ((alternativeNumbersToDel?.Count ?? 0) > 0)
            {
                resultDeleteAlternativeNumbers = DeleteCardsAlternativeNumbers(alternativeNumbersToDel?.Select(alternativeNumber => alternativeNumber.Id)?.ToList() ?? new());
            }

            // 3 - Delete CardsEvents
            var eventsToDel = await GetCardsEvents(cardsIds, token);

            if ((eventsToDel?.Count ?? 0) > 0)
            {
                resultDeleteEvents = DeleteFuelCardEvent(eventsToDel?.Select(eventToDel => eventToDel.Id)?.ToList() ?? new());
            }

            // 4 - Delete FuelCards
            resultDeleteCards = DeleteCardsInner(cardsIds);

            NotifyMessage += $"Всего было удалено из БД: " +
                       $"транзакции - {transToDel?.Count ?? 0} шт., " +
                       $"альтернативные номера карт - {alternativeNumbersToDel?.Count ?? 0} шт., " +
                       $"события по картам - {eventsToDel?.Count ?? 0} шт., " +
                       $"карты - {cardsIds?.Count ?? 0} шт. ";

            return resultDeleteTransactions
                   && resultDeleteAlternativeNumbers
                   && resultDeleteEvents
                   && resultDeleteCards;
        }

        /// <summary>
        /// Метод для удаления из БД коллекции альтернативных номеров топливных карт.
        /// </summary>
        /// <param name="alternativeNumbersIds">Коллекция идентификаторов альтернативных номеров топливных карт, подлежащих удалению из БД.</param>
        /// <returns>Успешность операции.</returns>
        public bool DeleteCardsAlternativeNumbers(List<int> alternativeNumbersIds)
        {
            var result = fuelRepository.DeleteCardsAlternativeNumbers(alternativeNumbersIds);
            NotifyMessage += fuelRepository.NotifyMessage ?? string.Empty;
            return result;
        }

        public bool DeleteNotFoundCards(List<int> notFoundCardsIds, CancellationToken token = default)
        {
            var result = fuelRepository.DeleteNotFoundCards(notFoundCardsIds);
            NotifyMessage += fuelRepository.NotifyMessage ?? string.Empty;
            return result;
        }
        #endregion

        #region Shared
        public async Task<List<FuelCardsEventResponse>> GetCardsEvents(int cardId, CancellationToken token = default)
        {
            var cardsEvents = await fuelRepository.FindCardsEvents(cardsEvent => cardsEvent.CardId == cardId, token);
            return cardsEvents?.Select(cardsEvents => mapper?.Map<FuelCardsEventResponse>(cardsEvents))?.ToList() ?? new();
        }

        protected bool DeleteFuelCardEvent(List<int> cardEventsIds)
        {
            var result = fuelRepository.DeleteFuelCardEvent(cardEventsIds);
            NotifyMessage += fuelRepository.NotifyMessage ?? string.Empty;
            return result;
        }

        /// <summary>
        /// Метод для получения последнего по времени события изменения статуса топливной карты.
        /// </summary>
        /// <param name="cardId">Идентификатор топливной карты.</param>
        /// <returns>Последнее по времени событие изменения статуса топливной карты.</returns>
        protected async Task<(FuelCardsEvent firstEvent, FuelCardsEvent secondEvent)> GetFirstAndSecondEvents(int cardId, CancellationToken token = default)
        {
            var allEvents = (await fuelRepository.FindCardsEvents(ev => ev.CardId == cardId, token))?
                .OrderBy(card => card.StartDate.Date)?.ThenBy(card => card.Id)?.ToList() ?? new();

            var firstEventValue = allEvents?.FirstOrDefault() ?? null;

            var secondEventValue = allEvents?.Count > 1
                ? allEvents?.ElementAt(1)
                : null;

            return (firstEventValue, secondEventValue);
        }

        /// <summary>
        /// Метод для получения последнего по времени события изменения статуса топливной карты.
        /// <param Name="cardId">Идентификатор топливной карты</param>
        /// <returns>Последнее по времени событие изменения статуса топливной карты</returns>
        /// </summary>
        protected async Task<FuelCardsEvent> GetLastCardsEvent(int cardId, CancellationToken token = default)
        {
            return (await GetFuelCardsEventsOrderedDesc(cardId, token)).FirstOrDefault() ?? null;
        }

        protected async Task<List<FuelCardsEvent>> GetFuelCardsEventsOrdered(int cardId, CancellationToken token = default)
        {
            return (await fuelRepository.FindCardsEvents(ev => ev.CardId == cardId, token))?
                .OrderBy(cardsEvent => cardsEvent.StartDate.Date)?
                .ThenBy(cardsEvent => cardsEvent.Id)?.ToList() ?? new();
        }

        protected async Task<List<FuelCardsEvent>> GetFuelCardsEventsOrderedDesc(int cardId, CancellationToken token = default)
        {
            return (await fuelRepository.FindCardsEvents(ev => ev.CardId == cardId, token))?
                .OrderByDescending(cardsEvent => cardsEvent.StartDate.Date)?
                .ThenByDescending(cardsEvent => cardsEvent.Id)?.ToList() ?? new();
        }

        /// <summary>
        /// Метод для редактирования события изменения статуса топливной карты.
        /// </summary>
        /// <param name="cardsEvent">Событие изменения статуса топливной карты, подлежащее редактированию.</param>
        /// <returns>Отредактированное событие изменения статуса топливной карты.</returns>
        protected FuelCardsEvent UpdateCardsEvent(FuelCardsEvent cardsEvent)
        {
            var result = fuelRepository.UpdateCardsEvent(cardsEvent) ?? new();
            NotifyMessage += fuelRepository.NotifyMessage ?? string.Empty;
            return result;
        }

        /// <summary>
        /// Метод для сохранения в бд нового события изменения статуса топливной карты.
        /// </summary>
        /// <param name="fuelCardEvent">Событие изменения статуса топливной карты.</param>
        /// <returns>Сохраненное в бд событие.</returns>
        protected FuelCardsEvent CreateCardsEvent(FuelCardsEvent fuelCardEvent)
        {
            var result = fuelRepository.CreateCardsEvent(fuelCardEvent) ?? new();
            NotifyMessage += fuelRepository.NotifyMessage ?? string.Empty;
            return result;
        }
        #endregion

        #region Additional
        public async Task<List<FuelCardsEvent>> GetCardsEvents(List<int> cardsIds, CancellationToken token = default)
        {
            return (await fuelRepository.FindCardsEvents(cardsEvents => cardsIds.Contains(cardsEvents.CardId), token)) ?? new();
        }

        private async Task<List<FuelCardsAlternativeNumber>> GetCardsAlternativeNumbers(List<int> cardsIds, CancellationToken token = default)
        {
            return (await fuelRepository.FindCardsAlternativeNumbers(alternativeNumber => cardsIds.Contains(alternativeNumber.CardId), token)) ?? new();
        }

        private bool DeleteCardsInner(List<int> cardsIds)
        {
            var result = fuelRepository.DeleteCards(cardsIds);
            NotifyMessage += fuelRepository.NotifyMessage ?? string.Empty;
            return result;
        }

        private bool DeleteFuelTransactions(List<FuelTransaction> transactions)
        {
            var result = fuelRepository.DeleteTransactions(transactions?.Select(transaction => transaction.Id)?.ToList() ?? new());
            NotifyMessage += fuelRepository.NotifyMessage ?? string.Empty;
            return result;
        }

        private FuelCard CreateCard(FuelCard card)
        {
            card.ExpirationYear = card.ExpirationDate.HasValue ? card.ExpirationDate.Value.Year : default;
            card.ExpirationMonth = card.ExpirationDate.HasValue ? card.ExpirationDate.Value.Month : default;
            var result = fuelRepository.CreateCard(card);
            NotifyMessage += fuelRepository.NotifyMessage ?? string.Empty;
            return result;
        }

        private async Task<List<FuelTransaction>> GetTransactions(List<int> cardsIds, CancellationToken token = default)
        {
            return await fuelRepository.FindRangeTransactions(transaction => cardsIds.Contains(transaction.CardId), token);
        }

        /// <summary>
        /// Метод для исключения из исходной коллекции тех топливных карт, которые уже есть в БД.
        /// </summary>
        /// <param name="cards">Коллекция топливных карт, из которой будут исключены те карты, которые уже есть в БД.</param>
        /// <returns>Коллекция карт после исключения из нее карт, которые уже есть в БД.</returns>
        private async Task<List<FuelCard>> TrimExistingFuelCards(List<FuelCardRequest> cards)
        {
            List<FuelCard> result = new();

            var cardsToSave = cards?.Select(cardItem => mapper?.Map<FuelCard>(cardItem))?.ToList() ?? new();

            foreach (var cardToSave in cardsToSave)
            {
                // Если карта с таким номером уже есть в БД - не включать ее в результирующую коллекцию
                try
                {
                    var isExist = await CheckIsInstanceExist(cardToSave);

                    if (isExist)
                    {
                        continue;
                    }
                    else
                    {
                        if (!result.Any(card => string.Compare(card.Number, cardToSave.Number, StringComparison.InvariantCultureIgnoreCase) == 0)) // проверка внутри коллекции на идентичные номера
                        {
                            result.Add(cardToSave);
                        }
                        else
                        {
                            ExistedInstances?.Add(cardToSave);
                        }
                    }
                }
                catch(Exception exc)
                {
                    exc.LogError(GetType().FullName, nameof(TrimExistingFuelCards));
                    NotifyMessage += $"Записи не подлежит удалению из системы, так как участвует в других записях ! " +
                              $"Можете обратиться в службу поддержки и оставить заявку на удаление ! " +
                              $"{exc.GetExeceptionMessages()} ! ";
                    throw;
                }
            }

            return result;
        }

        /// <summary>
        /// Метод для определения,необходимо создавать новое событие изменения статуса топливной карты или нет.
        /// </summary>
        /// <param name="updatedCard">Отредактированная на клиенте топливная карта, которая подлежит сохранению в БД.</param>
        /// <returns>Истина, если требуется сохранение в БД нового события / событий изменения статуса топливной карты.</returns>
        private static bool NeedUpdateLastFuelCardEvent(FuelCard updatedCard, FuelCard dbCard)
        {
            if (updatedCard != null)
            {
                if (updatedCard.IsArchived != dbCard.IsArchived) // если карта изымается из архива / или добавляется в архив
                {
                    return true;
                }
                if (updatedCard.DivisionID != dbCard.DivisionID) // если карта перемещена с одного подразделения на другое
                {
                    return true;
                }
                else if (updatedCard.CarId != null && dbCard.CarId == null) // если привязка к авто удалена - карта отправлена на Склад
                {
                    return true;
                }
                else if (updatedCard.CarId == null && dbCard.CarId != null) // если привязка к авто добавлена - карта со склада перемещена на Тягач
                {
                    return true;
                }
                else if (updatedCard.CarId != dbCard.CarId) // если карта с одного Тягача перемещена на другой Тягач
                {
                    return true;
                }
                else if (updatedCard.EmployeeId != null && dbCard.EmployeeId == null) // если привязка к водителю удалена - карта отправлена на Склад
                {
                    return true;
                }
                else if (updatedCard.EmployeeId == null && dbCard.EmployeeId != null) // если привязка к водителю добавлена - карта со склада перемещена на Водителя
                {
                    return true;
                }
                else if (updatedCard.EmployeeId != dbCard.EmployeeId) // если карта с одного Водителя перемещена на другого Водителя
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        protected async Task<List<KitEventTypeResponse>> GetKitEventTypes(CancellationToken token = default)
        {
            var eventsTypesDb = await fuelRepository.GetKitEventTypes(token);
            return eventsTypesDb?.Select(kit => mapper.Map<KitEventTypeResponse>(kit))?.ToList() ?? new();
        }

        /// <summary>
        /// Вспомогательный метод для создания экземпляра события изменения статуса топливной карты и сохранения его в БД.
        /// </summary>
        /// <param name="userName">Имя пользователя, производящего операцию.</param>
        /// <param name="card">Топливная карта.</param>
        private async Task CreateFuelCardEvent(string userName, FuelCard card, CancellationToken token = default)
        {
            if (card is null) return;

            var eventsTypes = await GetKitEventTypes(token);
            if (!eventsTypes.Any()) return; // если типы событий пустые - будет ошибка внешнего ключа, поэтому возврат

            if (card.IsArchived) // 1 - Если установлен флаг в архиве - то событие АРХИВ
            {
                CreateCardsEvent(
                    new FuelCardsEvent
                    {
                        CardId = card.Id,
                        CarId = null,
                        EmployeeId = null,
                        DivisionID = card.DivisionID,
                        EventTypeId = EventsTypeName.Архив.GetEventTypeId(eventsTypes),
                        StartDate = DateTime.Now.Date,
                        FinishDate = null,
                        Comment = string.Empty,
                        ModifiedOn = DateTime.Today,
                        ModifiedBy = userName,
                    });
            }
            else if (card.CarId.HasValue) // 2 - Если есть привязка к авто - то событие ТЯГАЧ
            {
                CreateCardsEvent(
                    new FuelCardsEvent
                    {
                        CardId = card.Id,
                        CarId = card.CarId,
                        EmployeeId = null,
                        DivisionID = card.DivisionID,
                        EventTypeId = EventsTypeName.Тягач.GetEventTypeId(eventsTypes),
                        StartDate = (card.IssueDate ?? card.ReceiveDate ?? DateTime.Now).Date,  // дата события ТЯГАЧ - это дата ввода в экспл.
                        FinishDate = null,
                        Comment = string.Empty,
                        ModifiedOn = DateTime.Today,
                        ModifiedBy = userName,
                    });
            }
            else if (card.EmployeeId.HasValue) // 3 - Если есть привязка к водителю - то событие ВОДИТЕЛЬ
            {
                CreateCardsEvent(
                    new FuelCardsEvent
                    {
                        CardId = card.Id,
                        CarId = null,
                        EmployeeId = card.EmployeeId,
                        DivisionID = card.DivisionID,
                        EventTypeId = EventsTypeName.Водитель.GetEventTypeId(eventsTypes),
                        StartDate = (card.IssueDate ?? card.ReceiveDate ?? DateTime.Now).Date,  // дата события ВОДИТЕЛЬ - это дата ввода в экспл.
                        FinishDate = null,
                        Comment = string.Empty,
                        ModifiedOn = DateTime.Today,
                        ModifiedBy = userName,
                    });
            }
            else if ((!card.EmployeeId.HasValue || card.EmployeeId.Value == 0)
                && (!card.CarId.HasValue || card.CarId.Value == 0)) // 4 - Если нет привязки к авто / водителю - то событие СКЛАД
            {
                CreateCardsEvent(
                    new FuelCardsEvent
                    {
                        CardId = card.Id,
                        CarId = null,
                        EmployeeId = null,
                        DivisionID = card.DivisionID,
                        EventTypeId = EventsTypeName.Склад.GetEventTypeId(eventsTypes),
                        StartDate = (card.ReceiveDate ?? card.IssueDate ?? DateTime.Now).Date,
                        FinishDate = null,
                        Comment = string.Empty,
                        ModifiedOn = DateTime.Today,
                        ModifiedBy = userName,
                    });
            }
        }

        /// <summary>
        /// Вспомогательный метод для установки в последнем событии по карте даты окончания события и даты изменения события равными текущему дню.
        /// </summary>
        /// <param name="lastEvent">Последнее событие по карте.</param>
        /// <param name="newDate">Новая дата.</param>
        /// <param name="userName">Имя пользователя.</param>
        /// <returns>Сохраненное в БД измененное событие.</returns>
        protected FuelCardsEvent SetFinishDateInPreviousEvent(FuelCardsEvent lastEvent, DateTime newDate, string userName)
        {
            // Отредактировать последнее событие по этой карте
            if (lastEvent != null)
            {
                lastEvent.FinishDate = newDate.Date; // проставить дату окончания события
                lastEvent.ModifiedOn = DateTime.Now; // отредактировать дату изменения события
                lastEvent.ModifiedBy = userName; // отредактировать пользователя, изменившего событие
                return UpdateCardsEvent(lastEvent); // сохранить изменения события в БД
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}
