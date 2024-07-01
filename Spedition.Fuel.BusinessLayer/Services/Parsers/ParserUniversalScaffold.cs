﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Spedition.Fuel.BusinessLayer.Enums;
using Spedition.Fuel.BusinessLayer.Enums.ReportsHeaders;
using Spedition.Fuel.BusinessLayer.Models;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.Parsers.ParserBaseServices;
using Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.Fuel.BusinessLayer.Services.Parsers;

public class ParserUniversalScaffold : FuelParserBase<FuelTransactionUniversalScaffold, FuelTransactionShortResponse, NotParsedTransaction, FuelTransaction>
{
    public ParserUniversalScaffold(
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
        ProvidersId = new List<int> { 15, 18, 22, 23 };
    }

    private static Dictionary<int, int> CurrencyByFuelProvider => new Dictionary<int, int>
    {
        { 15, 4 }, // ЦЕНТРОТЕХ RUB
        { 18, 4 }, // МДК ОЙЛ RUB
        { 22, 4 }, // Грин Ойл
        { 23, 4 }, // ДизельТранс RUB
    };

    protected override async Task<FuelTransaction> MappingParsedToDB(IParsedItem parsedReportsItem)
    {
        FuelTransaction dbTransaction = new();

        if (parsedReportsItem == null || parsedReportsItem is not FuelTransactionUniversalScaffold parsedTransaction)
        {
            $"Ошибка - несоответствие транзакции типу {nameof(FuelTransactionUniversalScaffold)} !".LogError(GetType().Name, nameof(MappingParsedToDB));
            return null;
        }

        try
        {
            // 1 - Идентификатор поставщика топлива
            dbTransaction.ProviderId = ProviderId;

            // 2 - Разновидность топлива
            dbTransaction.FuelTypeId = GetFuelType(parsedTransaction.FuelType);

            // 3 -Заправленное кол-во
            dbTransaction.Quantity = Math.Round(parsedTransaction.Quantity ?? 0, 2, MidpointRounding.AwayFromZero);

            // 4 - Цена за литр
            dbTransaction.Cost = Math.Round(parsedTransaction.Cost ?? 0, 3, MidpointRounding.AwayFromZero);

            // 5 - Идентификатор валюты «Казахстанский тенге»
            dbTransaction.CurrencyId = CurrencyByFuelProvider?.TryGetValue(ProviderId, out var val) ?? false ? val : null;

            // 6 - Общая стоимость
            var totalCost = (dbTransaction.Quantity ?? 0) * (dbTransaction.Cost ?? 0);
            dbTransaction.TotalCost = Math.Round(totalCost, 3, MidpointRounding.AwayFromZero);

            // 7 - По умолчанию ложь
            dbTransaction.IsCheck = false;

            // 8 - Идентификатор местоположения заправочной станции, где была осуществлена транзакция
            dbTransaction.CountryId = GetCountry();

            // 9 - Дата и время транзакции
            var operationDate = GetOperationDate(parsedTransaction.OperationDateTime_Day, parsedTransaction.OperationDateTime_Hour, parsedTransaction.OperationDateTime_Minute);
            if (operationDate == default)
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

            dbTransaction.OperationDate = operationDate;

            // 10 - Заправочная карта
            // Если топливная карта не внесена в систему - пополнить коллекцию и пропустить транзакцию - не вносить ее в систему
            var foundCardId = await SearchCard(parsedTransaction.CarNum); // номер авто - он же номер топливной карты

            if (!foundCardId.HasValue || foundCardId == 0)
            {
                var message = $"Номер заправочной карты «{parsedTransaction.CarNum ?? string.Empty}» не найден в БД";

                notSuccessItems?.Add(
                    new NotSuccessResponseItemDetailed<NotParsedTransaction>(
                        new NotParsedTransaction
                        {
                            CardNumber = string.Empty,
                            CarNumber = parsedTransaction.CarNum, // номер авто из отчета, который не удалось найти в бд (он же номер заправочной карты)
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

            if (dbTransaction.FuelTypeId != 2
                && dbTransaction.FuelTypeId != 10) // в БД добавляются только тр-ции по заправке топливом и adblue
            {
                var message = $"Разновидность услуги не учитывается и не хранится в БД. ";

                notSuccessItems?.Add(
                    new NotSuccessResponseItemDetailed<NotParsedTransaction>(
                        new NotParsedTransaction
                        {
                            CardNumber = string.Empty,
                            CarNumber = string.Empty,
                            NotFuelType = parsedTransaction.FuelType ?? "«Разновидность услуги не определена»", // наименование услуги из отчета о реализации
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

            return dbTransaction;
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(ParseFile), GetType().FullName);
            throw;
        }
    }

    private DateTime GetOperationDate(DateTime? date, int? hour, int? min)
    {
        return date.HasValue
            && (!hour.HasValue || (hour.HasValue && hour.Value >= 0 && hour <= 23))
            && (!min.HasValue || (min.HasValue && min.Value >= 0 && min <= 59))
            ? new DateTime(
                year: date.Value.Year,
                month: date.Value.Month,
                day: date.Value.Day,
                hour: hour.HasValue ? hour.Value : 0,
                minute: min.HasValue ? min.Value : 0,
                second: 0)
            : default;
    }
}
