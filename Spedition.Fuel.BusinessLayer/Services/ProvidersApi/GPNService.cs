
using System.IO;
using Google.Protobuf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using Spedition.Fuel.BusinessLayer.Configs;
using Spedition.Fuel.BusinessLayer.Models.E100;
using Spedition.Fuel.BusinessLayer.Models.GazProm;
using Spedition.Fuel.BusinessLayer.Models.GazProm.Enums;
using Spedition.Fuel.BusinessLayer.Models.GPN;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.ProvidersApi.Helpers;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Log = Serilog.Log;

namespace Spedition.Fuel.BusinessLayer.Services.ProvidersApi;

public class GPNService : ProvidersApiBase<GPNTransaction>
{
    private readonly IOptions<GPNConfigDemo> optionsDemo;

    public GPNService(
        IWebHostEnvironment env,
        FuelRepositories fuelRepositories,
        IConfiguration configuration,
        IMapper mapper,
        ICountryService countryService,
        ICurrencyService currencyService,
        IEventsTypeService eventsTypeService,
        IDivisionService divisionService,
        ITruckService truckService,
        IOptions<GPNConfig> options,
        IOptions<GPNConfigDemo> optionsDemo)
        : base(env, fuelRepositories, configuration, mapper, countryService, currencyService, eventsTypeService, divisionService, truckService, options)
    {
        this.optionsDemo = optionsDemo;
        ProviderId = 9;
    }

    private List<GPNAzs> ProvidersAzs { get; set; } = new();

    private List<GPNParameter> ProvidersCountries { get; set; } = new();

    public override async Task<bool> GetTransactions()
    {
        //var isTest = Env.IsEnvironment("Test");
        ItemsToMapping = new();

        CleanTempDirectory();

        if (Env.IsDevelopment())
        {
            GPNHelper.baseUrl = optionsDemo?.Value?.BaseUrl ?? string.Empty;
            var testAccounts = new List<ProvidersAccount>
            {
                new ProvidersAccount
                {
                    Login = optionsDemo?.Value?.Login ?? string.Empty,
                    Password = optionsDemo?.Value?.Password ?? string.Empty,
                    Key = optionsDemo?.Value?.Key ?? string.Empty,
                },
            };
            await GetTransactionsInner(testAccounts);
        }
        else
        {
            GPNHelper.baseUrl = Options?.Value?.BaseUrl ?? string.Empty;

            await InitProvidersAccounts();

            if ((ProvidersAccounts?.Count ?? 0) == 0)
            {
                Log.Error($"Ошибка при выполнении поиска в БД аккаунтов, относящихся к провайдеру топлива «GazPromNeft» " +
                          $"внутри метода {nameof(GetTransactions)} в классе {GetType()?.Name ?? string.Empty}. ");
                return false;
            }

            await GetTransactionsInner(ProvidersAccounts);
        }

        return (ItemsToMapping?.Count ?? 0) > 0;
    }

