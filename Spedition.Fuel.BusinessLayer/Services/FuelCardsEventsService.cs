using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Spedition.Fuel.BusinessLayer.Enums;
using Spedition.Fuel.BusinessLayer.Helpers;
using Spedition.Fuel.BusinessLayer.Services.BaseServices;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.BusinessLayer.Services
{
    public class FuelCardsEventsService : FuelCardsService, IFuelCardsEventsService
    {
        public FuelCardsEventsService(
            FuelCardRepository fuelRepository,
            IWebHostEnvironment env,
            IConfiguration configuration,
            IMapper mapper,
            ITruckService truckService,
            IDivisionService divisionService,
            IEventsTypeService eventsTypeService,
            IEmployeeService employeeService)
            : base(fuelRepository, env, configuration, mapper, truckService, divisionService, eventsTypeService, employeeService)
        {
        }

        #region Get
        public async Task<FuelCardsEventResponse> GetCardsEventPrevious(int eventId, CancellationToken token = default)
        {
            FuelCardsEvent previousEvent = await GetPreviousInner(eventId, token);

            return previousEvent is not null ? mapper?.Map<FuelCardsEventResponse>(previousEvent) : null;
        }

        public async Task<FuelCardsEventResponse> GetCardsEventNext(int eventId, CancellationToken token = default)
        {
            var nextEvent = await GetNextInner(eventId, token);

            return nextEvent is not null ? mapper?.Map<FuelCardsEventResponse>(nextEvent) : null;
        }

        #endregion

        #region Update

        /// <summary>
        /// Метод для редактирования события изменения статуса топливной карты.
        /// </summary>
        /// <param name="cardsEvent">Событие изменения статуса топливной карты, подлежащее редактированию.</param>
        /// <returns>Отредактированное событие изменения статуса топливной карты.</returns>
        public async Task<FuelCardsEventResponse> UpdateCardsEvent(FuelCardsEventRequest cardsEvent, string userName, CancellationToken token = default)
        {
            if ((cardsEvent?.Id ?? 0) == 0)
            {
                NotifyMessage += "Событие по топливной карте имеет пустое значение. ";
                NotifyMessage.LogError(GetType().Name, nameof(UpdateCardsEvent));
                return default;
            }

            NotifyMessage = string.Empty;
            var cardsEventToUpdate = mapper?.Map<FuelCardsEvent>(cardsEvent);
            var isLastEvent = (await GetLastCardsEvent(cardsEventToUpdate.CardId, token))?.Id == cardsEventToUpdate.Id;
            var isStartDateChanged = await IsCardsEventsStartDateChanged(cardsEventToUpdate);
            var isFinishDateChanged = await IsCardsEventsFinishDateChanged(cardsEventToUpdate);
            var firstAndSecondEvents = await GetFirstAndSecondEvents(cardsEventToUpdate.CardId, token);
            var isFirstEvent = cardsEventToUpdate.Id == firstAndSecondEvents.firstEvent.Id;
            var previousEvent = await GetPreviousInner(cardsEventToUpdate.Id, token); // предыдущее событие
            var nextEvent = await GetNextInner(cardsEventToUpdate.Id, token); // последущее событие
            //var eventsTypes = await eventsTypeService.GetDivisions();
            var eventsTypes = await GetKitEventTypes(token);
            var card = await GetCardInner(cardsEventToUpdate.CardId) ?? null;

            // Валидация
            if (cardsEventToUpdate.FinishDate.HasValue && cardsEventToUpdate.FinishDate.Value.Date < cardsEventToUpdate.StartDate.Date)
            {
                NotifyMessage += "Событие не может заканчиваться ранее даты его начала. ";
                NotifyMessage.LogError(GetType().Name, nameof(UpdateCardsEvent));
                return default;
            }

            if (previousEvent != null && cardsEventToUpdate.StartDate.Date < previousEvent.StartDate.Date)
            {
                NotifyMessage += "Текущее событие не может начинаться ранее предыдущего события. ";
                NotifyMessage.LogError(GetType().Name, nameof(UpdateCardsEvent));
                return default;
            }

            if (nextEvent != null && cardsEventToUpdate.StartDate.Date > nextEvent.StartDate.Date)
            {
                NotifyMessage += "Текущее событие не может начинаться позднее даты начала последующего события. ";
                NotifyMessage.LogError(GetType().Name, nameof(UpdateCardsEvent));
                return default;
            }

            // 1 - Обновить предыдущее событие
            if (isStartDateChanged)
            {
                SetFinishDateInPreviousEvent(previousEvent, cardsEventToUpdate?.StartDate.Date ?? previousEvent.FinishDate ?? DateTime.Today, userName);
            }

            // 2 - Обновить последущее событие
            if (isFinishDateChanged && (nextEvent?.Id ?? 0) != 0)
            {
                nextEvent.StartDate = cardsEventToUpdate?.FinishDate?.Date ?? nextEvent.StartDate.Date;
                nextEvent.ModifiedOn = DateTime.Today;
                nextEvent.ModifiedBy = userName;
                nextEvent = UpdateCardsEvent(nextEvent);
            }

            // 3 - Обновить текущее событие
            cardsEventToUpdate = UpdateCardsEvent(cardsEventToUpdate);

            // 4 - Обновить топливную карту
            // 4.1 - в части привязки к подразделению
            if (isLastEvent && cardsEventToUpdate.DivisionID.HasValue)
            {
                card.DivisionID = cardsEventToUpdate.DivisionID.Value;
            }

            // 4.2 - в части даты ее ввода в эксплуатацию, если событие является первым в цепочке событий по данной карте
            if (isFirstEvent
                && card != null
                && isStartDateChanged
                && cardsEventToUpdate.EventTypeId != EventsTypeName.Архив.GetEventTypeId(eventsTypes)
                && cardsEventToUpdate.EventTypeId != EventsTypeName.Склад.GetEventTypeId(eventsTypes))
            {
                card.IssueDate = cardsEventToUpdate.StartDate.Date;
            }

            // 4.3 - в части привязки к обьекту, если событие является последним в цепочке событий по данной карте
            if (isLastEvent)
            {
                if (cardsEventToUpdate.EventTypeId == EventsTypeName.Архив.GetEventTypeId(eventsTypes))
                {
                    card.IsArchived = true;
                    card.CarId = null;
                    card.EmployeeId = null;
                }
                else if (cardsEventToUpdate.EventTypeId == EventsTypeName.Склад.GetEventTypeId(eventsTypes))
                {
                    card.IsArchived = false;
                    card.CarId = null;
                    card.EmployeeId = null;
                }
                else
                {
                    card.IsArchived = false;
                    card.CarId = cardsEventToUpdate.CarId;
                    card.EmployeeId = cardsEventToUpdate.EmployeeId;
                }
            }

            // 5 - Сохранить измененную карту в БД
            UpdateCard(card);
            return mapper?.Map<FuelCardsEventResponse>(cardsEventToUpdate);
        }
        #endregion

        #region Create
        public async Task<FuelCardsEventResponse> CreateCardsEvent(FuelCardsEventRequest cardsEvent, string userName, CancellationToken token = default)
        {
            NotifyMessage = string.Empty;
            var cardsEventToCreate = mapper?.Map<FuelCardsEvent>(cardsEvent);
            var eventsTypes = await GetKitEventTypes(token);

            var isFirstEvent = ((await GetCardsEvents(cardsEventToCreate.CardId, token))?.Count ?? 0) == 0;

            // 1 - Обновить предыдущее событие (оно же последнее в цепочке событий), если такое имеется
            var previousEvent = await GetLastCardsEvent(cardsEventToCreate.CardId, token);
            SetFinishDateInPreviousEvent(previousEvent, cardsEventToCreate?.StartDate.Date ?? previousEvent.FinishDate ?? DateTime.Today, userName);

            // 2 - Сохранить в БД новое созданное событие
            cardsEventToCreate = CreateCardsEvent(cardsEventToCreate);

            // 3 - Обновить топливную карту - т.к. событие является последним в цепочке событий по данной карте
            var card = await GetCardInner(cardsEventToCreate.CardId, token);

            // 3.1 - в части даты ее ввода в экспл-ю, если это первая привязка к объекту
            if (card != null
                && !card.IssueDate.HasValue
                && cardsEventToCreate.EventTypeId != EventsTypeName.Архив.GetEventTypeId(eventsTypes)
                && cardsEventToCreate.EventTypeId != EventsTypeName.Склад.GetEventTypeId(eventsTypes))
            {
                card.IssueDate = cardsEventToCreate.StartDate.Date;
            }

            // 3.2 - в части привязки к подразделению
            if (cardsEventToCreate.DivisionID.HasValue)
            {
                card.DivisionID = cardsEventToCreate.DivisionID.Value;
            }

            // 3.3 - в части привязки к авто / водителю или отношению к архивной
            if (cardsEventToCreate.EventTypeId == EventsTypeName.Архив.GetEventTypeId(eventsTypes))
            {
                card.IsArchived = true;
                card.CarId = null;
                card.EmployeeId = null;
            }
            else if (cardsEventToCreate.EventTypeId == EventsTypeName.Склад.GetEventTypeId(eventsTypes))
            {
                card.IsArchived = false;
                card.CarId = null;
                card.EmployeeId = null;
            }
            else
            {
                card.IsArchived = false;
                card.CarId = cardsEvent.CarId;
                card.EmployeeId = cardsEvent.EmployeeId;
            }

            // 4 -  Сохранить измененную карту в БД
            UpdateCard(card);
            return mapper.Map<FuelCardsEventResponse>(cardsEventToCreate);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Метод для удаления из БД последнего в цепочке событий события.
        /// </summary>
        /// <param name="cardsEvent">Событие, подлежащее удалению из БД.</param>
        /// <returns>Истина, если операуия успешно завершена.</returns>
        public async Task<bool> DeleteLastCardsEvent(FuelCardsEventRequest cardsEvent, CancellationToken token = default)
        {
            NotifyMessage = string.Empty;
            bool result = default;
            var isLastEvent = ((await GetLastCardsEvent(cardsEvent?.CardId ?? 0, token))?.Id ?? 0) == cardsEvent.Id;
            var firstAndSecondEvents = await GetFirstAndSecondEvents(cardsEvent.CardId, token);
            var isFirstEvent = cardsEvent.Id == firstAndSecondEvents.firstEvent.Id;
            var isSecondEvent = cardsEvent.Id == firstAndSecondEvents.secondEvent.Id;
            var eventsTypes = await eventsTypeService.Get();

            var warningMessage = (string slice) => $"Событие не может быть удалено из системы, потому что не является {slice} в цепочке событий";

            if (!isLastEvent)
            {
                NotifyMessage += warningMessage("последним");
                return result;
            }

            if (isFirstEvent)
            {
                NotifyMessage += warningMessage("единственным");
                return result;
            }

            // 1 - Удалить последнее событие
            result = DeleteFuelCardEvents(cardsEvent?.Id ?? 0);

            if (!result)
            {
                NotifyMessage += "Событие не было удалено из системы! ";
                NotifyMessage.LogError(GetType().Name, nameof(DeleteLastCardsEvent));
                return result;
            }

            // 2 - Отредактировать предпоследнее событие в части даты окончания события - сделать открытой датой (=null)
            var card = await GetCardInner(cardsEvent.CardId);
            var newLastEvent = await GetLastCardsEvent(cardsEvent?.CardId ?? 0, token);
            newLastEvent.FinishDate = null;
            newLastEvent = UpdateCardsEvent(newLastEvent);

            // 3 - Отредактировать топливную карту в соответствии с событием которое заняло место последнего в цепочке событий после удаления крайнего
            // 3.1 - в части подр-я
            card.DivisionID = newLastEvent.DivisionID.Value;

            // 3.2 - в части привязки к объекту
            if (newLastEvent.EventTypeId == EventsTypeName.Архив.GetEventTypeId(eventsTypes))
            {
                card.IsArchived = true;
                card.CarId = null;
                card.EmployeeId = null;
            }
            else if (newLastEvent.EventTypeId == EventsTypeName.Склад.GetEventTypeId(eventsTypes))
            {
                card.IsArchived = false;
                card.CarId = null;
                card.EmployeeId = null;
            }
            else
            {
                card.IsArchived = false;
                card.CarId = newLastEvent.CarId;
                card.EmployeeId = newLastEvent.EmployeeId;
            }

            // 3.3 - Сохранить изменения в БД
            UpdateCard(card);

            return result;
        }

        private bool DeleteFuelCardEvents(int cardEventsId)
        {
            return fuelRepository.DeleteFuelCardEvent(new List<int> { cardEventsId });
        }

        private bool DeleteFuelCardEvents(IEnumerable<int> cardEventsIds)
        {
            return fuelRepository.DeleteFuelCardEvent(cardEventsIds?.ToList() ?? new());
        }
            #endregion

        #region Additional
        
        protected async Task<FuelCardsEvent> GetFirstEvent(int cardId, CancellationToken token = default)
        {
            return (await GetFuelCardsEventsOrdered(cardId, token))?.FirstOrDefault();
        }

        private async Task<FuelCardsEvent> GetNextInner(int eventId, CancellationToken token = default)
        {
            FuelCardsEvent nextEvent = null;

            // идентификатор топливной карты, к которой относится текущее событие
            var cardId = (await fuelRepository.GetCardsEvent(eventId))?.CardId ?? 0;

            // все события по карте
            var cardsEvents = await fuelRepository.FindCardsEvents(cardsEvent => cardsEvent.CardId == cardId, token);

            // все события по карте, отсортированные по возрастанию (ВАЖНО!) по дате, а затем по идентификатору
            var fuelCardsEvents = cardsEvents?
                .OrderBy(cardsEvent => cardsEvent.StartDate.Date)?
                .ThenBy(cardsEvent => cardsEvent.Id)?.ToList();

            var indexCurrEvent = fuelCardsEvents?.FindIndex(cardsEvent => cardsEvent.Id == eventId);

            // если событие не последнее в цепочке событий
            if (indexCurrEvent.HasValue && indexCurrEvent != cardsEvents.Count - 1)
            {
                nextEvent = fuelCardsEvents.ElementAt(indexCurrEvent.Value + 1);
            }

            return nextEvent;
        }

        /// <summary>
        /// Метод для определения, изменилась ли дата начала события(влияет на то, нужно ли редактировать предыдущее событие изменения статуса топливной карты или нет).
        /// </summary>
        /// <param name="eventChanged">Отредактированное на клиенте событие, которое подлежит сохранению в БД.</param>
        /// <returns>Истина, если дата начала события топливной карты была изменена.</returns>
        private async Task<bool> IsCardsEventsStartDateChanged(FuelCardsEvent eventChanged)
        {
            var foundEvent = await fuelRepository.GetCardsEvent(eventChanged.Id);

            if ((foundEvent?.Id ?? 0) > 0)
            {
                if (foundEvent?.StartDate.Date != eventChanged?.StartDate.Date)
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

        /// <summary>
        /// Метод для определения, необходимо редактировать последующее событие изменения статуса топливной карты или нет.
        /// </summary>
        /// <param name="eventChanged">Отредактированное на клиенте событие, которое подлежит сохранению в БД.</param>
        /// <returns>Истина, если дата окончания события топливной карты была изменена.</returns>
        private async Task<bool> IsCardsEventsFinishDateChanged(FuelCardsEvent eventChanged)
        {
            var foundEvent = await fuelRepository.GetCardsEvent(eventChanged.Id);

            if ((foundEvent?.Id ?? 0) > 0)
            {
                if (foundEvent?.FinishDate?.Date != eventChanged?.FinishDate?.Date)
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

        private async Task<FuelCardsEvent> GetPreviousInner(int eventId, CancellationToken token = default)
        {
            FuelCardsEvent previousEvent = null;

            // идентификатор топливной карты, к которой относится текущее событие
            var cardId = (await fuelRepository.GetCardsEvent(eventId))?.CardId ?? 0;

            // все события по карте
            var cardsEvents = await fuelRepository.FindCardsEvents(cardsEvent => cardsEvent.CardId == cardId, token);

            // все события по карте, отсортированные по возрастанию (ВАЖНО!) по дате, а затем по идентификатору
            var fuelCardsEvents = cardsEvents?
                .OrderBy(cardsEvent => cardsEvent.StartDate.Date)?
                .ThenBy(cardsEvent => cardsEvent.Id)?.ToList();

            var indexCurrentEvent = fuelCardsEvents?.FindIndex(cardsEvent => cardsEvent.Id == eventId);

            // если событие не первое в цепочке событий
            if (indexCurrentEvent.HasValue && indexCurrentEvent != 0)
            {
                previousEvent = fuelCardsEvents.ElementAt(indexCurrentEvent.Value - 1);
            }

            return previousEvent;
        }
        #endregion
    }
}