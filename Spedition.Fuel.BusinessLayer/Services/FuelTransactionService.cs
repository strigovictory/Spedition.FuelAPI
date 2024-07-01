using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using NPOI.OpenXmlFormats.Dml.Chart;
using NPOI.SS.Formula.Functions;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using Spedition.Fuel.BFF.Constants;
using Spedition.Fuel.BusinessLayer.Helpers;
using Spedition.Fuel.BusinessLayer.Services.BaseServices;
using Spedition.Fuel.DataAccess.Infrastructure.Repositories;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.Fuel.BusinessLayer.Services;

public class FuelTransactionService : GroupActionBase<FuelTransactionShortResponse, FuelTransactionShortResponse, FuelTransaction>, IFuelTransactionService
{
    private readonly ICountryService countryService;
    private readonly IDivisionService divisionService;
    private readonly ICurrencyService currencyService;
    private readonly FuelTransactionRepository fuelRepository;

    public FuelTransactionService(
        FuelTransactionRepository fuelRepository,
        IWebHostEnvironment env,
        IConfiguration configuration,
        IMapper mapper)
        : base(env, configuration, mapper)
    {
        this.fuelRepository = fuelRepository;
    }

    public FuelTransactionService(
        FuelTransactionRepository fuelRepository,
        IWebHostEnvironment env,
        IConfiguration configuration,
        IMapper mapper,
        ICountryService countryService,
        IDivisionService divisionService,
        ICurrencyService currencyService)
        : base(env, configuration, mapper)
    {
        this.countryService = countryService;
        this.divisionService = divisionService;
        this.currencyService = currencyService;
        this.fuelRepository = fuelRepository;
    }

    public override async Task<bool> CheckIsInstanceExist(FuelTransaction checkedTransaction)
    {
        bool result = default;

        try
        {
            var commonConditions = async () => (await fuelRepository.FindRangeAsync(
                transaction => transaction.Id != checkedTransaction.Id
                && transaction.CardId == checkedTransaction.CardId
                && transaction.Quantity == (checkedTransaction.Quantity ?? 0)
                && transaction.TotalCost == (checkedTransaction.TotalCost ?? 0)));

            var dateCondition = (FuelTransaction transactionItem) => transactionItem.OperationDate.IsDatesEquel(checkedTransaction.OperationDate);

            if (!string.IsNullOrEmpty(checkedTransaction.TransactionID)
             && !string.IsNullOrWhiteSpace(checkedTransaction.TransactionID))
            {
                result = (await commonConditions()).Any(
                    transaction => dateCondition(transaction)
                    && (!string.IsNullOrEmpty(transaction.TransactionID)
                    && (transaction.TransactionID == checkedTransaction.TransactionID
                    || transaction.TransactionID.Contains(checkedTransaction.TransactionID)
                    || checkedTransaction.TransactionID.Contains(transaction.TransactionID))));
            }
            else
            {
                result = (await commonConditions()).Any(transaction => dateCondition(transaction));
            }
        }
        catch (Exception exc)
        {
            NotifyMessage += $"Error transaction: {checkedTransaction?.ToString() ?? string.Empty}";
            exc.LogError(nameof(CheckIsInstanceExist), GetType().FullName, NotifyMessage);
            throw;
        }

        if (result)
        {
            ExistedInstances.Add(checkedTransaction);
        }

        return result;
    }

    #region Get
    public long GetCount(CancellationToken token = default)
    {
        return fuelRepository.GetCount(token);
    }

    public async Task<FuelTransactionResponse> GetTransaction(int id, CancellationToken token = default)
    {
        var dbTransaction = await fuelRepository.GetTransaction(id, token);
        return mapper.Map<FuelTransactionResponse>(dbTransaction ?? new());
    }

    public async Task<List<FuelTransactionFullResponse>> GetTransactions(CancellationToken token = default, int? toTake = null, int? toSkip = null)
    {
        var currencies = (currencyService?.Get())?.GetAwaiter().GetResult() ?? new();
        var countries = (countryService?.Get())?.GetAwaiter().GetResult() ?? new();
        var divisions = (divisionService?.GetDivisions())?.GetAwaiter().GetResult() ?? new();

        return await fuelRepository.GetTransactions(divisions, countries, currencies, token, toTake, toSkip);
    }
    #endregion

