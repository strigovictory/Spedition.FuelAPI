
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Spedition.Fuel.BusinessLayer.Enums;
using Spedition.Fuel.BusinessLayer.Helpers;
using Spedition.Fuel.BusinessLayer.Models;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.Parsers.ParserBaseServices;
using Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Trips;
using Log = Serilog.Log;

namespace Spedition.Fuel.BusinessLayer.Services.Parsers;

public class ParserAdnoc : FuelParserBase<FuelTransactionAdnoc, FuelTransactionShortResponse, NotParsedTransaction, FuelTransaction>
{
    public ParserAdnoc(
        IWebHostEnvironment env,
        FuelRepositories fuelRepositories,
        IConfiguration configuration,
        IMapper mapper,
        ICountryService countryService,
        ICurrencyService currencyService,
        IEventsTypeService eventsTypeService,
        IDivisionService divisionService,
        ITruckService carService)
        : base(env, fuelRepositories, configuration, mapper, countryService, currencyService, eventsTypeService, divisionService, carService, OuterLibrary.EPPlus)
    {
        ProvidersId = new List<int> { 19 };
    }

    #region Overriden
    /// <summary>
    /// В этом парсере переопределенная версия маппинга из-за того, что внутри маппера есть обращение к БД на сохранение новых топл.карт,
    /// класс Parallel.ForEachAsync этого не допускает.
    /// </summary>
    protected override async Task MappingParsedToDB()
    {
        try
        {
            List<FuelTransaction> result = new();

            await InitDataForMapping();

            foreach (var item in ItemsToMapping ?? new())
            {
                var dbItem = await MappingParsedToDB(item);

                if (dbItem != null)
                {
                    result.Add(dbItem);
                }
            }

            ItemsToSaveInDB = new(result);
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(MappingParsedToDB), GetType().FullName);
            throw;
        }
    }

    protected override async Task<FuelTransaction> MappingParsedToDB(IParsedItem parsedReportsItem)
    {
        FuelTransaction dbTransaction = new();

        if (parsedReportsItem == null || parsedReportsItem is not FuelTransactionAdnoc parsedTransaction)
        {
            Log.Error($"Ошибка - несоответствие транзакции типу {nameof(FuelTransactionAdnoc)} !");
            return null;
        }

        try
        {
            // 1 - Поставщик топлива «Adnoc»
            dbTransaction.ProviderId = ProviderId;

            // 2 - Разновидность топлива
            dbTransaction.FuelTypeId = GetFuelType(parsedTransaction.FuelType);

            // 3 - Заправленное кол-во
            dbTransaction.Quantity = Math.Round(parsedTransaction.Quantity, 2, MidpointRounding.AwayFromZero);

            // 4 - Цена за литр
            dbTransaction.Cost = Math.Round(parsedTransaction.Cost, 3, MidpointRounding.AwayFromZero);

            // 5 - Валюта
            dbTransaction.CurrencyId = Currencies?.FirstOrDefault(currency =>
            !string.IsNullOrEmpty(currency.Name) && currency.Name.ToLower().Contains("rub"))?.Id;

            // 6 - Общая стоимость
            var totalCost = dbTransaction.TotalCost ?? (dbTransaction.Cost ?? 0) * (dbTransaction.Quantity ?? 0);
            dbTransaction.TotalCost = Math.Round(totalCost, 3, MidpointRounding.AwayFromZero);

            // 7 - По умолчанию ложь
            dbTransaction.IsCheck = false;

            // 8 - Местоположение заправочной станции, где была осуществлена транзакция
            dbTransaction.CountryId = Countries?.FirstOrDefault(country =>
            !string.IsNullOrEmpty(country.CountryCode) && country.CountryCode.ToLower().Equals("ru"))?.Id;

            // 9 - Дата и время транзакции
            var operationDate = GetOperationDate(parsedTransaction.OperationDate, parsedTransaction.OperationTime);

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
                            IsMonthly = IsMonthly.HasValue && IsMonthly.Value ? true: default,
                            IsDayly = !IsMonthly.HasValue || !IsMonthly.Value ? true : default,
                        }, message));

                return null;
            }

            // 10 - Заправочная карта
            if (parsedTransaction.TrucksNumber == null)
            {
                var message = $"Номер авто из отчета имеет пустое значение! ";

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

            var cardId = await SearchCard(parsedTransaction.TrucksNumber);

            // Если карта не найдена - добавить (номер карты равен номеру авто)
            if (!cardId.HasValue)
            {
                // Найти тягач, к которому относится carNum
                var truck = await SearchTruck(parsedTransaction.TrucksNumber, parsedTransaction.OperationDate);

                if ((truck?.Id ?? 0) == 0)
                {
                    var message = $"Номер тягача «{parsedTransaction.TrucksNumber ?? string.Empty}» не найден в БД";

                    notSuccessItems?.Add(
                        new NotSuccessResponseItemDetailed<NotParsedTransaction>(
                            new NotParsedTransaction
                            {
                                CardNumber = parsedTransaction.TrucksNumber,
                                CarNumber = string.Empty,
                                NotFuelType = string.Empty,
                                TransactionID = dbTransaction.TransactionID,
                                OperationDate = parsedTransaction.OperationDate,
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

                var addedFuelCard = await AddFuelCard(truck, ProviderId, parsedTransaction.OperationDate);

                if ((addedFuelCard?.Id ?? 0) > 0)
                {
                    dbTransaction.CardId = addedFuelCard.Id;
                    await UpdateProvidersCards(); // обновить коллекцию заправочных карт провайдера
                }
                else
                {
                    var message = $"Ошибка на уровне сервера. Новая топливная карта для автомобиля {parsedTransaction.TrucksNumber ?? "«»"} не была добавлена в БД. ";

                    notSuccessItems?.Add(
                        new NotSuccessResponseItemDetailed<NotParsedTransaction>(
                            new NotParsedTransaction
                            {
                                CardNumber = parsedTransaction.TrucksNumber,
                                CarNumber = string.Empty,
                                NotFuelType = string.Empty,
                                TransactionID = dbTransaction.TransactionID,
                                OperationDate = parsedTransaction.OperationDate,
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

                    Log.Error(message);
                    return null;
                }
            }
            else
            {
                dbTransaction.CardId = cardId.Value;
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(MappingParsedToDB), GetType().FullName);
            throw;
        }

        return dbTransaction;
    }
    #endregion

    #region // Вспомогательные методы

    /// <summary>
    /// Метод для поиска тягача по его номеру.
    /// </summary>
    /// <param Name="requiredCarNum">Номер тягача из отчета.</param>
    /// <param Name="operationsDate">Дата транзакции по топливу.</param>
    /// <returns>Тягач.</returns>
    private async Task<TruckResponse> SearchTruck(string requiredCarNum, DateTime operationsDate)
    {
        int? result = null;

        if (string.IsNullOrEmpty(requiredCarNum))
        {
            return null;
        }

        Func<TruckResponse> ToSearchCar = () => Trucks?.FirstOrDefault(car => result.HasValue && car.Id == result.Value) ?? null;

        Func<List<TrucksLicensePlateResponse>, int?> GetCarIdInsidePeriod = (List<TrucksLicensePlateResponse> carsNumbers) =>
        carsNumbers?.OrderByDescending(carNumber => carNumber.LicensePlate)?
        .FirstOrDefault(carNumber => carNumber.BeginDate.Date <= operationsDate.Date
        && (!carNumber.EndDate.HasValue
        || (carNumber.EndDate.HasValue && carNumber.EndDate.Value.Date >= operationsDate.Date)))?.TruckId;

        var unUsedChars = new List<char> { '.', '-', '/', '*', '_', ' ' };

        // 1-й способ
        result = GetCarIdInsidePeriod(TrucksLicensePlates?.Where(carNumber => carNumber.LicensePlate.Trim().Contains(requiredCarNum.Trim()))?.ToList() ?? new());

        if (result != null)
            return ToSearchCar.Invoke();

        // 2-й способ
        result = GetCarIdInsidePeriod(TrucksLicensePlates?.Where(carNumber => requiredCarNum.Trim().Contains(carNumber.LicensePlate.Trim()))?.ToList() ?? new());

        // 3-й способ
        var trimmedCarsNumbers = requiredCarNum?.TrimSomeChars(unUsedChars) ?? string.Empty;
        if (result != null)
            return ToSearchCar.Invoke();

        if (string.IsNullOrEmpty(trimmedCarsNumbers))
        {
            return null;
        }

        result = GetCarIdInsidePeriod(TrucksLicensePlates?.Where(carNumber => carNumber.LicensePlate.TrimSomeChars(unUsedChars).Contains(trimmedCarsNumbers))?.ToList() ?? new());

        if (result != null)
            return ToSearchCar.Invoke();

        // 4-й способ
        result = GetCarIdInsidePeriod(TrucksLicensePlates?.Where(carNumber => trimmedCarsNumbers.Contains(carNumber.LicensePlate.TrimSomeChars(unUsedChars)))?.ToList() ?? new());

        if (result != null)
            return ToSearchCar.Invoke();

        var trimmedCarsNumbersEngl = trimmedCarsNumbers?.ConvertCyrillicToLatin() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedCarsNumbersEngl))
        {
            return null;
        }

        // 5-й способ
        result = GetCarIdInsidePeriod(TrucksLicensePlates?.Where(carNumber => carNumber.LicensePlate.TrimSomeChars(unUsedChars).ConvertCyrillicToLatin().Contains(trimmedCarsNumbersEngl))?.ToList() ?? new());

        if (result != null)
            return ToSearchCar.Invoke();

        // 6-й способ
        result = GetCarIdInsidePeriod(TrucksLicensePlates?.Where(carNumber => trimmedCarsNumbersEngl.Contains(carNumber.LicensePlate.TrimSomeChars(unUsedChars).ConvertCyrillicToLatin()))?.ToList() ?? new());

        return ToSearchCar.Invoke();
    }
    #endregion
}
