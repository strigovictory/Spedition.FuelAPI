using System.Transactions;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Spedition.Fuel.BusinessLayer.Services.BaseServices;

public abstract class SavingBase<TSuccess, TNotSuccess, TSearch> 
    : FuelRepositoriesBase<TSuccess, TNotSuccess, TSearch>
{
    protected SavingBase(IWebHostEnvironment env, FuelRepositories fuelRepositories, IConfiguration configuration, IMapper mapper) 
        : base(fuelRepositories, env, configuration, mapper)
    {
    }

    public bool? IsMonthly { get; set; }

    /// <summary>
    /// Коллекция распарсенных экземпляров, преобразованных в тип для хранения в БД.
    /// </summary>
    protected List<TSearch> ItemsToSaveInDB { get; set; } = new();

    /// <summary>
    /// Коллекция экземпляров, преобразованных в тип для хранения в БД, из которой исключены те экземпляры, которые уже есть в БД.
    /// </summary>
    protected List<TSearch> NewItemsToAddInDB { get; set; } = new();

    protected abstract Task<List<string>> ConstructInsertCommands();

    protected abstract Task<List<string>> ConstructUpdateCommands();

    protected async Task SaveItemsChanges()
    {
        try
        {
            await InitItemsToAddAndToUpdateInDB();

            var resultSavingToDB = await SaveItemsInDB();
            if (resultSavingToDB)
            {
                NotifyMessage += $"Операция успешно завершена. ";
                return;
            }
            else
            {
                NotifyMessage += "При сохранении данных в БД произошла ошибка! ";
                return;
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(SaveItemsChanges), GetType().FullName);
            throw;
        }
    }

    /// <summary>
    /// Метод, для формирования итоговых коллекций для добавления/обновления в БД.
    /// </summary>
    private async Task InitItemsToAddAndToUpdateInDB()
    {
        NewItemsToAddInDB = new();
        ExistedInstances = new();

        foreach (var item in ItemsToSaveInDB ?? new())
        {
            var isExist = await CheckIsInstanceExist(item);
            if (isExist)
            {
                continue;
            }
            else
            {
                NewItemsToAddInDB?.Add(item);
            }
        }
    }

    /// <summary>
    /// Метод для выполнения транзакции по сохранению данных в БД с возможностью rollback.
    /// </summary>
    /// <returns>Результат добавления/обновления в БД транзакций.</returns>
    private async Task<bool> SaveItemsInDB()
    {
        var insertResult = true;
        var updateResult = true;

        if ((NewItemsToAddInDB?.Count ?? 0) > 0)
            insertResult = await AddItemsInDB();

        if ((ExistedInstances?.Count ?? 0) > 0)
            updateResult = await UpdateItemsInDB();

        return insertResult && updateResult;
    }

    protected async Task<int> DoDbComand(List<string> commands)
    {
        // Если список инструкций не пустой - выполнить транзакцию
        if ((commands?.Count ?? 0) > 0)
        {
            var errorCommand = string.Empty;
            var counter = 0;
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                using SqlConnection connection = new(ConnectionString);

                connection.Open();

                foreach (var command in commands)
                {
                    errorCommand = command;
                    var rows = await connection.ExecuteAsync(command);
                    counter += rows > 0 ? 1 : 0; // т.к. при добавлении одной записи в таблицу, на которой висит триггер на Create, rows будет равно 2
                }

                transactionScope.Complete();
            }
            catch (Exception exc)
            {
                exc.LogError(nameof(AddItemsInDB), GetType().FullName, $"ErrorCommand: «{errorCommand}»");
                throw;
            }

            return counter;
        }
        else
        {
            NotifyMessage += $"Изменения не были зафиксированы, т.к. произошла ошибка на этапе формирования! " +
                $"набора инструкций для добавления новых экземпляров в базу данных";
            return 0;
        }
    }

    /// <summary>
    /// Метод для выполнения транзакции по добавлению данных в БД с возможностью rollback.
    /// </summary>
    /// <returns>Результат добавления.</returns>
    protected virtual async Task<bool> AddItemsInDB()
    {
        try
        {
            List<string> commands = await ConstructInsertCommands();

            // Если список инструкций не пустой - выполнить транзакцию
            if ((commands?.Count ?? 0) > 0)
            {
                var counter = await DoDbComand(commands);

                if (counter > 0 && NewItemsToAddInDB?.Count == counter)
                {
                    NotifyMessage += $"В базу данных были добавлены новые экземпляры в количестве {commands?.Count ?? 0} шт. !";
                    SuccessItems?.AddRange(NewItemsToAddInDB?.Select(dbItem => mapper.Map<TSuccess>(dbItem)));
                }
                else if (counter > 0 && NewItemsToAddInDB?.Count > counter)
                {
                    NotifyMessage += $"Из {NewItemsToAddInDB?.Count ?? 0} шт. экземпляров, " +
                               $"подлежащих добавлению в базу данных, было сохранено {counter} шт. !" +
                               $"Оставшиеся {(NewItemsToAddInDB?.Count ?? 0) - counter} шт. не были сохранены " +
                               $"по причине ошибки на уровне БД. ";
                    Log.Error(NotifyMessage);
                }

                return true;
            }
            else
            {
                NotifyMessage += $"Изменения не были зафиксированы, т.к. произошла ошибка на этапе формирования! " +
                    $"набора инструкций для добавления новых экземпляров в базу данных";
                return false;
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(SaveItemsInDB), GetType().FullName);
            throw;
        }
    }

    /// <summary>
    /// Метод для выполнения транзакции по обновлению данных в БД с возможностью rollback.
    /// </summary>
    /// <returns>Результат обновления.</returns>
    protected virtual async Task<bool> UpdateItemsInDB()
    {
        try
        {
            List<string> commands = await ConstructUpdateCommands();

            // Если список инструкций не пустой - выполнить транзакцию
            if ((commands?.Count ?? 0) > 0)
            {
                var counter = await DoDbComand(commands);

                if (counter > 0 && ExistedInstances?.Count == counter)
                {
                    NotifyMessage += $"В базе данных были обновлены экземпляры в количестве {commands?.Count ?? 0} шт. !";
                    SecondarySuccessItems?.AddRange(ExistedInstances?.Select(dbItem => mapper.Map<TSuccess>(dbItem)));
                }
                else if (counter > 0 && ExistedInstances?.Count > counter)
                {
                    NotifyMessage += $"Из {ExistedInstances?.Count ?? 0} шт. экземпляров, " +
                               $"подлежащих обновлению в базе данных, было сохранено {counter} шт. !" +
                               $"Оставшиеся {(ExistedInstances?.Count ?? 0) - counter} шт. не были сохранены " +
                               $"по причине ошибки на уровне БД. ";
                    Log.Error(NotifyMessage);
                }

                return true;
            }
            else
            {
                NotifyMessage += $"Изменения не были зафиксированы, т.к. произошла ошибка на этапе формирования! " +
                           $"набора инструкций для обновления экземпляров в базе данных";
                return false;
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(SaveItemsInDB), GetType().FullName);
            throw;
        }
    }
}
