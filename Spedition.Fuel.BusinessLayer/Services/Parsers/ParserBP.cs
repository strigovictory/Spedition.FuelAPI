using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Spedition.Fuel.BusinessLayer.Enums;
using Spedition.Fuel.BusinessLayer.Enums.ReportsHeaders;
using Spedition.Fuel.BusinessLayer.Helpers;
using Spedition.Fuel.BusinessLayer.Models;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.Parsers.ParserBaseServices;
using Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.Fuel.BusinessLayer.Services.Parsers;

public class ParserBP : FuelParserBase<FuelTransactionBP, FuelTransactionShortResponse, NotParsedTransaction, FuelTransaction>
{
    public ParserBP(
    IWebHostEnvironment env,
    FuelRepositories fuelRepositories,
    IConfiguration configuration,
    IMapper mapper,
    ICountryService countryService,
    ICurrencyService currencyService,
    IEventsTypeService eventsTypeService,
    IDivisionService divisionService,
    ITruckService carService)
    : base(env, fuelRepositories, configuration, mapper, countryService, currencyService, eventsTypeService, divisionService, carService, OuterLibrary.NPOI)
    {
        ProvidersId = new List<int> { 11 };
    }

    #region Methods
    protected override async Task InitSecondaryCollections()
    {
        FuelCardsCountries = await fuelRepositories?.FuelCardsCountries?.GetAsync();
    }

    protected override async Task<int?> SearchCard(string number)
    {
        if (string.IsNullOrEmpty(number) || string.IsNullOrWhiteSpace(number))
        {
            return null;
        }

        FuelCard foundCard = null;
        var unUsedChars = new List<char> { '.', '-', '/', '*', '_', ' ' };
        var charZero = new List<char> { '0' };

        Predicate<FuelCard> cardIsValid = (card) =>
        card != null
        && card?.Id > 0
        && card?.Number?.TrimWhiteSpaces()?.Length > 0
        && card?.Number?.TrimSomeChars(unUsedChars)?.Length > 1
        && card?.Number?.TrimSomeChars(charZero)?.Length > 1;

        // 1-й способ
        foundCard = ProvidersCards?.FirstOrDefault(
            card => !string.IsNullOrEmpty(card.Number)
            && !string.IsNullOrWhiteSpace(card.Number)
            && card.Number.Equals(number));

        if (cardIsValid(foundCard))
        {
            return foundCard?.Id;
        }

        // 2-й способ
        var resSql = await SearchCardByQuery(number);
        if (resSql)
        {
            return FoundCardsIds?.FirstOrDefault() ?? null;
        }

        // 3-й способ
        var cardsTrimServicesChars = number.TrimSomeChars(unUsedChars) ?? string.Empty;

        if (string.IsNullOrEmpty(cardsTrimServicesChars))
        {
            return null;
        }

        foundCard = ProvidersCards?.FirstOrDefault(
            card => !string.IsNullOrEmpty(card.Number?.TrimSomeChars(unUsedChars) ?? string.Empty)
            && card.Number.TrimSomeChars(unUsedChars).Equals(cardsTrimServicesChars));

        if (cardIsValid(foundCard))
        {
            return foundCard?.Id;
        }

        // 4-й способ
        var cardsTrimServicesCharsEngl = cardsTrimServicesChars?.ConvertCyrillicToLatin() ?? string.Empty;

        if (string.IsNullOrEmpty(cardsTrimServicesCharsEngl))
        {
            return null;
        }

        foundCard = ProvidersCards?.FirstOrDefault(
            card => !string.IsNullOrEmpty(card.Number.TrimSomeChars(unUsedChars)?.ConvertCyrillicToLatin() ?? string.Empty)
            && card.Number.TrimSomeChars(unUsedChars).ConvertCyrillicToLatin().Equals(cardsTrimServicesCharsEngl));

        if (cardIsValid(foundCard))
        {
            return foundCard?.Id;
        }

        // 5-й способ
        var cardsNumTrimZero = cardsTrimServicesCharsEngl?.TrimSomeCharsExceptLastSixth(charZero) ?? string.Empty;

        if (string.IsNullOrEmpty(cardsNumTrimZero))
        {
            return null;
        }

        foundCard = ProvidersCards?.FirstOrDefault(
            card => !string.IsNullOrEmpty(card.Number.TrimSomeChars(unUsedChars)?.ConvertCyrillicToLatin()?.TrimSomeCharsExceptLastSixth(charZero) ?? string.Empty)
            && (card.Number.TrimSomeChars(unUsedChars)?.ConvertCyrillicToLatin()?.TrimSomeCharsExceptLastSixth(charZero)?.Equals(cardsNumTrimZero) ?? false));

        if (cardIsValid(foundCard))
        {
            return foundCard?.Id;
        }

        // 6-й способ - Поиск в альтернативных номерах
        return SearchCardInAlternativeNumbers(number);
    }

