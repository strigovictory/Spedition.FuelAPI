using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Spedition.Fuel.BusinessLayer.Configs;
using Spedition.Fuel.BusinessLayer.Models;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Models.Tatneft;
using Spedition.Fuel.BusinessLayer.Services.ProvidersApi.Helpers;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.Entities;

namespace Spedition.Fuel.BusinessLayer.Services.ProvidersApi;

public class TatneftService : ProvidersApiBase<TatneftTransaction>
{
    public TatneftService(
        IWebHostEnvironment env, 
        FuelRepositories fuelRepositories,
        IConfiguration configuration,
        IMapper mapper,
        ICountryService countryService,
        ICurrencyService currencyService,
        IEventsTypeService eventsTypeService,
        IDivisionService divisionService,
        ITruckService truckService,
        IOptions<TatneftConfig> options) 
        : base(env, fuelRepositories, configuration, mapper, countryService, currencyService, eventsTypeService, divisionService, truckService, options)
    {
        ProviderId = 20;
    }

    private async Task UpdateProvidersDivisionCards(int divisionId)
    {
        ProvidersCards = await fuelRepositories?.Cards?.FindRangeAsync(card => card.ProviderId == ProviderId && card.DivisionID == divisionId);
    }

    public override async Task<bool> GetTransactions()
    {
        ItemsToMapping = new();

        // Аккаунты у поставщика
        await InitProvidersAccounts();

        if ((ProvidersAccounts?.Count ?? 0) == 0)
        {
            Log.Error($"Ошибка при выполнении поиска в БД аккаунтов, относящихся к провайдеру топлива «Татнефть» " +
                                            $"внутри метода {nameof(GetTransactions)} в классе {GetType()?.Name ?? string.Empty}. ");
            return false;
        }

        TatneftHelper.baseUrl = Options?.Value?.BaseUrl ?? string.Empty;

        foreach (var account in ProvidersAccounts)
        {
            TatneftHelper.appId = account?.Login ?? string.Empty;
            TatneftHelper.privateKey = account?.Password ?? string.Empty;

            await UpdateProvidersDivisionCards(account.DivisionId);

            // 1 - Получение числа транзакций, уже сохраненных в БД по этому провайдеру и этому подразделению
            TatneftHelper.dbTransactionsCount = (await fuelRepositories?.Transactions?.FindRangeAsync(transaction => transaction.ProviderId == ProviderId))
                .Where(transaction => ProvidersCards.Any(card => card.Id == transaction.CardId))?.Count() ?? 0;

            // 2 - Получение списка транзакций от АПИ провайдера
            var transactions = await TatneftHelper.GetTransactions();
            transactions?.Where(transaction => (transaction?.volume ?? 0) != 0)?.ToList()?.ForEach(transaction => ItemsToMapping?.Add(transaction));
        }

        return (ItemsToMapping?.Count ?? 0) > 0;
    }

    protected override async Task<FuelTransaction> MappingParsedToDB(IParsedItem parsedReportsItem)
    {
        FuelTransaction dbTransaction = new();

        if (parsedReportsItem == null || parsedReportsItem is not TatneftTransaction parsedTransaction)
        {
            $"Ошибка - несоответствие транзакции типу {nameof(TatneftTransaction)} !".LogError(GetType().Name, nameof(MappingParsedToDB));
            return null;
        }

        try
        {
            // 0 - Идентификатор транщакции
            dbTransaction.TransactionID = parsedTransaction.id;

            // 1 - Поставщик топлива
            dbTransaction.ProviderId = ProviderId;

            // 2 - Разновидность топлива
            dbTransaction.FuelTypeId = GetFuelType(parsedTransaction.fuel_type);

            // 9 -Заправленное кол-во
            dbTransaction.Quantity = parsedTransaction?.volume ?? 0;

            // 4 - Цена за литр
            dbTransaction.Cost = parsedTransaction?.price ?? 0;

            // 5 - Валюта
            dbTransaction.CurrencyId = await GetCurrency("RUB");

            // 6 - Общая стоимость
            dbTransaction.TotalCost = (parsedTransaction?.price ?? 0) * (parsedTransaction?.volume ?? 0);

            // 7 - По умолчанию ложь
            dbTransaction.IsCheck = false;

            // 8 - Местоположение заправочной станции, где была осуществлена транзакция
            dbTransaction.CountryId = await GetCountry(parsedTransaction?.address ?? string.Empty);

            // 9 - Дата и время транзакции
            var operationDay = GetOperationDate(parsedTransaction.date);
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
            var foundCardId = await SearchCard(parsedTransaction.card_num);

            if (!foundCardId.HasValue || foundCardId == 0)
            {
                var message = $"Номер заправочной карты «{parsedTransaction.card_num ?? string.Empty}» не найден в БД";

                notSuccessItems?.Add(
                    new NotSuccessResponseItemDetailed<NotParsedTransaction>(
                        new NotParsedTransaction
                        {
                            CardNumber = parsedTransaction.card_num,
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
                            NotFuelType = parsedTransaction.fuel_type ?? "«Разновидность услуги не определена»", // наименование услуги из отчета о реализации
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
