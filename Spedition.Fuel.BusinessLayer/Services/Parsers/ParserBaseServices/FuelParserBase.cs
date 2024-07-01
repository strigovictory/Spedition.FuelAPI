using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Diagnostics.Metrics;
using System.Transactions;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NPOI.SS.Formula.Functions;
using NPOI.Util;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Utilities.Collections;
using Spedition.Fuel.BusinessLayer.Enums;
using Spedition.Fuel.BusinessLayer.Helpers;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Shared;
using Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Transport;
using Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Finance;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Geo;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Office;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Trips;
using Spedition.Fuel.Shared.Entities;
using Spedition.Fuel.Shared.Helpers;
using Spedition.Office.Shared.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Log = Serilog.Log;

namespace Spedition.Fuel.BusinessLayer.Services.Parsers.ParserBaseServices;

public abstract class FuelParserBase<TParsed, TSuccess, TNotSuccess, TSearch>
    : FuelParserOuterLibBase<TParsed, FuelTransactionShortResponse, NotParsedTransaction, FuelTransaction>,
    ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction>
    where TParsed : class, IParsedItem
{
    private readonly ICountryService countryService;
    private readonly ICurrencyService currencyService;
    private readonly IEventsTypeService eventsTypeService;
    private readonly ITruckService truckService;
    private readonly IDivisionService divisionService;

    public FuelParserBase(
    IWebHostEnvironment env,
    FuelRepositories fuelRepositories,
    IConfiguration configuration,
    IMapper mapper,
    ICountryService countryService,
    ICurrencyService currencyService,
    IEventsTypeService eventsTypeService,
    IDivisionService divisionService,
    ITruckService truckService,
    OuterLibrary outerLibrary)
    : base(env, fuelRepositories, configuration, mapper, outerLibrary)
    {
        this.countryService = countryService;
        this.currencyService = currencyService;
        this.eventsTypeService = eventsTypeService;
        this.truckService = truckService;
        this.divisionService = divisionService;
    }

    #region  Properties / fields
    public int ProviderId { get; set; }

    public virtual List<int> ProvidersId { get; protected set; }

    protected List<int> FoundCardsIds { get; set; } = new();

    /// <summary>
    /// Топливные карты текущего поставщика.
    /// </summary>
    protected List<FuelCard> ProvidersCards { get; set; }

    protected List<CurrencyResponse> Currencies { get; private set; }

    protected List<CountryResponse> Countries { get; private set; }

    protected List<DivisionResponse> Divisions { get; private set; }

    protected List<TruckResponse> Trucks { get; private set; }

    protected List<TrucksLicensePlateResponse> TrucksLicensePlates { get; private set; }

    protected List<FuelProvider> Providers { get; private set; }

    protected List<FuelCardsAlternativeNumber> AlternativeNumbers { get; private set; }

    protected List<FuelCardsCountry> FuelCardsCountries { get; set; }

    /// <summary>
    /// Коллекция Т-экземпляров, которые являются дубликатами и не подлежат обновению в БД, т.к. идентичны тем, которые есть в БД.
    /// </summary>
    public List<FuelTransaction> ExistedInstancesNotModified { get; set; } = new();

    /// <summary>
    /// Коллекция Т-экземпляров, которые являются дубликатами, но и не могут быть обновены в БД из-за невозможности менять поля, которые влияют на другие данные в БД.
    /// </summary>
    public List<FuelTransaction> ExistedInstancesCantUpdate { get; set; } = new();

    /// <summary>
    /// Коллекция Т-экземпляров, которые являются дубликатами и подлежат обновлению в БД.
    /// </summary>
    public List<FuelTransaction> ExistedInstancesToUpdate { get; set; } = new();

    /// <summary>
    /// Коллекция транзакций, которые были определены как дубликаты.
    /// </summary>
    public override List<FuelTransaction> ExistedInstances
    {
        get => ExistedInstancesNotModified
            .Union(ExistedInstancesCantUpdate)
            .Union(ExistedInstancesToUpdate)
            .ToList();
    }
    #endregion

    #region Init / overriden methods
    protected override List<NotSuccessResponseItemDetailed<NotParsedTransaction>> GetNotSuccessItems()
    {
        List<NotSuccessResponseItemDetailed<NotParsedTransaction>> result = new(notSuccessItems);

        ExistedInstancesNotModified?.ForEach(existedItem =>
        result?.Add(new NotSuccessResponseItemDetailed<NotParsedTransaction>(
            mapper.Map<NotParsedTransaction>(existedItem),
            $"Повторная загрузка отчета. Идентичная транзакция уже была загружена в БД. ")));

        ExistedInstancesCantUpdate?.ForEach(existedItem =>
        result?.Add(new NotSuccessResponseItemDetailed<NotParsedTransaction>(
            mapper.Map<NotParsedTransaction>(existedItem),
            $"Транзакция не может быть обновлена в БД по причине наличия ссылки на отчет водителя. ")));

        // добавить с ошибкой
        result?.AddRange(ErrorItems);

        return result;
    }

    protected override async Task<bool> UpdateItemsInDB()
    {
        try
        {
            var commands = await ConstructUpdateCommands();

            // Если список инструкций не пустой - выполнить транзакцию
            if ((commands?.Count ?? 0) > 0)
            {
                var counter = await DoDbComand(commands);

                if (counter > 0 && ExistedInstancesToUpdate?.Count == counter)
                {
                    NotifyMessage += $"В базе данных были обновлены экземпляры в количестве {commands?.Count ?? 0} шт. !";
                    SecondarySuccessItems?.AddRange(ExistedInstancesToUpdate?.Select(dbItem => mapper.Map<FuelTransactionShortResponse>(dbItem)));
                }
                else if (counter > 0 && ExistedInstancesToUpdate?.Count > counter)
                {
                    NotifyMessage += $"Из {ExistedInstancesToUpdate?.Count ?? 0} шт. экземпляров, " +
                               $"подлежащих обновлению в базе данных, было сохранено {counter} шт. !" +
                               $"Оставшиеся {(ExistedInstancesToUpdate?.Count ?? 0) - counter} шт. не были сохранены " +
                               $"по причине ошибки на уровне БД. ";
                    Log.Error(NotifyMessage);
                }
                else if (counter == 0)
                {
                    NotifyMessage += $"В базе данных не было обновлено ни одного экземпляра по причине ошибки на уровне БД. ";
                    Log.Error(NotifyMessage);
                }
            }

            return true;
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(UpdateItemsInDB), GetType().FullName);
            throw;
        }
    }

    protected override async Task InitDataForMapping()
    {
        InitExternalCollections();
        await InitSecondaryCollections();
        await UpdateProvidersCards();
    }

    /// <summary>
    /// Операция выделена в отдельный метод для вызова из насдледников парсера после пополнения коллекции топливных карт (напр.Аднок).
    /// </summary>
    protected virtual async Task UpdateProvidersCards()
    {
        ProvidersCards = await fuelRepositories?.Cards?.FindRangeAsync(card => card.ProviderId == ProviderId);
    }

    protected virtual async Task InitSecondaryCollections()
    {
        Providers = await fuelRepositories?.Providers?.GetAsync();
        AlternativeNumbers = await fuelRepositories?.CardsAlternativeNumbers?.GetAsync();
    }
    
    protected void InitExternalCollections()
    {
        Currencies = (currencyService?.Get())?.GetAwaiter().GetResult() ?? new();
        Countries = (countryService?.Get())?.GetAwaiter().GetResult() ?? new();
        Divisions = (divisionService?.GetDivisions())?.GetAwaiter().GetResult() ?? new();
        Trucks = (truckService?.GetTrucks())?.GetAwaiter().GetResult() ?? new();
        TrucksLicensePlates = (truckService?.GetTrucksLicensePlates())?.GetAwaiter().GetResult() ?? new();
    }

    public override async Task<bool> CheckIsInstanceExist(FuelTransaction checkedTransaction)
    {
        FuelTransaction foundTransaction = default;

        // Делегат для выполнения основной проверки, является ли тр-ция дубликатом
        var mainConditions = async () => 
            (await fuelRepositories?.Transactions?.FindRangeAsync(transaction => 
            transaction.Id != checkedTransaction.Id
            && transaction.CardId == checkedTransaction.CardId
            && transaction.Quantity == (checkedTransaction.Quantity ?? 0)));

        // Делегат для выполнения окончательной проверки, является ли тр-ция дубликатом
        var dateCondition = (FuelTransaction transactionItem) => transactionItem.OperationDate.IsDatesEquel(checkedTransaction.OperationDate);

        // Проверить на уникальность - является ли транзакция дубликатом
        try
        {
            foundTransaction = (await mainConditions()).FirstOrDefault(transaction => dateCondition(transaction));
        }
        catch (Exception exc)
        {
            NotifyMessage += $"Error transaction: {checkedTransaction?.ToString() ?? string.Empty}";
            exc.LogError(nameof(CheckIsInstanceExist), GetType().FullName, NotifyMessage);
            throw;
        }

        // Флаг, сигнализирующий о том, что тр-ция является дубликатом
        var isInstanceExist = (foundTransaction?.Id ?? 0) > 0;

        if (isInstanceExist) // Если тр-ция дубликат
        {
            // Флаг, сигнализирующий о том, что тип отчета новый
            var isReportKindNew = ((!foundTransaction.IsMonthly.HasValue || !foundTransaction.IsMonthly.Value) && IsMonthly.HasValue && IsMonthly.Value)
                || ((!foundTransaction.IsDaily.HasValue || !foundTransaction.IsDaily.Value) && (!IsMonthly.HasValue || !IsMonthly.Value));

            // Флаг, сигнализирующий о том, что тр-ция имеет заполненную ссылку на отчет водителя
            var isDriverReportHasValue = foundTransaction.DriverReportId.HasValue;

            // Флаг, сигнализирующий о том, что у тр-ции изменились данные (которые не участвуют в качестве параметров при проверке тр-ции на идентичность)
            var isDataChanged = foundTransaction.FuelTypeId != checkedTransaction.FuelTypeId
                || (foundTransaction.Cost ?? 0) != (checkedTransaction.Cost.HasValue ? checkedTransaction.Cost.Value : 0)
                || (foundTransaction.TotalCost ?? 0) != (checkedTransaction.TotalCost.HasValue ? checkedTransaction.TotalCost.Value : 0)
                || (foundTransaction.CurrencyId ?? 0) != (checkedTransaction.CurrencyId.HasValue ? checkedTransaction.CurrencyId.Value : 0)
                || (foundTransaction.CountryId ?? 0) != (checkedTransaction.CountryId.HasValue ? checkedTransaction.CountryId.Value : 0)
                || (foundTransaction.TransactionID ?? string.Empty) != (checkedTransaction.TransactionID ?? string.Empty);

            // Делегат для заполнения типа отчета у тр-ции
            var fillReportType = () =>
            {
                checkedTransaction.IsMonthly = IsMonthly.HasValue && IsMonthly.Value
                    ? true
                    : foundTransaction.IsMonthly;

                checkedTransaction.IsDaily = !IsMonthly.HasValue || !IsMonthly.Value
                    ? true
                    : foundTransaction.IsDaily;
            };

            var isMustUpdate = false; // Флаг, указывающий на то, что тр-я подлежит обновлению в БД

            // Обновить поля тр-ции
            if (!isReportKindNew && !isDataChanged) // если тип отчета и данные старые
            {
                ExistedInstancesNotModified?.Add(checkedTransaction);
            }
            else if (isReportKindNew && isDataChanged) // если тип отчета и данные новые
            {
                if (isDriverReportHasValue) // если есть ссылка на водительский отчет
                {
                    ExistedInstancesCantUpdate?.Add(checkedTransaction);
                    checkedTransaction = foundTransaction; // чтобы не затерлись поля, которые нельзя менять из-за наличия ссылки на отчет водителя
                }

                isMustUpdate = true;
            }
            else if (isDataChanged) // если тип отчета старый, а данные новые
            {
                if (isDriverReportHasValue) // если есть ссылка на водительский отчет
                {
                    ExistedInstancesCantUpdate?.Add(checkedTransaction);
                }
                else
                {
                    isMustUpdate = true;
                }
            }
            else // если тип отчета новый, а данные старые
            {
                isMustUpdate = true;
            }

            if (isMustUpdate)
            {
                checkedTransaction.Id = foundTransaction.Id; // заполнить идентификатор тр-ции
                checkedTransaction.IsCheck = foundTransaction.IsCheck; // заполнить флаг о проверке тр-ции
                fillReportType(); // заполнить ссылку на новый тип отчета
                ExistedInstancesToUpdate?.Add(checkedTransaction);
            }
        }
        else // если транзакция новая, заполнить ссылку на тип отчета
        {
            checkedTransaction.IsMonthly = IsMonthly.HasValue && IsMonthly.Value ? true : default;
            checkedTransaction.IsDaily = !IsMonthly.HasValue || !IsMonthly.Value ? true : default;
        }

        return isInstanceExist;
    }

    protected override async Task<List<string>> ConstructInsertCommands()
    {
        List<string> commands = new();

        foreach (var item in NewItemsToAddInDB ?? new())
        {
            try
            {
                // получение наименования таблицы в БД из атрибута к модели
                var tableName = item.GetType().GetTableAttributeValueName() ?? nameof(item);
                var isCheck = item.IsCheck ? 1 : 0;
                var isDaily = item.IsDaily.HasValue ? (item.IsDaily.Value ? "1" : "0") : "null";
                var isMonthly = item.IsMonthly.HasValue ? (item.IsMonthly.Value ? "1" : "0") : "null";
                var currencyId = item.CurrencyId == null ? "null" : item.CurrencyId.ToString();
                var cardId = item.CardId.ToString();
                var countryId = item.CountryId == null ? "null" : item.CountryId.ToString();
                var transactionId = item.TransactionID == null ? "null" : string.Concat("'", item.TransactionID, "'");
                var operationDate = DateTimeFormat(item.OperationDate);
                var quantity = $"CAST('{item.Quantity?.ToString()?.Replace(",", ".") ?? "0"}' AS NUMERIC(20,4))";
                var cost = $"CAST('{item.Cost?.ToString()?.Replace(",", ".") ?? "0"}' AS NUMERIC(20,4))";
                var totalCost = $"CAST('{item.TotalCost?.ToString()?.Replace(",", ".") ?? "0"}' AS NUMERIC(20,4))";
                commands.Add(
                    SenderToDb.InsertDataToDB(
                        table: $"{tableName}",
                        columns: string.Concat(
                            $"{item.GetType().GetProperty(nameof(item.ProviderId)).GetRequiredAttributeValue<FuelTransaction, ColumnAttribute>((ColumnAttribute attr) => attr.Name)}, ",
                            $"{nameof(item.OperationDate)}, ",
                            $"{item.GetType().GetProperty(nameof(item.FuelTypeId)).GetRequiredAttributeValue<FuelTransaction, ColumnAttribute>((ColumnAttribute attr) => attr.Name)}, ",
                            $"{nameof(item.Quantity)}, ",
                            $"{nameof(item.Cost)}, ",
                            $"{nameof(item.CurrencyId)}, ",
                            $"{nameof(item.TotalCost)}, ",
                            $"{nameof(item.CardId)}, ",
                            $"{nameof(item.IsCheck)}, ",
                            $"{nameof(item.CountryId)}, ",
                            $"{nameof(item.TransactionID)}, ",
                            $"{nameof(item.IsDaily)}, ",
                            $"{nameof(item.IsMonthly)} "),
                        values: string.Concat(
                            $"{item.ProviderId}, ",
                            $"{operationDate}, ",
                            $"{item.FuelTypeId}, ",
                            $"{quantity}, ",
                            $"{cost}, ",
                            $"{currencyId}, ",
                            $"{totalCost}, ",
                            $"{cardId}, ",
                            $"{isCheck}, ",
                            $"{countryId}, ",
                            $"{transactionId}, ",
                            $"{isDaily}, ",
                            $"{isMonthly} ")));
            }
            catch (Exception exc)
            {
                exc.LogError(nameof(ConstructInsertCommands), GetType().FullName);
                throw;
            }
        }

        return commands;
    }

    protected override async Task<List<string>> ConstructUpdateCommands()
    {
        List<string> commands = new();

        foreach (var item in ExistedInstancesToUpdate ?? new())
        {
            try
            {
                // получение наименования таблицы в БД из атрибута к модели
                var tableName = item.GetType().GetTableAttributeValueName() ?? nameof(item);
                var isCheck = item.IsCheck ? 1 : 0;
                var isDaily = item.IsDaily.HasValue ? (item.IsDaily.Value ? "1" : "0") : "null";
                var isMonthly = item.IsMonthly.HasValue ? (item.IsMonthly.Value ? "1" : "0") : "null";
                var currencyId = item.CurrencyId == null ? "null" : item.CurrencyId.ToString();
                var cardId = item.CardId.ToString();
                var countryId = item.CountryId == null ? "null" : item.CountryId.ToString();
                var transactionId = item.TransactionID == null ? "null" : string.Concat("'", item.TransactionID, "'");
                var operationDate = DateTimeFormat(item.OperationDate);
                var quantity = $"CAST('{item.Quantity?.ToString()?.Replace(",", ".") ?? "0"}' AS NUMERIC(20,4))";
                var cost = $"CAST('{item.Cost?.ToString()?.Replace(",", ".") ?? "0"}' AS NUMERIC(20,4))";
                var totalCost = $"CAST('{item.TotalCost?.ToString()?.Replace(",", ".") ?? "0"}' AS NUMERIC(20,4))";
                commands.Add(
                    SenderToDb.UpdateDataInDB(
                        table: $"{tableName}",
                        columnsValues: string.Concat(
                            $"{item.GetType().GetProperty(nameof(item.ProviderId)).GetRequiredAttributeValue<FuelTransaction, ColumnAttribute>((ColumnAttribute attr) => attr.Name)} = {item.ProviderId}, ",
                            $"{nameof(item.OperationDate)} = {operationDate}, ",
                            $"{item.GetType().GetProperty(nameof(item.FuelTypeId)).GetRequiredAttributeValue<FuelTransaction, ColumnAttribute>((ColumnAttribute attr) => attr.Name)} = {item.FuelTypeId}, ",
                            $"{nameof(item.Quantity)} = {quantity}, ",
                            $"{nameof(item.Cost)} = {cost}, ",
                            $"{nameof(item.CurrencyId)} = {currencyId}, ",
                            $"{nameof(item.TotalCost)} = {totalCost}, ",
                            $"{nameof(item.CardId)} = {cardId}, ",
                            $"{nameof(item.IsCheck)} = {isCheck}, ",
                            $"{nameof(item.CountryId)} = {countryId}, ",
                            $"{nameof(item.TransactionID)} = {transactionId}, ",
                            $"{nameof(item.IsDaily)} = {isDaily}, ",
                            $"{nameof(item.IsMonthly)} = {isMonthly} "),
                        predicate: $"{nameof(item.Id)} = {item.Id}"));
            }
            catch (Exception exc)
            {
                exc.LogError(nameof(ConstructUpdateCommands), GetType().FullName);
                throw;
            }
        }

        return commands;
    }

    #endregion

    #region Get-Methods

    /// <summary>
    /// Метод для получения страны, в которой была осуществлена транзакция.
    /// </summary>
    /// <param Name="countryCode">Идентификатор местоположения транзакции по заправке из тела таблицы из отчета Excel.</param>
    /// <returns>Идентификатор страны.</returns>
    protected int? GetCountry(int countryCode)
    {
        return FuelCardsCountries?.FirstOrDefault(_ => _.CountryCode == countryCode)?.CountryId;
    }

    /// <summary>
    /// Метод для получения разновидности оказанной услуги (топливо, adblue, прочие).
    /// </summary>
    /// <param Name="fuelTypeName">Наименование услуги из тела таблицы.</param>
    /// <returns>Идентификатор услуги из таблицы tRideCostCategories.</returns>
    protected virtual int GetFuelType(string fuelTypeName)
    {
        try
        {
            if (string.IsNullOrEmpty(fuelTypeName))
            {
                Log.Warning($"Услуга имеет пустое наименование. ");
                return 13; // Прочее
            }
            else if ((fuelTypeName?.ToUpper()?.Contains(Enum.GetName(FuelKind.diesel).ToUpper()) ?? false)
                || (fuelTypeName?.ToUpper()?.Contains(Enum.GetName(FuelKind.dt).ToUpper()) ?? false)
                || (fuelTypeName?.ToUpper()?.Contains(Enum.GetName(FuelKind.дт).ToUpper()) ?? false)
                || (fuelTypeName?.ToUpper()?.Contains(Enum.GetName(FuelKind.act).ToUpper()) ?? false)
                || (fuelTypeName?.ToUpper()?.Contains(Enum.GetName(FuelKind.топливо).ToUpper()) ?? false)
                || (fuelTypeName?.ToUpper()?.Contains("OLEJ NAPEDOWY") ?? false)
                || (fuelTypeName?.ToUpper()?.Contains("ULT ON") ?? false)
                || (fuelTypeName?.ToUpper()?.Contains("ДТ-Л") ?? false))
            {
                return 2;
            }
            else if ((fuelTypeName?.ToLower()?.Contains(Enum.GetName(FuelKind.adblue)) ?? false)
                || (fuelTypeName?.ToUpper()?.Contains("AD BLUE") ?? false)
                || (fuelTypeName?.ToUpper()?.Contains(Enum.GetName(FuelKind.адблю).ToUpper()) ?? false)
                || (fuelTypeName?.ToUpper()?.Contains("АД БЛЮ") ?? false))
            {
                return 10;
            }
            else
            {
                return 13; // Прочие
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(GetFuelType), GetType().FullName);
            throw;
        }
    }

    /// <summary>
    /// Метод для получения идентификатора карты по ее номеру.
    /// </summary>
    /// <param Name="number">Номер заправочной карты.</param>
    /// <returns>Идентификатор заправочной карты.</returns>
    protected virtual async Task<int?> SearchCard(string number)
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
            && (card.Number.Contains(number) || number.Contains(card.Number)));

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
            && !string.IsNullOrWhiteSpace(card.Number?.TrimSomeChars(unUsedChars) ?? string.Empty)
            && (card.Number.TrimSomeChars(unUsedChars).Contains(cardsTrimServicesChars) 
            || cardsTrimServicesChars.Contains(card.Number.TrimSomeChars(unUsedChars))));

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
            card => !string.IsNullOrEmpty(card.Number?.TrimSomeChars(unUsedChars)?.ConvertCyrillicToLatin() ?? string.Empty)
            && !string.IsNullOrWhiteSpace(card.Number?.TrimSomeChars(unUsedChars)?.ConvertCyrillicToLatin() ?? string.Empty)
            && (card.Number.TrimSomeChars(unUsedChars).ConvertCyrillicToLatin().Contains(cardsTrimServicesCharsEngl)
            || cardsTrimServicesCharsEngl.Contains(card.Number.TrimSomeChars(unUsedChars).ConvertCyrillicToLatin())));

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
            card => !string.IsNullOrEmpty(card.Number?.TrimSomeChars(unUsedChars)?.ConvertCyrillicToLatin()?.TrimSomeCharsExceptLastSixth(charZero) ?? string.Empty)
            && !string.IsNullOrWhiteSpace(card.Number?.TrimSomeChars(unUsedChars)?.ConvertCyrillicToLatin()?.TrimSomeCharsExceptLastSixth(charZero) ?? string.Empty)
            && (card.Number.TrimSomeChars(unUsedChars).ConvertCyrillicToLatin().TrimSomeCharsExceptLastSixth(charZero).Contains(cardsNumTrimZero)
            || cardsNumTrimZero.Contains(card.Number.TrimSomeChars(unUsedChars).ConvertCyrillicToLatin().TrimSomeCharsExceptLastSixth(charZero))));

        if (cardIsValid(foundCard))
        {
            return foundCard?.Id;
        }

        // 6-й способ - Поиск в альтернативных номерах
        return SearchCardInAlternativeNumbers(number);
    }

    /// <summary>
    /// Метод для выполнения поиска в БД топливной карты с заданным номером.
    /// </summary>
    /// <returns>Истина, если карта найдена и наоборот.</returns>
    protected async Task<bool> SearchCardByQuery(string requiredCardsNum)
    {
        FoundCardsIds = new();

        var tablesSchemaName = typeof(FuelCard)?.GetTableAttributesValues();

        var command = (tablesSchemaName.Value.schema + "." + tablesSchemaName.Value.table)
            .FilterDataString(props: new List<(string propName, string propValue)>
            {
                ("FK_FuelCardTypeID", ProviderId.ToString()),
                (nameof(FuelCard.Number), requiredCardsNum.TrimSomeChars(new List<char>{ '\''})),
            });

        if (!string.IsNullOrEmpty(command))
        {
            using var connection = new SqlConnection(ConnectionString);

            connection.Open();

            try
            {
                FoundCardsIds = (await connection.QueryAsync<FuelCard>(command))?.Select(card => card.Id)?.ToList() ?? new();

                if ((FoundCardsIds?.Count ?? 0) > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exc)
            {
                exc.LogError(nameof(SearchCard), GetType().FullName);
                throw;
            }
            finally
            {
                connection?.Close();
            }
        }
        else
        {
            NotifyMessage = string.Concat($"Произошла ошибка на этапе формирования " +
                                    $"набора инструкций для поиска номеров топливных карт в базе данных");
            return false;
        }
    }

    /// <summary>
    /// Метод для поиска идентификатора карты по ее номеру в таблице с альтернативными номерами.
    /// </summary>
    /// <param Name="number">Номер заправочной карты.</param>
    /// <returns>Идентификатор заправочной карты.</returns>
    protected virtual int? SearchCardInAlternativeNumbers(string number)
    {
        if (string.IsNullOrEmpty(number) || string.IsNullOrWhiteSpace(number))
        {
            return null;
        }

        return AlternativeNumbers?.FirstOrDefault(alterNumber => alterNumber.Number.ToLower().Contains(number.ToLower()))?.CardId;
    }

    /// <summary>
    /// Метод для поиска идентификатора страны, в которой была осуществлена заправка по ее наименованию.
    /// </summary>
    /// <param Name="countryCode">Наименование страны из отчета.</param>
    /// <returns>Идентификатор страны.</returns>
    protected async Task<int?> GetCountry(string countryCode)
    {
        var isNotEmpty = (string value) => !string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value) && value.Length > 1;

        return isNotEmpty(countryCode)
            ? Countries?
            .FirstOrDefault(country => 
            (isNotEmpty(country?.CountryCode)
            && country.CountryCode.ToLower().Equals(countryCode.ToLower()))
            || (isNotEmpty(country?.NameEng)
            && (country.NameEng.ToLower().Equals(countryCode.ToLower()) 
            || countryCode.ToLower().Contains(country.NameEng.ToLower())))
            || (isNotEmpty(country?.CountryCode)
            && (country.CountryCode.ToLower().Contains(countryCode.ToLower()) 
            || countryCode.ToLower().Contains(country.CountryCode.ToLower()))))?.Id ?? null
            : null;
    }

    protected int? GetCountry()
    {
        return Providers?.FirstOrDefault(provider => ProviderId == provider.Id)?.CountryId;
    }

    protected async Task<int?> GetCurrency(string currName)
    {
        return string.IsNullOrEmpty(currName?.Trim()) || string.IsNullOrWhiteSpace(currName?.Trim())
            ? null
            : Currencies?.FirstOrDefault(currency => 
            currency.Name.Trim().ToLower().Contains(currName.Trim().ToLower())
            || currName.Trim().ToLower().Contains(currency.Name.Trim().ToLower())
            || currency.FullName.Trim().ToLower().Contains(currName.Trim().ToLower())
            || currName.Trim().ToLower().Contains(currency.FullName.Trim().ToLower())
            || currency.FullNameEng.Trim().ToLower().Contains(currName.Trim().ToLower())
            || currName.Trim().ToLower().Contains(currency.FullNameEng.Trim().ToLower()))?.Id ?? null;
    }

    /// <summary>
    /// Метод для конвертации двух переменных, содержащих дату и время транзакции в формат DateTime.
    /// </summary>
    /// <param Name="dateInner">День, когда была произведена транзакция.</param>
    /// <param Name="timeInner">Время транзакции.</param>
    /// <returns>Дата и время заправки в формате DateTime.</returns>
    protected DateTime GetOperationDate(DateTime dateInner, string timeInner)
    {
        if (dateInner == default)
            return default;

        DateTime result = default;
        var time = timeInner?.Trim() ?? string.Empty;

        var hourStr = "0";
        var minuteStr = "0";
        var secundeStr = "0";

        var firstColomn = time.IndexOf(':');
        var secondColomn = time.LastIndexOf(':');

        // Форматирование времени
        var timeLength = time?.Length ?? 0;

        // Если секунды не указаны
        if (firstColomn != -1 && secondColomn != -1)
        {
            if (firstColomn == secondColomn)
            {
                minuteStr = time.Substring(timeLength - 2, 2);
                hourStr = time.Substring(timeLength - 5, 2);
                secundeStr = "0";
            }
            else if (firstColomn != secondColomn)
            {
                secundeStr = time.Substring(timeLength - 2, 2);
                minuteStr = time.Substring(timeLength - 5, 2);
                hourStr = time.Substring(timeLength - 8, 2);
            }
        }

        // Парсинг строки в дату
        try
        {
            // конкатенация даты и времени с последующим форматированием в формат DateTime
            if (int.TryParse(hourStr, out var hour)
                && int.TryParse(minuteStr, out var minute)
                && int.TryParse(secundeStr, out var second))
            {
                Log.Debug($"dateInner item = day: {dateInner.Day}, month: {dateInner.Month}, year: {dateInner.Year}, hour: {hour}, minute: {minute}, second: {second}");
                result = new DateTime(day: dateInner.Day, month: dateInner.Month, year: dateInner.Year, hour: hour, minute: minute, second: second);
            }
            else
            {
                NotifyMessage += $"Произошла ошибка при попытке преобразовать значение «{dateInner} {timeInner}» в формат даты! ";
                NotifyMessage.LogError(GetType().Name, nameof(GetOperationDate));
            }
        }
        catch (Exception exc)
        {
            NotifyMessage += $"Произошла ошибка при попытке преобразовать значение «{dateInner} {timeInner}» в формат даты! ";
            exc.LogError(nameof(ParseFile), GetType().FullName, NotifyMessage);
            throw;
        }

        return result;
    }

    protected DateTime GetOperationDate(DateTime dateInner, DateTime timeInner)
    {
        if (dateInner == default)
            return default;
        else
            return new DateTime(
                day: dateInner.Day,
                month: dateInner.Month,
                year: dateInner.Year,
                hour: timeInner.Hour,
                minute: timeInner.Minute,
                second: timeInner.Second);
    }

    protected DateTime GetOperationDate(string dateTimeStr)
    {
        if (!string.IsNullOrEmpty(dateTimeStr)
            && !string.IsNullOrWhiteSpace(dateTimeStr)
            && DateTime.TryParse(dateTimeStr, out var date)
            && date != default)
        {
            return new DateTime(
                day: date.Day,
                month: date.Month,
                year: date.Year,
                hour: date.Hour,
                minute: date.Minute,
                second: date.Second);
        }
        else
        {
            return default;
        }
    }
    #endregion

    #region Post-Methods
    protected async Task<int> GetDivision(int truckId)
    {
        return (await truckService.GetTruck(truckId)).Id;
    }

    /// <summary>
    /// Метод для сохранения в БД новой топливной карты - проверка на идентичность не нужна,
    /// т.к. добавление происходит после того, как парсером не был найден номер карты(он же номер тягача) в БД.
    /// </summary>
    /// <param Name="truck">Тягач</param>
    /// <param Name="providerId">Идентификатор провайдера.</param>
    /// <param Name="transactionDate">Дата транзакции.</param>
    /// <returns>Сохраненная в БД заправочная карта.</returns>
    protected async Task<FuelCard> AddFuelCard(TruckResponse truck, int providerId, DateTime transactionDate)
    {
        FuelCard result = null;
        try
        {
            // 0 - Определение идентификатора подразделения
            var divisionId = await GetDivision(truck.Id);

            // 1 - Добавить новую карту в БД
            result = fuelRepositories?.Cards?.Add(
                new FuelCard
                {
                    CarId = truck?.Id ?? null,
                    DivisionID = divisionId,
                    ProviderId = providerId,
                    Number = truck?.LicensePlate ?? string.Empty, // вносим так, как указано в таблице TCars
                    ExpirationDate = null,
                    ExpirationMonth = null,
                    ExpirationYear = null,
                    ReceiveDate = new DateTime(day: 1, month: transactionDate.Month, year: transactionDate.Year, hour: 0, minute: 0, second: 0),
                    IssueDate = new DateTime(day: 1, month: transactionDate.Month, year: transactionDate.Year, hour: 0, minute: 0, second: 0),
                    IsReserved = false,
                    IsArchived = false,
                });

            // 2 - Добавить событие Тягач на новую добавленную в БД карту
            var eventsTypes = await eventsTypeService.Get();

            var cardEvent = new FuelCardsEvent
            {
                CardId = result.Id,
                CarId = truck.Id,
                EventTypeId = EventsTypeName.Тягач.GetEventTypeId(eventsTypes),
                StartDate = transactionDate.Date, // дата начала события ТЯГАЧ равна дате транзакции
                FinishDate = null,
                Comment = string.Empty,
                ModifiedOn = DateTime.Now,
                ModifiedBy = UserName ?? string.Empty,
            };

            var createdEvent = AddCardsEvent(cardEvent);
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(AddFuelCard), GetType().FullName);
            throw;
        }

        return result;
    }

    /// <summary>
    /// Метод для сохранения в бд нового события изменения статуса топливной карты.
    /// </summary>
    /// <param Name="fuelCardEvent">Событие изменения статуса топливной карты.</param>
    /// <returns>Сохраненное в бд событие.</returns>
    protected FuelCardsEvent AddCardsEvent(FuelCardsEvent fuelCardEvent)
    {
        FuelCardsEvent result = new();

        try
        {
            result = fuelRepositories.CardsEvents.Add(fuelCardEvent) ?? new();
        }
        catch (Exception exc)
        {
            NotifyMessage += fuelRepositories?.CardsEvents?.Message ?? string.Empty;
            exc.LogError(GetType().FullName, nameof(AddCardsEvent), NotifyMessage);
            throw;
        }

        NotifyMessage += fuelRepositories.CardsEvents.Message ?? string.Empty;

        return result;
    }

    public virtual async Task SaveFilledTransactions(List<NotParsedTransactionFilled> transactions)
    {
        NotifyMessage = string.Empty;

        // 1 - Конвертация дозаполненных пользователем транзакций и сохранение новых топливных карт и альтернативных номеров при необх-ти
        ItemsToSaveInDB = await MapTransaction(transactions ?? new());

        // 2 - Сохранение новых транзакций в БД
        if ((ItemsToSaveInDB?.Count ?? 0) > 0)
        {
            await SaveItemsChanges();
            SuccessItems?.AddRange(NewItemsToAddInDB?.Select(dbItem => mapper.Map<FuelTransactionShortResponse>(dbItem)));
        }
        else
        {
            NotifyMessage += $"Произошла ошибка. Отсутствуют транзакции, подлежащие сохранению в БД! ";
            Log.Error(nameof(SaveFilledTransactions), GetType().FullName, NotifyMessage);
        }
    }

    protected async Task<List<FuelTransaction>> MapTransaction(List<NotParsedTransactionFilled> transactions)
    {
        List<FuelTransaction> result = new();

        Trucks = truckService?.GetTrucks()?.GetAwaiter().GetResult() ?? new();
        ProvidersCards = await fuelRepositories?.Cards?.FindRangeAsync(card => ProvidersId.Any(providerId => providerId == card.ProviderId)) ?? new();

        foreach (var transaction in transactions ?? new())
        {
            var entry = await MapTransaction(transaction, Trucks);

            if (entry != null)
            {
                result.Add(entry);
            }
        }

        return result;
    }

    private async Task<FuelTransaction> MapTransaction(NotParsedTransactionFilled transaction, List<TruckResponse> cars)
    {
        FuelTransaction result = null;

        var cardsId = transaction?.FuelCardIdSelected ?? 0;
        var carsId = transaction?.CarIdSelected ?? 0;

        if (cardsId == 0 && carsId == 0) // Если ни карта ни авто не выбраны
        {
            NotifyMessage += "Ошибка - не был выбран ни номер топливной карты ни номер авто. ";
            NotifyMessage.LogError(GetType().Name, nameof(MapTransaction));
            return result;
        }
        else if (cardsId == 0 && carsId != 0) // если пользователем выбран авто
        {
            var car = cars?.FirstOrDefault(car => car.Id == carsId);

            // Проверить, внесена ли топливная карта с номером, идентичным номеру тягача, в БД
            cardsId = await SearchCard(car?.LicensePlate) ?? 0;

            if (cardsId == 0) // если пользователем выбран номер авто и карта с таким номером не внесена в БД
            {
                var addedCard = await AddFuelCard(truck: car, providerId: ProviderId, transaction.OperationDate);

                if ((addedCard?.Id ?? 0) == 0)
                {
                    NotifyMessage += $"Ошибка при попытке сохранить новую топливную карту ";
                    NotifyMessage.LogError(GetType().Name, nameof(MapTransaction));
                    return result;
                }
                else
                {
                    cardsId = addedCard.Id;
                }
            }
            else // если пользователем выбран номер авто и карта с таким номером есть в БД, но не определилась ранее при ее поиске из-за существенных расхождений в написании номера в отчете и в БД
            {
                var addedAlterNumber = AddCardsAlternativeNumber(transaction?.CarNumber ?? string.Empty, cardsId);

                if ((addedAlterNumber?.Id ?? 0) == 0)
                {
                    NotifyMessage += $"Ошибка при попытке сохранить новый альтернативный номер топливной карты  " +
                               $"«{transaction?.CardNumber ?? string.Empty}» " +
                               $"с идентификатором «{cardsId.ToString() ?? string.Empty}» ";
                    NotifyMessage.LogError(GetType().Name, nameof(MapTransaction));
                    return result;
                }
            }
        }
        else // если пользователем выбрана топливная карта
        {
            var addedAlterNumber = AddCardsAlternativeNumber(transaction?.CardNumber ?? string.Empty, cardsId);

            if ((addedAlterNumber?.Id ?? 0) == 0)
            {
                NotifyMessage += $"Ошибка при попытке сохранить новый альтернативный номер топливной карты  " +
                           $"«{transaction?.CardNumber ?? string.Empty}» " +
                           $"с идентификатором «{cardsId.ToString() ?? string.Empty}» ";
                NotifyMessage.LogError(GetType().Name, nameof(MapTransaction));
                return result;
            }
        }

        result = new FuelTransaction
        {
            ProviderId = transaction.ProviderId,
            OperationDate = transaction.OperationDate,
            FuelTypeId = transaction.FuelTypeId,
            Quantity = transaction.Quantity,
            Cost = transaction.Cost,
            CurrencyId = transaction.CurrencyId,
            TotalCost = transaction.TotalCost,
            CardId = cardsId,
            IsCheck = transaction.IsCheck,
            CountryId = transaction.CountryId,
            DriverReportId = transaction.DriverReportId,
            TransactionID = transaction.TransactionID,
            IsDaily = IsMonthly.HasValue ? !IsMonthly.Value : true,
        };

        return result;
    }

    public FuelCardsAlternativeNumber AddCardsAlternativeNumber(string number, int cardsId)
    {
        FuelCardsAlternativeNumber result;

        try
        {
            result = fuelRepositories?.CardsAlternativeNumbers?.Add(
                new FuelCardsAlternativeNumber
                {
                    Number = number,
                    CardId = cardsId,
                    ModifiedOn = DateTime.Now,
                    ModifiedBy = UserName ?? string.Empty,
                });
        }
        catch (Exception exc)
        {
            NotifyMessage += fuelRepositories?.CardsAlternativeNumbers?.Message ?? string.Empty;
            exc.LogError(GetType().FullName, nameof(AddCardsAlternativeNumber), NotifyMessage);
            throw;
        }

        NotifyMessage += fuelRepositories?.CardsAlternativeNumbers?.Message ?? string.Empty;

        return result;
    }

    /// <summary>
    /// Метод для формирования и сохранения коллекции не обнаруженных в БД номеров заправочных карт.
    /// </summary>
    protected override async Task SaveNotFoundItemsInDB()
    {
        List<NotFoundFuelCard> notFoundCards = new();

        var notFoundCardsNumbers = NotSuccessItems?.Where(notsuccess => !string.IsNullOrEmpty(notsuccess.NotSuccessItem.CardNumber))?
            .Select(notsuccess => notsuccess.NotSuccessItem.CardNumber)?.ToList() ?? new();

        // Сформировать коллекцию карт
        if (notFoundCardsNumbers?.Count > 0)
        {
            foreach (var num in notFoundCardsNumbers)
            {
                notFoundCards.Add(new NotFoundFuelCard
                {
                    Number = num ?? string.Empty,
                    UserName = UserName ?? string.Empty,
                    ImportDate = DateTime.Now,
                    FuelProviderId = ProviderId,
                });
            }
        }

        // Сохранить коллекцию в БД
        if (notFoundCards?.Count > 0)
            await SaveNotExistCardsNumbers(notFoundCards);
    }

    private async Task SaveNotExistCardsNumbers(List<NotFoundFuelCard> notFoundCards)
    {
        List<NotFoundFuelCard> notFoundCardsToSave = new();

        // Проверить, есть ли в БД подобные номера карт и если нету - сохранить
        if ((notFoundCards?.Count ?? 0) > 0)
        {
            foreach (var notFoundCard in notFoundCards)
            {
                var isExist = await IsExistNotFoundCardsNumber(notFoundCard.Number);

                if (!isExist)
                    notFoundCardsToSave.Add(notFoundCard);
            }
        }

        try
        {
            if ((notFoundCardsToSave?.Count ?? 0) > 0)
            {
                fuelRepositories?.NotFoundCards?.Add(notFoundCardsToSave);
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(SaveNotExistCardsNumbers), GetType().FullName, NotifyMessage);
            throw;
        }
    }

    private async Task<bool> IsExistNotFoundCardsNumber(string number)
    {
        if (string.IsNullOrEmpty(number) || string.IsNullOrWhiteSpace(number))
        {
            return false;
        }

        return await fuelRepositories?.NotFoundCards?.AnyAsync(notFoundNumber => notFoundNumber.Number.ToLower().Equals(number.ToLower()));
    }
    #endregion
}