    protected override int? SearchCardInAlternativeNumbers(string number)
    {
        if (string.IsNullOrEmpty(number) || string.IsNullOrWhiteSpace(number))
        {
            return null;
        }

        return AlternativeNumbers?.FirstOrDefault(alterNumber => alterNumber.Number.ToLower().Equals(number.ToLower()))?.CardId;
    }

    protected override async Task<FuelTransaction> MappingParsedToDB(IParsedItem parsedReportsItem)
    {
        FuelTransaction dbTransaction = new();

        if (parsedReportsItem == null || parsedReportsItem is not FuelTransactionBP parsedTransaction)
        {
            $"Ошибка - несоответствие транзакции типу {nameof(FuelTransactionBP)} !".LogError(GetType().Name, nameof(MappingParsedToDB));
            return null;
        }

        try
        {
            // 0 - Идентификатор транзакции
            dbTransaction.TransactionID = parsedTransaction.TransactionNumber;

            // 1 - Поставщик топлива «BP»
            dbTransaction.ProviderId = ProviderId;

            // 2 - Разновидность топлива
            dbTransaction.FuelTypeId = GetFuelType(parsedTransaction.FuelType);

            // 3 - Колличество, литров
            var quantity = Math.Round(parsedTransaction.Quantity, 2, MidpointRounding.AwayFromZero);
            dbTransaction.Quantity = parsedTransaction.NumberSign == "0" || string.IsNullOrEmpty(parsedTransaction.NumberSign) ? quantity : Math.Abs(quantity) * (-1);

            // 4 - Цена за литр
            var cost = Math.Round(parsedTransaction.Cost, 3, MidpointRounding.AwayFromZero);
            dbTransaction.Cost = parsedTransaction.NumberSign == "0" || string.IsNullOrEmpty(parsedTransaction.NumberSign) ? cost : Math.Abs(cost) * (-1);

            // 5 - Валюта
            var curr = string.IsNullOrEmpty(parsedTransaction.Currency?.Trim()) ? "pln" : parsedTransaction.Currency;
            dbTransaction.CurrencyId = await GetCurrency(curr);

            // 6 - Общая стоимость
            var totalCost = Math.Round(parsedTransaction.TotalCost, 3, MidpointRounding.AwayFromZero);
            dbTransaction.TotalCost = parsedTransaction.NumberSign == "0" || string.IsNullOrEmpty(parsedTransaction.NumberSign) ? totalCost : Math.Abs(totalCost) * (-1);

            // 7 - По умолчанию ложь
            dbTransaction.IsCheck = false;

            // 8 - Местоположение заправочной станции, где была осуществлена транзакция
            dbTransaction.CountryId = GetCountry(parsedTransaction.Country);

            // 9 - Дата и время транзакции
            var operationDate = GetOperationDate(parsedTransaction);
            if (operationDate != default)
            {
                dbTransaction.OperationDate = operationDate;
            }
            else
            {
                var message = $"{dbTransaction?.ToString() ?? "Транзакция"} не м.б. добавлена в БД, т.к. не удалось определить дату операции";

                notSuccessItems?.Add(
                    new NotSuccessResponseItemDetailed<NotParsedTransaction>(
                        new NotParsedTransaction
                        {
                            CardNumber = string.Empty,
                            CarNumber = string.Empty,
                            NotFuelType = string.Empty,
                            TransactionID = dbTransaction.TransactionID,
                            OperationDate = dbTransaction.OperationDate,
                            Quantity = dbTransaction.Quantity,
                            Cost = dbTransaction.Cost,
                            TotalCost = dbTransaction.TotalCost,
                            IsCheck = dbTransaction.IsCheck,
                            ProviderId = dbTransaction.ProviderId,
                            FuelTypeId = dbTransaction.FuelTypeId,
                            CurrencyId = dbTransaction.CurrencyId,
                            CardId = dbTransaction.CardId,
                            CountryId = dbTransaction.CountryId,
                            DriverReportId = dbTransaction.DriverReportId,
                            IsMonthly = IsMonthly.HasValue && IsMonthly.Value ? true : default,
                            IsDayly = !IsMonthly.HasValue || !IsMonthly.Value ? true : default,
                        }, message));

                return null;
            }

            // 10 - Заправочная карта
            var cardNumFull = string.Concat(
                parsedTransaction.Card_part_1 ?? string.Empty,
                parsedTransaction.Card_part_2 ?? string.Empty,
                parsedTransaction.Card_part_3.AddToSixDigits());

            var foundCardId = await SearchCard(cardNumFull);

            if (!foundCardId.HasValue || foundCardId == 0)
            {
                var message = $"Номер заправочной карты «{cardNumFull ?? string.Empty}» не найден в БД";

                notSuccessItems?.Add(
                    new NotSuccessResponseItemDetailed<NotParsedTransaction>(
                        new NotParsedTransaction
                        {
                            CardNumber = cardNumFull,
                            CarNumber = string.Empty,
                            NotFuelType = string.Empty,
                            TransactionID = dbTransaction.TransactionID,
                            OperationDate = dbTransaction.OperationDate,
                            Quantity = dbTransaction.Quantity,
                            Cost = dbTransaction.Cost,
                            TotalCost = dbTransaction.TotalCost,
                            IsCheck = dbTransaction.IsCheck,
                            ProviderId = dbTransaction.ProviderId,
                            FuelTypeId = dbTransaction.FuelTypeId,
                            CurrencyId = dbTransaction.CurrencyId,
                            CardId = dbTransaction.CardId,
                            CountryId = dbTransaction.CountryId,
                            DriverReportId = dbTransaction.DriverReportId,
                            IsMonthly = IsMonthly.HasValue && IsMonthly.Value ? true : default,
                            IsDayly = !IsMonthly.HasValue || !IsMonthly.Value ? true : default,
                        }, message));

                return null;
            }

            dbTransaction.CardId = foundCardId.Value;
            return dbTransaction;
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(MappingParsedToDB), GetType().FullName);
            throw;
        }
    }

    /// <summary>
    /// Метод для конвертации двух строк, содержащих дату и время транзакции в формат DateTime.
    /// </summary>
    /// <param Name="transaction">Транзакция.</param>
    /// <returns>Дата и время заправки в формате DateTime.</returns>
    private DateTime GetOperationDate(FuelTransactionBP transaction)
    {
        DateTime res = default;
        try
        {
            string date = transaction?.OperationDate?.Trim() ?? string.Empty;
            string time = transaction?.OperationTime?.Trim() ?? string.Empty;

            var yearStr = string.Empty;
            var monthStr = string.Empty;
            var dayStr = string.Empty;
            var hourStr = "0";
            var minuteStr = "0";
            var secondStr = "0";

            // Форматирование даты
            if (date.Length < 8)
            {
                NotifyMessage += $"Ошибка при попытке преобразовать значение «{date} {time}» в формат даты ";
                NotifyMessage.LogError(GetType().Name, nameof(GetOperationDate));
                return res;
            }

            yearStr = date.Substring(0, 4);
            monthStr = date.Substring(4, 2);
            dayStr = date.Substring(6, 2);

            // Форматирование времени
            var timeLength = time.Length;

            var firstColumn = time.IndexOf(':');
            var secondColumn = time.LastIndexOf(':');

            // Если секунды не указаны
            if (firstColumn != -1 && secondColumn != -1)
            {
                if (firstColumn == secondColumn)
                {
                    minuteStr = time.Substring(timeLength - 2, 2);
                    hourStr = time.Substring(timeLength - 5, 2);
                    secondStr = "0";
                }
                else if (firstColumn != secondColumn)
                {
                    secondStr = time.Substring(timeLength - 2, 2);
                    minuteStr = time.Substring(timeLength - 5, 2);
                    hourStr = time.Substring(timeLength - 8, 2);
                }
            }
            else
            {
                secondStr = "0";
                minuteStr = "0";
                hourStr = "0";
            }

            if (int.TryParse(yearStr, out var year)
            && int.TryParse(monthStr, out var month)
            && int.TryParse(dayStr, out var day)
            && int.TryParse(hourStr, out var hour)
            && int.TryParse(minuteStr, out var minute)
            && int.TryParse(secondStr, out var second))
            {
                res = new DateTime(day: day, month: month, year: year, hour: hour, minute: minute, second: second);
            }
            else
            {
                NotifyMessage += $"Ошибка при попытке преобразовать значение {date ?? string.Empty} {time ?? string.Empty} в формат даты ";
                NotifyMessage.LogError(GetType().Name, nameof(GetOperationDate));
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(GetOperationDate), GetType().FullName);
            throw;
        }

        return res;
    }
    #endregion
}
