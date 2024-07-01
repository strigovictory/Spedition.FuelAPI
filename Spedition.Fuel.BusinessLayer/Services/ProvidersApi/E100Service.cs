using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Spedition.Fuel.BusinessLayer.Configs;
using Spedition.Fuel.BusinessLayer.Models;
using Spedition.Fuel.BusinessLayer.Models.E100;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Models.Tatneft;
using Spedition.Fuel.BusinessLayer.Services.ProvidersApi.Helpers;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.Fuel.BusinessLayer.Services.ProvidersApi;

public class E100Service : ProvidersApiBase<E100Transaction>
{
    public E100Service(
        IWebHostEnvironment env,
        FuelRepositories fuelRepositories,
        IConfiguration configuration,
        IMapper mapper,
        ICountryService countryService,
        ICurrencyService currencyService,
        IEventsTypeService eventsTypeService,
        IDivisionService divisionService,
        ITruckService carService,
        IOptions<E100Config> options)
        : base(env, fuelRepositories, configuration, mapper, countryService, currencyService, eventsTypeService, divisionService, carService, options)
    {
        ProviderId = 10; // Поставщик топлива
    }

    public override async Task<bool> GetTransactions()
    {
        ItemsToMapping = new();

        // Аккаунты у поставщика
        await InitProvidersAccounts();

        if ((ProvidersAccounts?.Count ?? 0) == 0)
        {
            Log.Error($"Ошибка при выполнении поиска в БД аккаунтов, относящихся к провайдеру топлива «E100» " +
                                            $"внутри метода {nameof(GetTransactions)} в классе {GetType()?.Name ?? string.Empty}. ");
            return false;
        }

        foreach (var account in ProvidersAccounts)
        {
            E100Helper.baseUrl = account?.BaseUrl ?? string.Empty;
            E100Helper.login = account?.Login ?? string.Empty;
            E100Helper.password = account?.Password ?? string.Empty;

            var isAuthenticated = await E100Helper.GetAuth();

            if (!isAuthenticated)
            {
                $"Ошибка авторизации. Подробности: провайдер Е100, {E100Helper.Details}. "
                    .LogError(GetType().Name, nameof(GetTransactions));
                continue;
            }

            var pageFirst = await E100Helper.GetTransactions();
            ItemsToMapping?.AddRange(pageFirst?.Transactions?.Where(transaction => transaction.Datetime_insert != null)?.ToList() ?? new());

            for (var i = 2; i <= (pageFirst?.Page_count ?? 0); i++)
            {
                var pageNext = await E100Helper.GetTransactions(i);
                ItemsToMapping?.AddRange(pageNext?.Transactions?.Where(transaction => transaction.Datetime_insert != null)?.ToList() ?? new());
            }
        }

        return (ItemsToMapping?.Count ?? 0) > 0;
    }

    protected override async Task<FuelTransaction> MappingParsedToDB(IParsedItem parsedReportsItem)
    {
        FuelTransaction dbTransaction = new();

        if (parsedReportsItem == null || parsedReportsItem is not E100Transaction parsedTransaction)
        {
            $"Ошибка - несоответствие транзакции типу {nameof(E100Transaction)} !".LogError(GetType().Name, nameof(MappingParsedToDB));
            return null;
        }

        try
        {
            // 0 - Идентификатор транщакции
            dbTransaction.TransactionID = parsedTransaction.UnId;

            // 1 - Поставщик топлива
            dbTransaction.ProviderId = ProviderId;

            // 2 - Разновидность топлива
            dbTransaction.FuelTypeId = GetFuelType(parsedTransaction?.Service_id ?? 0);

            // 9 -Заправленное кол-во
            dbTransaction.Quantity = parsedTransaction.Service_id == 132 || parsedTransaction.Service_id == 133 ? parsedTransaction.Volume * 10 : parsedTransaction.Volume;

            // 4 - Цена за литр
            dbTransaction.Cost = parsedTransaction?.Price ?? 0;

            // 5 - Валюта
            var currencyName = parsedTransaction.Currency == "RUR" ? "RUB" : parsedTransaction.Currency;
            dbTransaction.CurrencyId = await GetCurrency(currencyName);

            // 6 - Общая стоимость
            dbTransaction.TotalCost = (parsedTransaction?.Price ?? 0) * (parsedTransaction?.Volume ?? 0);

            // 7 - По умолчанию ложь
            dbTransaction.IsCheck = false;

            // 8 - Местоположение заправочной станции, где была осуществлена транзакция
            dbTransaction.CountryId = await GetCountry(parsedTransaction.Station_id?.Trim()?.Substring(0, 2) ?? string.Empty);

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

            if (dbTransaction.FuelTypeId > 13) // в БД добавляются только тр-ции по заправке топливом и adblue и прочие
            {
                var message = $"Разновидность услуги не учитывается и не хранится в БД. ";
                notSuccessItems?.Add(
                    new NotSuccessResponseItemDetailed<NotParsedTransaction>(
                        new NotParsedTransaction
                        {
                            CardNumber = string.Empty,
                            CarNumber = string.Empty,
                            NotFuelType = parsedTransaction.Service_name ?? "«Разновидность услуги не определена»", // наименование услуги из отчета о реализации
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

    private int GetFuelType(int serviceId)
    {
        var fuelServices = new List<int> { 2, 3, 11, 12, 17, 21, 23, 24, 25, 27, 32, 33, 36, 38, 234, 239 };
        var adblueServices = new List<int> { 41, 42, 132, 133 };
        var otherServices = new List<int> { 61, 63, 244 };

        if (fuelServices.Any(fuelService => fuelService == serviceId))
            return 2;
        else if (adblueServices.Any(adblueService => adblueService == serviceId))
            return 10;
        else if (otherServices.Any(otherService => otherService == serviceId))
            return 13;
        else return 14;
    }
}