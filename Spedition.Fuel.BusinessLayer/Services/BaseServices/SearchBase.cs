using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Spedition.Fuel.BusinessLayer.Helpers;

namespace Spedition.Fuel.BusinessLayer.Services.BaseServices;

public abstract class SearchBase<T> : FileBase, ISearch<T>
{
    private readonly string connectionString;

    public SearchBase(IConfiguration configuration, IWebHostEnvironment env) 
        : base(env)
    {
        connectionString = configuration?.GetValue<string>("ConnectionStrings:SpeditionDb") ?? string.Empty;
    }

    protected string ConnectionString => connectionString;

    /// <summary>
    /// Коллекция Т-экземпляров, которые являются дубликатами.
    /// </summary>
    public virtual List<T> ExistedInstances { get; protected set; } = new();

    /// <summary>
    /// Метод для проверки на наличие в БД идентичных экземпляров.
    /// </summary>
    /// <param name="item">Т-экземпляр, подлежащий проверке на наличие в БД.</param>
    /// <returns>Истина, если Т-экземпляр не уникальный и наоборот.</returns>
    public virtual async Task<bool> CheckIsInstanceExist(T item)
    {
        return await Task.FromResult(false);
    }

    /// <summary>
    /// Метод для проверки на наличие в БД идентичных экземпляров посредством прямого запроса к БД  с помощью Dapper.
    /// </summary>
    /// <param name="props">Кортеж значений, в котором первое - это наименование свойства, а второе - его значение.</param>
    /// <param name="notEquelId">Идентификатор Т-экземпляра, который не должен учитываться в поиске.</param>
    /// <returns>Истина, если Т-экземпляр не уникальный и наоборот.</returns>
    public async Task<bool> SearchValueString(List<(string propName, string propValue)> props, int notEquelId = 0)
    {
        NotifyMessage = string.Empty;

        var tablesSchemaName = typeof(T)?.GetTableAttributesValues();

        var command = (tablesSchemaName.Value.schema + "." + tablesSchemaName.Value.table).FilterDataString(props: props, notEquelId: notEquelId);

        if (command == null)
        {
            NotifyMessage += string.Concat($"Произошла ошибка на этапе формирования! " +
                                    $"набора инструкций для выполнения поиска в базе данных");
            NotifyMessage.LogError(GetType().Name, nameof(SearchValueString));
            return false;
        }

        using SqlConnection connection = new (connectionString);

        // Выполнить команду
        try
        {
            connection?.Open();

            var searchedInstances = await connection?.QueryAsync<IEnumerable<T>>(command);

            return searchedInstances?.Any() ?? false;
        }
        catch (Exception exc)
        {
            exc.LogError(GetType().FullName, nameof(SearchValueString));
            throw;
        }
        finally
        {
            connection?.Close();
        }
    }
}