    private async Task GetTransactionsInner(List<ProvidersAccount> accounts)
    {
        var date = DateTime.Now;
        var year = date.Year;
        var month = date.Month;
        var day = date.Day;
        var hour = date.Hour;
        var minute = date.Minute;

        var fileNameBase = $"{day}_{month}_{year}_{hour}_{minute}.json";

        foreach (var account in accounts)
        {
            GPNHelper.login = account?.Login ?? string.Empty;
            GPNHelper.password = account?.Password ?? string.Empty;
            GPNHelper.apiKey = account?.Key ?? string.Empty;

            // 1 - Авторизоваться
            var isAuthenticated = await GPNHelper.GetAuth();

            if (!isAuthenticated)
            {
                $"Ошибка авторизации. Подробности: провайдер Neftika, {NeftikaHelper.Details}. "
                    .LogError(GetType().Name, nameof(GetTransactions));
                continue;
            }

            // 2 - Заполнить коллекцию АЗС
            if ((ProvidersAzs?.Count ?? 0) == 0)
            {
                var azs = await GPNHelper.SendRequest<GPNAzsData>(null, null, false);
                ProvidersAzs?.AddRange(azs?.Result ?? new());
                File.WriteAllText(
                    $"{Env?.WebRootPath ?? string.Empty}\\src\\tmp\\GPN\\AZS\\AZS_{fileNameBase}",
                    JsonConvert.SerializeObject(ProvidersAzs));
            }

            // 3 - Заполнить коллекцию стран
            if ((ProvidersCountries?.Count ?? 0) == 0)
            {
                var countries = await GPNHelper.GetParametersValues(GPNParameters.Country);
                ProvidersCountries?.AddRange(countries?.Result ?? new());
                File.WriteAllText(
                    $"{Env?.WebRootPath ?? string.Empty}\\src\\tmp\\GPN\\Countries\\Countries_{fileNameBase}",
                    JsonConvert.SerializeObject(ProvidersCountries));
            }

            if ((day == 1 || day == 16) && hour == 6) // два раза в месяц 1-го и 6-го числа в 6 утра забираем инф-цию по транзакциям по каждой карте
            {
                // 4 - Получить список топливных карт
                var cards = await GPNHelper.SendRequest<GPNCardData>(null, null, true);

                foreach (var card in cards?.Result?.Where(cardItem => cardItem.Status == "Active")?.ToList() ?? new())
                {
                    var transactions = await GPNHelper.SendRequest<GPNTransactionData>(card.Id, 30, true); // получить список транзакций по заданной карте 
                    transactions?.Result?.ForEach(transaction =>
                    {
                        ItemsToMapping?.AddRange(transaction.MapTransaction());
                    });

                    File.WriteAllText(
                        $"{Env?.WebRootPath ?? string.Empty}\\src\\tmp\\GPN\\ByCard\\{account?.Login ?? string.Empty}_{card?.Number ?? string.Empty}_{fileNameBase}",
                        JsonConvert.SerializeObject(ItemsToMapping));
                }
            }
            else
            {
                var transactions = await GPNHelper.SendRequest<GPNTransactionData>(null, 30, true); // получить список транзакций в рамках договора
                (transactions?.Result ?? new()).ForEach(transaction =>
                {
                    ItemsToMapping?.AddRange(transaction.MapTransaction());
                });

                File.WriteAllText(
                    $"{Env?.WebRootPath ?? string.Empty}\\src\\tmp\\GPN\\ByContract\\{account?.Login ?? string.Empty}_{fileNameBase}",
                    JsonConvert.SerializeObject(ItemsToMapping));
            }
        }
    }

    protected override async Task MappingParsedToDB()
    {
        List<FuelTransaction> result = new();

        await InitDataForMapping();

        // Если коллекция пустая - распарсить транзакции из сохраненных файлов json
        if ((ItemsToMapping?.Count ?? 0) == 0)
        {
            var filePathSegment = string.Empty;

            if ((DateTime.Now.Day == 1 || DateTime.Now.Day == 16) && DateTime.Now.Hour == 6)
            {
                filePathSegment = "ByCard";
            }
            else
            {
                filePathSegment = "ByContract";
            }

            ItemsToMapping = new(ReadDataFromFile<GPNTransaction>(filePathSegment));
        }

        foreach (var itemToMapping in ItemsToMapping ?? new())
        {
            var dbItem = await MappingParsedToDB(itemToMapping);

            if (dbItem != null)
            {
                result.Add(dbItem);
            }
        }

        ItemsToSaveInDB = new(result);
    }

