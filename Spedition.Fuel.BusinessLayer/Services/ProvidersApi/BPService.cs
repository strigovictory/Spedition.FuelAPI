using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Spedition.Fuel.BusinessLayer.Configs;
using Spedition.Fuel.BusinessLayer.Helpers;
using Spedition.Fuel.BusinessLayer.Models;
using Spedition.Fuel.BusinessLayer.Models.BPApi;
using Spedition.Fuel.BusinessLayer.Models.E100;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Models.Tatneft;
using Spedition.Fuel.BusinessLayer.Services.ProvidersApi.Helpers;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.Fuel.BusinessLayer.Services.ProvidersApi;

public class BPService : ProvidersApiBase<BPApiTransaction>
{
    public BPService(
        IWebHostEnvironment env,
        FuelRepositories fuelRepositories,
        IConfiguration configuration,
        IMapper mapper,
        ICountryService countryService,
        ICurrencyService currencyService,
        IEventsTypeService eventsTypeService,
        IDivisionService divisionService,
        ITruckService truckService,
        IOptions<BPConfig> options)
        : base(env, fuelRepositories, configuration, mapper, countryService, currencyService, eventsTypeService, divisionService, truckService, options)
    {
        ProviderId = 11; // Поставщик топлива
    }

    protected override async Task InitSecondaryCollections()
    {
        FuelCardsCountries = await fuelRepositories?.FuelCardsCountries?.GetAsync();
    }

    private async Task<List<BPTransaction>> GetDBTransactionsBP()
    {
        List<BPTransaction> result = new();

        if (Periodicity.HasValue)
        {
            var startTime = DateTime.Now.AddDays(-Periodicity.Value).Date;
            result = await fuelRepositories?.BPTransactions?.FindRangeAsync(
                transaction => transaction.InvoiceNotifyDate.HasValue && transaction.InvoiceNotifyDate.Value.Date >= startTime);
        }
        else
        {
            result = await fuelRepositories?.BPTransactions?.GetAsync();
        }

        return result;
    }

    public override async Task<bool> GetTransactions()
    {
        ItemsToMapping = new();

        var dbTransactionsBP = await GetDBTransactionsBP();
        dbTransactionsBP?.ForEach(transaction => ItemsToMapping?.Add(mapper?.Map<BPApiTransaction>(transaction)));

        // test
        //ItemsToMapping?.Add(mapper?.Map<BPApiTransaction>(dbTransactionsBP?.FirstOrDefault(_ => !string.IsNullOrEmpty(_.OriginalCountryCode))));

        return (ItemsToMapping?.Count ?? 0) > 0;
    }

    protected override async Task<FuelTransaction> MappingParsedToDB(IParsedItem parsedReportsItem)
    {
        FuelTransaction dbTransaction = new();

        if (parsedReportsItem == null || parsedReportsItem is not BPApiTransaction parsedTransaction)
        {
            $"Ошибка - несоответствие транзакции типу {nameof(BPApiTransaction)} !".LogError(GetType().Name, nameof(MappingParsedToDB));
            return null;
        }

        try
        {
            // 0 - Идентификатор тр-ции
            dbTransaction.TransactionID = parsedTransaction.TxnNo;

            // 1 - Поставщик топлива «BP»
            dbTransaction.ProviderId = ProviderId;

            // 2 - Разновидность топлива
            dbTransaction.FuelTypeId = GetFuelType(parsedTransaction.ProdDesc);

            // 3 - Колличество, литров
            dbTransaction.Quantity = parsedTransaction.Quantity; // знак уже включен

            // 4 - Цена за литр
            dbTransaction.Cost = parsedTransaction.UnitPrice;

            // 5 - Валюта
            var curr = string.IsNullOrEmpty(parsedTransaction?.OriginalCurrency?.Trim()) ? "pln" : parsedTransaction.OriginalCurrency;
            dbTransaction.CurrencyId = await GetCurrency(curr);

            // 6 - Общая стоимость
            dbTransaction.TotalCost = parsedTransaction.GrossRebate; // знак уже включен

            // 7 - По умолчанию ложь
            dbTransaction.IsCheck = false;

            // 8 - Местоположение заправочной станции, где была осуществлена транзакция
            dbTransaction.CountryId = int.TryParse(parsedTransaction.OriginalCountryCode, out var val) ? GetCountry(val) : null;

            // 9 - Дата и время транзакции
            var operationDate = parsedTransaction.InvoiceNotifyDate;
            if (operationDate.HasValue && operationDate != default)
            {
                dbTransaction.OperationDate = operationDate.Value;
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
                            IsDayly = true,
                            IsMonthly = default,
                        }, message));

                return null;
            }

            // 10 - Заправочная карта
            var cardNumFull = string.Concat(
                parsedTransaction?.IssueNo ?? string.Empty, 
                parsedTransaction?.AuthorityId.Substring(1, (parsedTransaction?.AuthorityId?.Length ?? 0) - 1) ?? string.Empty, // K100957
                parsedTransaction?.CardSerialNo?.ToString()?.AddToSixDigits());

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
                            IsDayly = true,
                            IsMonthly = default,
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
}