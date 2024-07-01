using Microsoft.Data.SqlClient;
using NPOI.SS.Formula.Functions;
using static OfficeOpenXml.ExcelErrorValue;

namespace Spedition.Fuel.BusinessLayer.Helpers;

/// <summary>
/// Класс для ADO-Net-запросов к БД.
/// </summary>
public static class SenderToDb
{
    /// <summary>
    /// Метод для формирования строковой команды на выборку в указанной таблице.
    /// </summary>
    /// <param Name="table">Наименование таблицы, в которой будет вестись поиск.</param>
    /// <param Name="props">Коллекция кортежей с названиями поля таблицы и искомого значения.</param>
    /// <param Name="notEquelId">Идентификатор экземпляра, который не участвует в поиске.</param>
    /// <returns>Строка SQL-команды.</returns>
    public static string FilterDataString(
        this string table,
        List<(string propName, string propValue)> props,
        int notEquelId = 0)
    {
        var collationValue = "COLLATE Cyrillic_General_CI_AS";

        var firstProp = props.ElementAt(0);

        var expression = $"SELECT * FROM {table} AS items " +
                         $"WHERE items.{firstProp.propName} LIKE '%{firstProp.propValue}' ";

        if (props.Count > 1)
        {
            props.RemoveAt(0); // удалить первую строку, которая уже есть в выражении
            foreach (var prop in props)
            {
                expression += $"AND items.{prop.propName} LIKE '%{prop.propValue}%' ";
            }
        }

        return notEquelId switch
        {
            0 => expression += collationValue,
            _ => expression += $"AND items.ID != {notEquelId} {collationValue}"
        };
    }

    /// <summary>
    ///  Метод для формирования строковой команды на добавление данных.
    /// </summary>
    /// <param Name="table">в какую таблицу.</param>
    /// <param Name="values">какие значения.</param>
    /// <param Name="columns">в какие поля.</param>
    /// <returns>Строка SQL-команды.</returns>
    public static string InsertDataToDB(string table, string values, string columns)
    {
        return $"INSERT INTO {table} ({columns}) VALUES ({values})";
    }

    /// <summary>
    ///  Метод для формирования строковой команды на обновление данных.
    /// </summary>
    /// <param Name="table">в какую таблицу.</param>
    /// <param Name="values">какие значения.</param>
    /// <param Name="columns">в какие поля.</param>
    /// <returns>Строка SQL-команды.</returns>
    public static string UpdateDataInDB(string table, string columnsValues, string predicate)
    {
        return $"UPDATE {table} SET {columnsValues} WHERE {predicate}";
    }
}
