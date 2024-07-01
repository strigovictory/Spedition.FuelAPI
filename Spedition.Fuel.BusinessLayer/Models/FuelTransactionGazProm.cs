using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.Shared.Attributes;

namespace Spedition.Fuel.BusinessLayer.Models;

public class FuelTransactionGazProm : IParsedItem
{
    [Name("Гр. номер карты", "Карта", "Карта")]
    public string Card { get; set; }

    [Name("Дата", "Дата", "Дата")]
    public DateTime OperationDate { get; set; }

    [Name("Услуга", "Услуга", "Услуга")]
    public string FuelType { get; set; }

    [Name("Количество", "Количество", "Количество")]
    public decimal Quantity { get; set; }

    [Name("Цена на ТО", "Цена", "Цена")]
    public decimal Cost { get; set; }

    [Name("Стоимость со скидкой", "Стоимость", "Стоимость")]
    public decimal TotalCost { get; set; }

    [Name("Адрес ТО", "", "Адрес (ТО)")]
    public string Country { get; set; }

    [Name("", "", "Валюта")]
    public string Currency { get; set; }

    // Наименование  страны из названия листа в книге excel - при невозможности парсинга из тела таблицы
    public string CountryWorksheet { get; set; }

    public override string ToString()
    {
        return $"Транзакция " +
            $"по карте {Card ?? string.Empty} " +
            $"от {OperationDate.ToString("dd.MM.yyyy HH:mm:ss")}, " +
            $"количество: {Quantity}, " +
            $"тип услуги: {FuelType ?? string.Empty}, " +
            $"цена: {Cost}, общая стоимость: {TotalCost}, валюта: {Currency ?? string.Empty}, " +
            $"место заправки: {Country ?? string.Empty} ";
    }
}