    protected override async Task<FuelTransaction> MappingParsedToDB(IParsedItem parsedReportsItem)
    {
        if (parsedReportsItem == null || parsedReportsItem is not GPNTransaction parsedTransaction)
        {
            $"Ошибка - несоответствие транзакции типу {nameof(GPNTransaction)} !".LogError(GetType().Name, nameof(MappingParsedToDB));
            return null;
        }

        FuelTransaction dbTransaction = new();

        try
        {
            // 0 - Идентификатор транщакции
            dbTransaction.TransactionID = parsedTransaction.Id;

            // 1 - Поставщик топлива
            dbTransaction.ProviderId = ProviderId;

            // 2 - Разновидность топлива
            dbTransaction.FuelTypeId = GetFuelType(parsedTransaction.Product);

            // 9 -Заправленное кол-во
            dbTransaction.Quantity = parsedTransaction.Incoming ? parsedTransaction.Amount : -1 * parsedTransaction.Amount;

            // 4 - Цена за литр
            dbTransaction.Cost = parsedTransaction?.Price ?? 0;

            // 5 - Валюта
            var currencyCode = parsedTransaction.Currency == 810 ? 643 : parsedTransaction.Currency;

            dbTransaction.CurrencyId = GetCurrency(currencyCode);

            // 6 - Общая стоимость
            dbTransaction.TotalCost = parsedTransaction.Incoming ? parsedTransaction.Discount_cost : -1 * parsedTransaction.Discount_cost;

            // 7 - По умолчанию ложь
            dbTransaction.IsCheck = false;

            // 8 - Местоположение заправочной станции, где была осуществлена транзакция
            dbTransaction.CountryId = await GetCountryByAzs(parsedTransaction.Service_center);

            // 9 - Дата и время транзакции
            var operationDay = parsedTransaction.Time;
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
            var foundCardId = await SearchCard(parsedTransaction.Card_number);

            if (!foundCardId.HasValue || foundCardId == 0)
            {
                var message = $"Номер заправочной карты «{parsedTransaction.Card_number ?? string.Empty}» не найден в БД";

                notSuccessItems?.Add(
                    new NotSuccessResponseItemDetailed<NotParsedTransaction>(
                        new NotParsedTransaction
                        {
                            CardNumber = parsedTransaction.Card_number,
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
                            NotFuelType = parsedTransaction.Product ?? "«Разновидность услуги не определена»", // наименование услуги из отчета о реализации
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
            return null; // продолжить парсить транзакции
        }
    }

    private int? GetCurrency(int currencyCode)
    {
        return Currencies?.FirstOrDefault(currency => currency.Code == currencyCode)?.Id ?? null;
    }

    private async Task<int?> GetCountryByAzs(string serviceCenter)
    {
        int? countryId = null;

        if (string.IsNullOrEmpty(serviceCenter))
        {
            $"Страна / местоположение АЗС не может быть определено. Подробности: наименгование сервисного центра («{nameof(serviceCenter)}») пустое. "
                .LogWarning(GetType().Name, nameof(GetCountry));
            return null;
        }

        if ((ProvidersAzs?.Count ?? 0) == 0)
        {
            ReadDataFromFile<GPNAzs>("AZS");
        }

        var countryCode = GetCountryCode(serviceCenter);

        if (!string.IsNullOrEmpty(countryCode))
        {
            var countryNameToSearch = countryCode == "BLR" ? "BY" : countryCode;
            countryId = await GetCountry(countryNameToSearch);
        }

        if (countryId == null)
        {
            if ((ProvidersCountries?.Count ?? 0) == 0)
            {
                ReadDataFromFile<GPNParameter>("Countries");
            }

            var countryName = ProvidersCountries?.FirstOrDefault(country => country.Id == countryCode)?.Value ?? string.Empty;
            countryId = await GetCountry(countryName);
        }

        return countryId;
    }

    private string GetCountryCode(string serviceCenter)
    {
        return ProvidersAzs?.FirstOrDefault(azs => azs.Id.Equals(serviceCenter, StringComparison.InvariantCultureIgnoreCase))
            ?.CountryCode ?? string.Empty;
    }

    private List<TResult> ReadDataFromFile<TResult>(string filePathSegment)
        where TResult : class
    {
        List<TResult> jsonItems = new();
        List<string> filesPathes = new();

        filesPathes = Directory.GetFiles($"{Env?.WebRootPath ?? string.Empty}\\src\\tmp\\GPN\\{filePathSegment}")?.ToList() ?? new();

        foreach (var filePath in filesPathes)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    using var reader = new StreamReader(filePath);
                    while (!reader.EndOfStream)
                    {
                        var json = reader.ReadToEnd();
                        jsonItems.AddRange(JsonConvert.DeserializeObject<List<TResult>>(json));
                    }
                }
                catch (Exception exc)
                {
                    exc.LogError(
                        nameof(GPNService),
                        nameof(ReadDataFromFile),
                        $"Ошибка при десериализации ответа из файла «{filePath}» в тип {typeof(TResult).Name}. ");
                    throw;
                }
            }
        }

        return jsonItems;
    }

    private void CleanTempDirectory()
    {
        foreach (var directory in Directory.GetDirectories($"{Env?.WebRootPath ?? string.Empty}\\src\\tmp\\GPN")?.ToList() ?? new())
        {
            Directory.GetFiles(directory)?.ToList()?.ForEach(file => File.Delete(file));
        }
    }
}
