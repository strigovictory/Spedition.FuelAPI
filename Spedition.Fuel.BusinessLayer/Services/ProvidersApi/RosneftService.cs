using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Spedition.Fuel.BusinessLayer.Configs;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Models.Neftika;
using Spedition.Fuel.BusinessLayer.Models.Rosneft;
using Spedition.Fuel.BusinessLayer.Services.ProvidersApi.Helpers;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.Fuel.BusinessLayer.Services.ProvidersApi;

public class RosneftService : ProvidersApiBase<RosneftTransaction>
{
    public RosneftService(
        IWebHostEnvironment env,
        FuelRepositories fuelRepositories,
        IConfiguration configuration,
        IMapper mapper,
        ICountryService countryService,
        ICurrencyService currencyService,
        IEventsTypeService eventsTypeService,
        IDivisionService divisionService,
        ITruckService truckService,
        IOptions<RosneftConfig> options)
        : base(env, fuelRepositories, configuration, mapper, countryService, currencyService, eventsTypeService, divisionService, truckService, options)
    {
        ProviderId = 2;
    }

    private List<RosneftServiceStation> ServiceStations { get; set; } = new();

    private List<RosneftServiceStationsCountry> ServiceStationsCountries { get; set; } = new();

    public override async Task<bool> GetTransactions()
    {
        ItemsToMapping = new();

        await InitProvidersAccounts();

        if ((ProvidersAccounts?.Count ?? 0) == 0)
        {
            Log.Error($"Ошибка при выполнении поиска в БД аккаунтов, относящихся к провайдеру топлива «Rosneft» " +
                                            $"внутри метода {nameof(GetTransactions)} в классе {GetType()?.Name ?? string.Empty}. ");
            return false;
        }

        RosneftHelper.baseUrl = Options?.Value?.BaseUrl ?? string.Empty;

        foreach (var account in ProvidersAccounts)
        {
            RosneftHelper.login = account?.Login ?? string.Empty;
            RosneftHelper.password = account?.Password ?? string.Empty;
            RosneftHelper.contract = account?.Key ?? string.Empty;

            // 1 - Получение списка АЗС
            var azs = await RosneftHelper.GetServiceStations();
            ServiceStations?.AddRange(azs ?? new());

            // 2 - Получение списка стран
            var countries = await RosneftHelper.GetServiceStationsCountries();
            ServiceStationsCountries?.AddRange(countries ?? new());

            // 3 - Получение списка транзакций
            // данные забираем за предыдущие три дня
            var dateFrom = new DateTime(day: DateTime.Now.Day, month: DateTime.Now.Month, year: DateTime.Now.Year, hour: 0, minute: 0, second: 0).AddDays(-3).ToString("yyyy-MM-ddTHH:mm:ss");
            var dateTo = new DateTime(day: DateTime.Now.Day, month: DateTime.Now.Month, year: DateTime.Now.Year, hour: 23, minute: 59, second: 59).AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ss");

            var transactions = await RosneftHelper.GetTransactions(dateFrom, dateTo);

            transactions?.Where(transaction => 
            (transaction.Type == RosneftOperationsType.Обслуживание || transaction.Type == RosneftOperationsType.ВозвратНаСчет)
            && (transaction?.Value ?? 0) != 0)?
            .ToList()?.ForEach(transaction => ItemsToMapping?.Add(transaction));
        }

        return (ItemsToMapping?.Count ?? 0) > 0;
    }

    protected override async Task<FuelTransaction> MappingParsedToDB(IParsedItem parsedReportsItem)
    {
        FuelTransaction dbTransaction = new();

        if (parsedReportsItem == null || parsedReportsItem is not RosneftTransaction parsedTransaction)
        {
            $"Ошибка - несоответствие транзакции типу {nameof(RosneftTransaction)} !".LogError(GetType().Name, nameof(MappingParsedToDB));
            return null;
        }

        try
        {
            // 0 - Идентификатор транзакции
            dbTransaction.TransactionID = parsedTransaction.Code;

            // 1 - Поставщик топлива
            dbTransaction.ProviderId = ProviderId;

            // 2 - Разновидность топлива
            dbTransaction.FuelTypeId = GetFuelType(parsedTransaction?.GCode);

            // 9 -Заправленное кол-во
            dbTransaction.Quantity = parsedTransaction.Value > 0 ? (parsedTransaction.Type == RosneftOperationsType.ВозвратНаСчет ? parsedTransaction.Value * -1 : parsedTransaction.Value) : parsedTransaction.Value;

            // 4 - Цена за литр - брутто со всеми включенными налогами/наценками
            dbTransaction.Cost = parsedTransaction.Price;

            // 5 - Валюта
            dbTransaction.CurrencyId = await GetCurrency("RUB");

            // 6 - Общая стоимость - брутто со всеми включенными налогами/наценками
            dbTransaction.TotalCost = parsedTransaction.Sum > 0 ? (parsedTransaction.Type == RosneftOperationsType.ВозвратНаСчет ? parsedTransaction.Sum * -1 : parsedTransaction.Sum) : parsedTransaction.Sum; // брутто со всеми включенными налогами/наценками

            // 7 - По умолчанию ложь
            dbTransaction.IsCheck = false;

            // 8 - Местоположение заправочной станции, где была осуществлена транзакция
            dbTransaction.CountryId = await GetCountry(parsedTransaction.PosCode);

            // 9 - Дата и время транзакции
            var operationDay = GetOperationDate(parsedTransaction.Date);
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
                            NotFuelType = parsedTransaction.GCode ?? "«Разновидность услуги не определена»", // наименование услуги из отчета о реализации
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

    private async Task<int?> GetCountry(string azs)
    {
        int? country = null;

        var countryCode = ServiceStations?.FirstOrDefault(_ => _.Code.Contains(azs, StringComparison.InvariantCultureIgnoreCase))?.CountryCode ?? string.Empty;
        var countryName = ServiceStationsCountries?.FirstOrDefault(_ => _.CountryCode.Equals(countryCode, StringComparison.InvariantCultureIgnoreCase))?.CountryName ?? string.Empty;
        
        if (!string.IsNullOrEmpty(countryCode))
            country = await GetCountry(countryName);

        return country;
    }
}