    #region Put
    public async Task<FuelTransactionShortResponse> UpdateTransaction(FuelTransactionRequest transaction)
    {
        NotifyMessage = string.Empty;
        FuelTransactionShortResponse result = new();

        var transactionToSave = mapper?.Map<FuelTransaction>(transaction);

        // Перед сохранением изменений в БД, проверить уникальность обновленной транзакции
        if (!(await CheckIsInstanceExist(transactionToSave)))
        {
            var resUpdate = fuelRepository.PutTransaction(transactionToSave);
            NotifyMessage += fuelRepository.NotifyMessage ?? string.Empty;
            result = mapper?.Map<FuelTransactionShortResponse>(resUpdate);
        }

        return result;
    }
    #endregion

    #region Post
    public async Task CreateTransactions(List<FuelTransactionRequest> transactions)
    {
        NotifyMessage = string.Empty;
        SuccessItems = new();
        ErrorItems = new();
        ExistedInstances = new();

        if ((transactions?.Count ?? 0) == 0)
        {
            NotifyMessage += "Пустая коллекция транзакций не подлежит сохранению в БД !";
            NotifyMessage.LogError(GetType().Name, nameof(CreateTransactions));
            return;
        }

        // 1 - Предварительно исключить из коллекции те тр-ции, которые уже существуют в БД
        var transactionToSave = await TrimExistedTransactions(transactions);

        // 2 - Сохранить новые топливные карты в БД
        foreach (var transaction in transactionToSave)
        {
            var createdTransaction = fuelRepository.CreateTransactions(transaction);
            NotifyMessage += fuelRepository.NotifyMessage ?? "Ошибка на уровне БД !";

            if ((createdTransaction?.Id ?? 0) > 0)
            {
                SuccessItems?.Add(mapper.Map<FuelTransactionShortResponse>(createdTransaction));
                NotifyMessage += $"{createdTransaction?.ToString() ?? string.Empty} была сохранена !";
            }
            else
            {
                ErrorItems?.Add(
                    new NotSuccessResponseItemDetailed<FuelTransactionShortResponse>
                    {
                        NotSuccessItem = mapper?.Map<FuelTransactionShortResponse>(transaction),
                        Reason = NotifyMessage,
                    });
                NotifyMessage += $"{transaction?.ToString() ?? string.Empty} не была сохранена !";
                continue;
            }
        }
    }
    #endregion

    #region Delete

    /// <summary>
    /// Метод для удаления из БД группы транзакции.
    /// </summary>
    /// <param name="transactionsIds">Коллекция транзакций, подлежащая удалению из БД.</param>
    /// <returns>Коллекция транзакций после удаления из БД.</returns>
    public bool DeleteTransactions(List<int> transactionsIds)
    {
        var result = fuelRepository.DeleteTransactions(transactionsIds);
        NotifyMessage += fuelRepository.NotifyMessage ?? "Ошибка на уровне БД !";
        return result;
    }

