using System;
using Spedition.Fuel.DataAccess.Infrastructure.Repositories.Interfaces;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Office;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Trips;
using Spedition.Fuel.Shared.Helpers;

namespace Spedition.Fuel.DataAccess.Infrastructure.Repositories;

public class FuelCardRepository : FuelContextAccessorBase
{
    public FuelCardRepository(SpeditionContext context)
        : base(context)
    {
    }

    #region Get
    public async Task<List<KitEventType>> GetKitEventTypes(CancellationToken token = default)
    {
        return await context.KitEventTypes?.ToListAsync(token);
    }

    public async Task<bool> AnyCard(Expression<Func<FuelCard, bool>> predicate, CancellationToken token = default)
    {
        return await context.Cards?.AsQueryable()?.AnyAsync(predicate, token);
    }

    public async Task<bool> AnyAlternativeNumber(
        Expression<Func<FuelCardsAlternativeNumber, bool>> predicate, CancellationToken token = default)
    {
        return await context.CardsAlternativeNumbers?.AsQueryable()?.AnyAsync(predicate, token);
    }

    public virtual async Task<FuelCard> GetCard(int id, CancellationToken token = default)
    {
        return context.Cards?.FindAsync(new object[] { id }, token).GetAwaiter().GetResult();
    }

    public async Task<List<FuelCardFullResponse>> GetCards(
        List<DivisionResponse> divisions,
        List<TruckResponse> trucks,
        List<EmployeeResponse> employees,
        CancellationToken token = default)
    {
        var resultFuelJoin = await (from card in context.Cards
                                    join provider in context?.Providers on card.ProviderId equals provider.Id
                                    select new FuelCardFullResponse
                                    {
                                        Id = card.Id,
                                        Number = card.Number,
                                        ExpirationDate = card.ExpirationDate,
                                        ReceiveDate = card.ReceiveDate,
                                        IssueDate = card.IssueDate,
                                        IsReserved = card.IsReserved,
                                        Note = card.Note,
                                        IsArchived = card.IsArchived,
                                        ProviderName = provider.Name,
                                        ProviderId = provider.Id,
                                        CarId = card.CarId,
                                        DivisionID = card.DivisionID,
                                        EmployeeId = card.EmployeeId,
                                    })?.ToListAsync(token) ?? new();

        var result = (from card in resultFuelJoin
                      join division in divisions.DefaultIfEmpty() on card.DivisionID equals division?.Id ?? 0 into divisionsCards
                      from divisionCard in divisionsCards.DefaultIfEmpty()
                      join truck in trucks.DefaultIfEmpty() on card.CarId ?? 0 equals truck?.Id ?? 0 into trucksCards
                      from truckCard in trucksCards.DefaultIfEmpty()
                      join employee in employees.DefaultIfEmpty() on card.EmployeeId ?? 0 equals employee?.Id ?? 0 into employeesCards
                      from employeeCard in employeesCards.DefaultIfEmpty()
                      select new FuelCardFullResponse
                      {
                          Id = card.Id,
                          Number = card.Number,
                          ExpirationDate = card.ExpirationDate,
                          ReceiveDate = card.ReceiveDate,
                          IssueDate = card.IssueDate,
                          IsReserved = card.IsReserved,
                          Note = card.Note,
                          IsArchived = card.IsArchived,
                          ProviderId = card.ProviderId,
                          ProviderName = card.ProviderName,
                          CarId = card.CarId,
                          CarName = truckCard?.LicensePlate ?? string.Empty,
                          DivisionID = card.DivisionID,
                          DivisionName = divisionCard?.Name ?? string.Empty,
                          EmployeeId = card.EmployeeId,
                          EmployeeName = $"{employeeCard?.LastName ?? string.Empty} {employeeCard?.FirstName ?? string.Empty} {employeeCard?.MiddleName ?? string.Empty}",
                      })?.ToList() ?? new();

        return result;
    }

    public async Task<List<FuelCardNotFoundResponse>> GetNotFoundCards(CancellationToken token = default)
    {
       return await (from nfcard in context.NotFoundCard
                     join provider in context.Providers on nfcard.FuelProviderId equals provider.Id
                     select new FuelCardNotFoundResponse
                     {
                        Id = nfcard.Id,
                        Number = nfcard.Number,
                        UserName = nfcard.UserName,
                        ImportDate = nfcard.ImportDate,
                        FuelProviderId = nfcard.FuelProviderId,
                        FuelProviderName = provider.Name,
                     })?.ToListAsync(token) ?? new();
    }

    public async Task<List<FuelCardsAlternativeNumber>> FindCardsAlternativeNumbers(
        Expression<Func<FuelCardsAlternativeNumber, bool>> predicate, CancellationToken token = default)
    {
        return await context.CardsAlternativeNumbers?.AsQueryable()?.Where(predicate)?.ToListAsync(token) ?? new();
    }

