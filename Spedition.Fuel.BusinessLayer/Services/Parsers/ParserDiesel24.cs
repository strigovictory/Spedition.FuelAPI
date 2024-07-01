using Microsoft.AspNetCore.Hosting;
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

public class ParserDiesel24 : FuelParserBase<FuelTransactionDiesel24, FuelTransactionShortResponse, NotParsedTransaction, FuelTransaction>
{
    public ParserDiesel24(
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
        ProvidersId = new List<int> { 7 };
    }

    protected override async Task<FuelTransaction> MappingParsedToDB(IParsedItem parsedReportsItem)
    {
        FuelTransaction dbTransaction = new();

        if (parsedReportsItem == null || parsedReportsItem is not FuelTransactionDiesel24 parsedTransaction)
        {
            $"Ошибка - несоответствие транзакции типу {nameof(FuelTransactionDiesel24)} !".LogError(GetType().Name, nameof(MappingParsedToDB));
            return null;
        }

        try
        {            
            // 1 - Поставщик топлива «Diesel24»
            dbTransaction.ProviderId = ProviderId;

            // 2 - Разновидность топлива
            dbTransaction.FuelTypeId = GetFuelType(parsedTransaction.FuelType);

            // 3 - Заправленное кол-во
            dbTransaction.Quantity = Math.Round(parsedTransaction.Quantity, 2, MidpointRounding.AwayFromZero);

            // 4 - Цена за литр // с 17.10.22 Метик сказал этого столбца в отчетах не будет
            //entryMapped.Cost = Math.Round(parsedTransaction.Cost, 3, MidpointRounding.AwayFromZero);

            // 5 - Валюта
            dbTransaction.CurrencyId = null;

            // 6 - Общая стоимость
            //dbTransaction.TotalCost = (parsedTransaction.Cost ?? 0) * (parsedTransaction.Quantity ?? 0);

            // 7 - По умолчанию ложь
            dbTransaction.IsCheck = false;

            // 8 - Местоположение заправочной станции, где была осуществлена транзакция
            dbTransaction.CountryId = await GetCountry(parsedTransaction.Country?.Trim()?.Substring(0, 2) ?? string.Empty);

            // 9 - Дата и время транзакции
            var operationDay = GetOperationDate(parsedTransaction.OperationDate, parsedTransaction.OperationTime);
            if (operationDay != default)
                dbTransaction.OperationDate = operationDay;
            else
            {
                var message = $"{parsedTransaction.ToString()} не м.б. добавлена в БД, т.к. не удалось определить дату операции";

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

    protected override int GetFuelType(string fuelTypeName)
    {
        try
        {
            if (string.IsNullOrEmpty(fuelTypeName))
            {
                Log.Warning($"Услуга имеет пустое наименование. ");
                return 13; // Прочее
            }
            else if (fuelTypeName?.Equals("425") ?? false)
            {
                return 2;
            }
            else if (fuelTypeName?.Equals("450") ?? false)
            {
                return 10;
            }
            else
            {
                return base.GetFuelType(fuelTypeName);
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(GetFuelType), GetType().FullName);
            throw;
        }
    }
}
