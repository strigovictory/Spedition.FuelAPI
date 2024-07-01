using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Spedition.Fuel.BusinessLayer.Configs;
using Spedition.Fuel.BusinessLayer.Models.E100;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Models.Neftika;
using Spedition.Fuel.BusinessLayer.Services.ProvidersApi.Helpers;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.Fuel.BusinessLayer.Services.ProvidersApi;

public class NeftikaService : ProvidersApiBase<NeftikaTransaction>
{
    public NeftikaService (
        IWebHostEnvironment env,
        FuelRepositories fuelRepositories,
        IConfiguration configuration,
        IMapper mapper,
        ICountryService countryService,
        ICurrencyService currencyService,
        IEventsTypeService eventsTypeService,
        IDivisionService divisionService,
        ITruckService truckService,
        IOptions<NeftikaConfig> options)
        : base(env, fuelRepositories, configuration, mapper, countryService, currencyService, eventsTypeService, divisionService, truckService, options)
    {
        ProviderId = 16;
    }

    public override async Task<bool> GetTransactions()
    {
        ItemsToMapping = new();

        await InitProvidersAccounts();

        if ((ProvidersAccounts?.Count ?? 0) == 0)
        {
            Log.Error($"Ошибка при выполнении поиска в БД аккаунтов, относящихся к провайдеру топлива «Neftika» " +
                                            $"внутри метода {nameof(GetTransactions)} в классе {GetType()?.Name ?? string.Empty}. ");
            return false;
        }

        NeftikaHelper.baseUrl = Options?.Value?.BaseUrl ?? string.Empty;

        foreach (var account in ProvidersAccounts)
        {
            NeftikaHelper.login = account?.Login ?? string.Empty;
            NeftikaHelper.password = account?.Password ?? string.Empty;

            var isAuthenticated = await NeftikaHelper.GetAuth();

            if (!isAuthenticated)
            {
                $"Ошибка авторизации. Подробности: провайдер Neftika, {NeftikaHelper.Details}. "
                    .LogError(GetType().Name, nameof(GetTransactions));
                continue;
            }

            var transactions = await NeftikaHelper.GetTransactions();
            transactions?.Where(transaction => (transaction?.Liters ?? 0) != 0)?.ToList()?.ForEach(transaction => ItemsToMapping?.Add(transaction));
        }

        return (ItemsToMapping?.Count ?? 0) > 0;
    }

    protected override async Task<FuelTransaction> MappingParsedToDB(IParsedItem parsedReportsItem)
    {
        FuelTransaction dbTransaction = new();

        if (parsedReportsItem == null || parsedReportsItem is not NeftikaTransaction parsedTransaction)
        {
            $"Ошибка - несоответствие транзакции типу {nameof(NeftikaTransaction)} !".LogError(GetType().Name, nameof(MappingParsedToDB));
            return null;
        }

        try
        {
            // 0 - Идентификатор транщакции
            dbTransaction.TransactionID = parsedTransaction.ID.ToString();

            // 1 - Поставщик топлива
            dbTransaction.ProviderId = ProviderId;

            // 2 - Разновидность топлива
            dbTransaction.FuelTypeId = GetFuelType(parsedTransaction?.ServiceName);

            // 9 -Заправленное кол-во
            dbTransaction.Quantity = parsedTransaction.Liters < 0 ? Math.Abs(parsedTransaction.Liters) : parsedTransaction.Liters * -1;

            // 4 - Цена за литр
            dbTransaction.Cost = parsedTransaction?.Price ?? 0;

            // 5 - Валюта
            dbTransaction.CurrencyId = await GetCurrency(parsedTransaction?.Currency ?? string.Empty);

            // 6 - Общая стоимость
            dbTransaction.TotalCost = parsedTransaction.AmountOriginal < 0 ? Math.Abs(parsedTransaction.AmountOriginal) : parsedTransaction.AmountOriginal * -1;

            // 7 - По умолчанию ложь
            dbTransaction.IsCheck = false;

            // 8 - Местоположение заправочной станции, где была осуществлена транзакция
            dbTransaction.CountryId = await GetCountry(parsedTransaction.UIDAZS.Substring(0, 2) ?? string.Empty);

            // 9 - Дата и время транзакции
            var operationDay = parsedTransaction.Date;
            if (operationDay != default)
                dbTransaction.OperationDate = operationDay;
            else
            {
                var message = $"{parsedTransaction?.ToString() ?? "Транзакция"} не м.б. добавлена в БД, т.к. не удалось определить дату операции";
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
            // Если топливная карта не внесена в систему - пополнить коллекцию и пропустить транзакцию - не вносить ее в систему
            var foundCardId = await SearchCard(parsedTransaction.Card);

            if (!foundCardId.HasValue || foundCardId == 0)
            {
                var message = $"Номер заправочной карты «{parsedTransaction.Card ?? string.Empty}» не найден в БД";

                notSuccessItems?.Add(
                    new NotSuccessResponseItemDetailed<NotParsedTransaction>(
                        new NotParsedTransaction
                        {
                            CardNumber = parsedTransaction.Card,
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

            if (dbTransaction.FuelTypeId != 2 && dbTransaction.FuelTypeId != 10) // в БД добавляются только тр-ции по заправке топливом и adblue
            {
                var message = $"Разновидность услуги не учитывается и не хранится в БД. ";
                notSuccessItems?.Add(
                    new NotSuccessResponseItemDetailed<NotParsedTransaction>(
                        new NotParsedTransaction
                        {
                            CardNumber = string.Empty,
                            CarNumber = string.Empty,
                            NotFuelType = parsedTransaction.ServiceName ?? "«Разновидность услуги не определена»", // наименование услуги из отчета о реализации
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

            return dbTransaction;
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(MappingParsedToDB), GetType().FullName);
            throw;
        }
    }
}