    public async Task<FuelCardsEvent> GetCardsEvent(int eventId, CancellationToken token = default)
    {
        return context.CardsEvents?.FindAsync(new object[] { eventId }, token).GetAwaiter().GetResult();
    }

    public async Task<List<FuelCardsEvent>> FindCardsEvents(Expression<Func<FuelCardsEvent, bool>> predicate, CancellationToken token = default)
    {
        return await context.CardsEvents?.AsQueryable()?.Where(predicate)?.ToListAsync(token) ?? new();
    }

    public async Task<List<FuelTransaction>> FindRangeTransactions(Expression<Func<FuelTransaction, bool>> predicate, CancellationToken token = default)
    {
        return await context?.Transactions?.AsQueryable()?.Where(predicate)?.ToListAsync(token) ?? new();
    }

    public async Task<List<FuelCard>> FindCards(Expression<Func<FuelCard, bool>> predicate, CancellationToken token = default)
    {
        return await context.Cards?.AsQueryable()?.Where(predicate)?.ToListAsync(token) ?? new();
    }

    #endregion

    #region Update
    public FuelCard UpdateCard(FuelCard card)
    {
        FuelCard result;
        notifyMessage = string.Empty;
        try
        {
            result = context.Cards?.Update(card)?.Entity;
            context?.SaveChanges();
            notifyMessage += $"Операция успешно завершена ! Данные были обновлены в системе !";
        }
        catch (Exception exc)
        {
            exc.LogError(GetType().FullName, nameof(UpdateCard));
            notifyMessage += $"Запись не была обновлена в системе, произошла ошибка на уровне базы данных ! " +
                      $"{exc.GetExeceptionMessages()} ! ";
            throw;
        }

        return result;
    }

    public FuelCardsAlternativeNumber UpdateCardsAlternativeNumber(FuelCardsAlternativeNumber alternativeNumber)
    {
        FuelCardsAlternativeNumber result;
        notifyMessage = string.Empty;
        try
        {
            result = context.CardsAlternativeNumbers?.Update(alternativeNumber)?.Entity;
            context?.SaveChanges();
            notifyMessage = $"Операция успешно завершена ! Данные были обновлены в системе !";
        }
        catch (Exception exc)
        {
            exc.LogError(GetType().FullName, nameof(UpdateCardsAlternativeNumber));
            notifyMessage = $"Запись не была обновлена в системе, произошла ошибка на уровне базы данных ! " +
                      $"{exc.GetExeceptionMessages()} ! ";
            throw;
        }

        return result;
    }

    public FuelCardsEvent UpdateCardsEvent(FuelCardsEvent cardsEvent)
    {
        FuelCardsEvent result;
        notifyMessage = string.Empty;
        try
        {
            result = context.CardsEvents?.Update(cardsEvent)?.Entity;
            context?.SaveChanges();
            notifyMessage = $"Операция успешно завершена ! Данные были обновлены в системе !";
        }
        catch (Exception exc)
        {
            exc.LogError(GetType().FullName, nameof(UpdateCardsEvent));
            notifyMessage = $"Запись не была обновлена в системе, произошла ошибка на уровне базы данных ! " +
                      $"{exc.GetExeceptionMessages()} ! ";
            throw;
        }

        return result;
    }
    #endregion

    #region Create
    public FuelCard CreateCard(FuelCard card)
    {
        FuelCard result;
        notifyMessage = string.Empty;
        try
        {
            result = context.Cards?.Add(card).Entity;
            context.SaveChanges();
            notifyMessage = $"Операция успешно завершена ! Данные добавлены в систему !";
        }
        catch (Exception exc)
        {
            exc.LogError(GetType().FullName, nameof(CreateCard));
            notifyMessage = $"Запись не была добавлена в систему, произошла ошибка на уровне базы данных ! " +
                      $"{exc.GetExeceptionMessages()} ! ";
            throw;
        }

        return result;
    }

    public FuelCardsAlternativeNumber CreateCardsAlternativeNumber(FuelCardsAlternativeNumber alterntiveNumber)
    {
        FuelCardsAlternativeNumber result;
        notifyMessage = string.Empty;
        try
        {
            result = context.CardsAlternativeNumbers?.Add(alterntiveNumber).Entity;
            context.SaveChanges();
            notifyMessage = $"Операция успешно завершена ! Данные добавлены в систему !";
        }
        catch (Exception exc)
        {
            exc.LogError(GetType().FullName, nameof(CreateCardsAlternativeNumber));
            notifyMessage = $"Запись не была добавлена в систему, произошла ошибка на уровне базы данных ! " +
                      $"{exc.GetExeceptionMessages()} ! ";
            throw;
        }

        return result;
    }