    public async Task<(int success, int notSuccess)> DeleteTransactionsDuplicates(
        int? fuelProvId, int? month = null, int? year = null, CancellationToken token = default)
    {
        NotifyMessage = string.Empty;
        var success = 0;
        var notSuccess = 0;
        DateTime start = default;
        DateTime finish = default;
        List<FuelTransaction> transactionsFiltered = new();
        Predicate<FuelTransaction> conditionByProviderAndFuelType;

        if (fuelProvId.HasValue) // только топливо и adblue
        {
            conditionByProviderAndFuelType = (FuelTransaction transaction) =>
            (transaction.FuelTypeId == 2 || transaction.FuelTypeId == 10)
            && transaction.ProviderId == fuelProvId.Value;
        }
        else
        {
            conditionByProviderAndFuelType = (FuelTransaction transaction) =>
            (transaction.FuelTypeId == 2 || transaction.FuelTypeId == 10);
        }

        var transactionsPerProviderAndFuelType = await fuelRepository.FindRangeAsync(transaction => conditionByProviderAndFuelType(transaction), token) ?? new();

        var conditionByPeriod = (FuelTransaction transaction, DateTime start, DateTime finish) =>
            transaction.OperationDate.Date >= start && transaction.OperationDate.Date <= finish;

        if (month.HasValue)
        {
            start = new DateTime(day: 1, month: month.Value, year: year.Value);
            finish = new DateTime(day: DateTime.DaysInMonth(year.Value, month.Value), month: month.Value, year: year.Value);
            transactionsFiltered = transactionsPerProviderAndFuelType?.Where(transaction => conditionByPeriod(transaction, start, finish))?.ToList() ?? new();
        }
        else if (!month.HasValue && year.HasValue)
        {
            start = new DateTime(day: 1, month: 1, year: year.Value);
            finish = new DateTime(day: 31, month: 12, year: year.Value);
            transactionsFiltered = transactionsPerProviderAndFuelType?.Where(transaction => conditionByPeriod(transaction, start, finish))?.ToList() ?? new();
        }
        else transactionsFiltered = new(transactionsPerProviderAndFuelType);

        var duplicatedTransactions = transactionsFiltered?.GroupBy(transaction => new
        {
            transaction.ProviderId,
            transaction.CardId,
            transaction.OperationDate,
            transaction.Quantity,
            transaction.TransactionID,
            transaction.Cost,
        })?
        .Select(transaction => new
        {
            records = transaction.ToList(),
        })?
        .Where(group => group.records.Count() > 1)?.ToList() ?? new();

        foreach (var duplicate in duplicatedTransactions)
        {
            if (DeleteDuplicatedTransactions(duplicate.records))
                success++;
            else
                notSuccess++;
        }

        NotifyMessage = $"В системе было обнаружено {duplicatedTransactions?.Count ?? 0} групп дубликатов топливных транзакций, из них {success} было удалено! ";
        return (success: success, notSuccess: notSuccess);
    }

    private bool DeleteDuplicatedTransactions(List<FuelTransaction> transactions)
    {
        var result = false;

        var transactionsCount = transactions?.Count ?? 0;

        if (transactionsCount > 1)
        {
            // удалению подлежат только те тр-и у которых флаг «Проверено» выключен и те,  у к-х ссылка на отчет водтеля не заполнена
            var transactionsFiltered = transactions?.Where(transaction => !transaction.IsCheck && !transaction.DriverReportId.HasValue)?.ToList() ?? new();

            if (transactionsFiltered.Count > 0)
            {
                if (transactionsCount != transactionsFiltered.Count())
                {
                    result = fuelRepository.DeleteTransactions(transactionsFiltered?.Select(transaction => transaction.Id)?.ToList() ?? new());
                    NotifyMessage += fuelRepository.NotifyMessage ?? string.Empty;
                }
                else
                {
                    // удалить все отфильтрованные тр-ции, за исключением одной
                    var transactionsToDel = transactionsFiltered?.OrderBy(transaction => transaction.Id)?.TakeLast(transactionsCount - 1)?.ToList() ?? new();
                    result = fuelRepository.DeleteTransactions(transactionsToDel?.Select(transaction => transaction.Id)?.ToList() ?? new());
                    NotifyMessage += fuelRepository.NotifyMessage ?? string.Empty;
                }
            }
        }

        return result;
    }
    #endregion

    #region Additional
    private async Task<List<FuelTransaction>> TrimExistedTransactions(List<FuelTransactionRequest> transactions)
    {
        List<FuelTransaction> result = new();

        foreach (var transaction in transactions?.Select(transaction => mapper?.Map<FuelTransaction>(transaction))?.ToList() ?? new())
        {
            if (!(await CheckIsInstanceExist(transaction)))
            {
                result.Add(transaction);
            }
        }

        return result;
    }
    #endregion
}
