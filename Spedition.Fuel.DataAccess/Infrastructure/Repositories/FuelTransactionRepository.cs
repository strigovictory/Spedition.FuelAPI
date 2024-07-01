using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Spedition.Fuel.DataAccess.Infrastructure.Repositories.Interfaces;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Finance;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Geo;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Office;
using Spedition.Fuel.Shared.Helpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Spedition.Fuel.DataAccess.Infrastructure.Repositories;

public class FuelTransactionRepository : FuelContextAccessorBase
{
    public FuelTransactionRepository(SpeditionContext context)
        : base(context)
    {
    }

    public long GetCount(CancellationToken token = default)
    {
        return context?.Transactions?.AsQueryable()?.LongCount() ?? 0;
    }

    public async Task<FuelTransaction> GetTransaction(int id, CancellationToken token = default)
    {
        return context?.Transactions?.FindAsync(id, token).GetAwaiter().GetResult();
    }

    public async Task<List<FuelTransactionFullResponse>> GetTransactions(
        List<DivisionResponse> divisions,
        List<CountryResponse> countries,
        List<CurrencyResponse> currencies,
        CancellationToken token = default,
        int? toTake = null,
        int? toSkip = null)
    {
        var take = toTake.HasValue ? toTake.Value : 1; // Microsoft.Data.SqlClient.SqlException : The number of rows provided for a FETCH clause must be greater then zero.
        var skip = toSkip.HasValue ? toSkip.Value : 0;
        var resultFuelJoin = await (from transaction in context.Transactions
                         join card in context.Cards on transaction.CardId equals card.Id
                         join provider in context.Providers on card.ProviderId equals provider.Id
                         join fuelType in context.FuelTypes on transaction.FuelTypeId equals fuelType.Id
                         select new FuelTransactionFullResponse()
                         {
                             Id = transaction.Id,
                             TransactionID = transaction.TransactionID,
                             OperationDate = transaction.OperationDate,
                             Quantity = transaction.Quantity,
                             Cost = transaction.Cost,
                             TotalCost = transaction.TotalCost,
                             IsCheck = transaction.IsCheck,
                             ProviderId = transaction.ProviderId,
                             ProviderName = provider.Name,
                             FuelTypeId = transaction.FuelTypeId,
                             FuelTypeName = fuelType.Name,
                             CurrencyId = transaction.CurrencyId,
                             Card = card,
                             CardId = transaction.CardId,
                             CardName = card.Number,
                             CountryId = transaction.CountryId,
                             DriverReportId = transaction.DriverReportId,
                             IsDaily = transaction.IsDaily,
                             IsMonthly = transaction.IsMonthly,
                         })?.AsQueryable()?.Skip(skip)?.Take(take)?.ToListAsync(token) ?? new();

        var result = (from transaction in resultFuelJoin
                      join division in divisions.DefaultIfEmpty() on transaction.Card.DivisionID equals division?.Id ?? 0 into transactionsDivisions
                      from transactionDivision in transactionsDivisions.DefaultIfEmpty()
                      join country in countries.DefaultIfEmpty() on transaction.CountryId ?? 0 equals country?.Id ?? 0 into transactionsCountries
                      from transactionCountry in transactionsCountries.DefaultIfEmpty()
                      join currency in currencies.DefaultIfEmpty() on transaction.CurrencyId ?? 0 equals currency?.Id ?? 0 into transactionsCurrencies
                      from transactionCurrency in transactionsCurrencies.DefaultIfEmpty()
                      select new FuelTransactionFullResponse()
                      {
                          Id = transaction.Id,
                          TransactionID = transaction.TransactionID,
                          OperationDate = transaction.OperationDate,
                          Quantity = transaction.Quantity,
                          Cost = transaction.Cost,
                          TotalCost = transaction.TotalCost,
                          IsCheck = transaction.IsCheck,
                          DivisionName = transactionDivision?.Name ?? string.Empty,
                          ProviderId = transaction.ProviderId,
                          ProviderName = transaction.ProviderName,
                          FuelTypeId = transaction.FuelTypeId,
                          FuelTypeName = transaction.FuelTypeName,
                          CurrencyId = transaction.CurrencyId,
                          CurrencyName = transactionCurrency?.Name ?? string.Empty,
                          CardId = transaction.CardId,
                          CardName = transaction.CardName,
                          CountryId = transaction.CountryId,
                          CountryName = transactionCountry?.Name ?? string.Empty,
                          DriverReportId = transaction.DriverReportId,
                          IsDaily = transaction.IsDaily,
                          IsMonthly = transaction.IsMonthly,
                      })?.ToList() ?? new();

        return result;
    }

    public async Task<List<FuelTransaction>> FindRangeAsync(Expression<Func<FuelTransaction, bool>> predicate, CancellationToken token = default)
    {
        return await context?.Transactions?.AsQueryable()?.Where(predicate)?.ToListAsync(token) ?? new();
    }

    public FuelTransaction PutTransaction(FuelTransaction transaction)
    {
        notifyMessage = string.Empty;
        FuelTransaction result;
        try
        {
            result = context.Transactions?.Update(transaction)?.Entity;
            context.SaveChanges();
            notifyMessage = $"Операция успешно завершена ! Данные были обновлены в системе !";
        }
        catch (Exception exc)
        {
            exc.LogError(GetType().FullName, nameof(PutTransaction));
            notifyMessage = $"Запись не была обновлена в системе, произошла ошибка на уровне базы данных ! " +
                      $"{exc.GetExeceptionMessages()} ! ";
            throw;
        }

        return result;
    }

    public FuelTransaction CreateTransactions(FuelTransaction transaction)
    {
        FuelTransaction result;
        notifyMessage = string.Empty;
        try
        {
            result = context.Transactions?.Add(transaction)?.Entity;
            context.SaveChanges();
            notifyMessage = $"Операция успешно завершена ! Данные добавлены в систему !";
        }
        catch (Exception exc)
        {
            exc.LogError(GetType().FullName, nameof(CreateTransactions));
            notifyMessage = $"Запись не была добавлена в систему, произошла ошибка на уровне базы данных ! " +
                      $"{exc.GetExeceptionMessages()} ! ";
            throw;
        }

        return result;
    }

    public bool DeleteTransactions(List<int> transactionsIds)
    {
        var result = false;
        List<FuelTransaction> entities = new();

        transactionsIds?.ForEach(id => entities.Add(context.Transactions?.Find(id) ?? null));
        entities.RemoveAll(entity => entity == null);

        try
        {
            context.Transactions?.RemoveRange(entities);
            context.SaveChanges();
            result = true;
            notifyMessage = $"Операция успешно завершена ! Коллекция из {transactionsIds?.Count() ?? 0} элементов была удалена из системы !";
        }
        catch (Exception exc)
        {
            exc.LogError(GetType().FullName, nameof(DeleteTransactions));
            notifyMessage = $"Записи не подлежит удалению из системы, так как участвует в других записях ! " +
                      $"Можете обратиться в службу поддержки и оставить заявку на удаление ! " +
                      $"{exc.GetExeceptionMessages()} ! ";
            throw;
        }

        return result;
    }
}