    public FuelCardsEvent CreateCardsEvent(FuelCardsEvent cardsEvent)
    {
        FuelCardsEvent result;
        notifyMessage = string.Empty;
        try
        {
            result = context.CardsEvents?.Add(cardsEvent).Entity;
            context.SaveChanges();
            notifyMessage = $"Операция успешно завершена ! Данные добавлены в систему !";
        }
        catch (Exception exc)
        {
            exc.LogError(GetType().FullName, nameof(CreateCardsAlternativeNumber));
            notifyMessage = $"Запись не была добавлена в систему, произошла ошибка на уровне базы данных ! " +
                      $"{exc.GetExeceptionMessages()} ! ";
            throw;
        }

        return result;
    }
    #endregion

    #region Delete
    public bool DeleteCards(List<int> cardsIds)
    {
        var result = false;
        notifyMessage = string.Empty;
        List<FuelCard> entities = new();

        cardsIds?.ForEach(id => entities.Add(context.Cards?.Find(id) ?? null));
        entities.RemoveAll(entity => entity == null);

        try
        {
            context.Cards?.RemoveRange(entities);
            context.SaveChanges();
            result = true;
            notifyMessage += $"Операция успешно завершена ! Коллекция из {cardsIds?.Count() ?? 0} элементов была удалена из системы !";
        }
        catch (Exception exc)
        {
            exc.LogError(GetType().FullName, nameof(DeleteCards));
            notifyMessage += $"Записи не подлежит удалению из системы, так как участвует в других записях ! " +
                      $"Можете обратиться в службу поддержки и оставить заявку на удаление ! " +
                      $"{exc.GetExeceptionMessages()} ! ";
            throw;
        }

        return result;
    }

    public bool DeleteCardsAlternativeNumbers(List<int> alternativeNumbersIds)
    {
        var result = false;
        notifyMessage = string.Empty;
        List<FuelCardsAlternativeNumber> entities = new();

        alternativeNumbersIds?.ForEach(id => entities.Add(context.CardsAlternativeNumbers?.Find(id) ?? null));
        entities.RemoveAll(entity => entity == null);

        try
        {
            context.CardsAlternativeNumbers?.RemoveRange(entities);
            context.SaveChanges();
            result = true;
            notifyMessage = $"Операция успешно завершена ! Коллекция из {alternativeNumbersIds?.Count() ?? 0} элементов была удалена из системы !";
        }
        catch (Exception exc)
        {
            exc.LogError(GetType().FullName, nameof(DeleteCardsAlternativeNumbers));
            notifyMessage = $"Записи не подлежит удалению из системы, так как участвует в других записях ! " +
                      $"Можете обратиться в службу поддержки и оставить заявку на удаление ! " +
                      $"{exc.GetExeceptionMessages()} ! ";
            throw;
        }

        return result;
    }

    public bool DeleteNotFoundCards(List<int> notfoundNumbersIds)
    {
        var result = false;
        notifyMessage = string.Empty;
        List <NotFoundFuelCard> entities = new();

        notfoundNumbersIds?.ForEach(id => entities.Add(context.NotFoundCard?.Find(id) ?? null));
        entities.RemoveAll(entity => entity == null);

        try
        {
            context.NotFoundCard?.RemoveRange(entities);
            context.SaveChanges();
            result = true;
            notifyMessage += $"Операция успешно завершена ! Коллекция из {notfoundNumbersIds?.Count() ?? 0} элементов была удалена из системы !";
        }
        catch (Exception exc)
        {
            exc.LogError(GetType().FullName, nameof(DeleteNotFoundCards));
            notifyMessage += $"Записи не подлежит удалению из системы, так как участвует в других записях ! " +
                      $"Можете обратиться в службу поддержки и оставить заявку на удаление ! " +
                      $"{exc.GetExeceptionMessages()} ! ";
            throw;
        }

        return result;
    }

    public bool DeleteFuelCardEvent(List<int> cardEventsIds)
    {
        var result = false;
        notifyMessage = string.Empty;
        List<FuelCardsEvent> entities = new();

        cardEventsIds?.ForEach(id => entities.Add(context.CardsEvents?.Find(id) ?? null));
        entities.RemoveAll(entity => entity == null);

        try
        {
            context.CardsEvents?.RemoveRange(entities);
            context.SaveChanges();
            result = true;
            notifyMessage += $"Операция успешно завершена ! Коллекция из {cardEventsIds?.Count() ?? 0} элементов была удалена из системы !";
        }
        catch (Exception exc)
        {
            exc.LogError(GetType().FullName, nameof(DeleteFuelCardEvent));
            notifyMessage += $"Записи не подлежит удалению из системы, так как участвует в других записях ! " +
                      $"Можете обратиться в службу поддержки и оставить заявку на удаление ! " +
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
    #endregion

    public void CleanTEntriesChangeTracker<T>()
        where T : class
    {
        foreach (EntityEntry entityEntry in context?.ChangeTracker?.Entries<T>()?.ToArray() ?? new EntityEntry[0])
        {
            if (entityEntry?.Entity != null)
            {
                entityEntry.State = EntityState.Detached;
            }
        }
    }
}
