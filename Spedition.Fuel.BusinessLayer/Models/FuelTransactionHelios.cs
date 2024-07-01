using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.Shared.Attributes;
using Spedition.Office.Shared.Entities;

namespace Spedition.Fuel.BusinessLayer.Models;

public class FuelTransactionHelios : IParsedItem
{
    [Name("Дата", "Дата")]
    public DateTime? OperationDateTime { get; set; }

    [Name("Карта", "Карта")]
    public string Card { get; set; }

    [Name("Товар", "Товар")]
    public string FuelType { get; set; }

    [Name("Количество", "Количество")]
    public decimal? Quantity { get; set; }

    [Name("Цена", "Цена")]
    public decimal? Cost { get; set; }

    [Name("Итого", "Итого")]
    public decimal? TotalCost { get; set; }

    public override string ToString()
    {
        return $"Транзакция " +
            $"по карте {Card ?? string.Empty} " +
            $"от {OperationDateTime?.ToString("dd.MM.yyyy HH:mm:ss") ?? string.Empty}, " +
            $"количество: {Quantity ?? 0}, " +
            $"тип услуги: {FuelType ?? string.Empty}, " +
            $"цена: {Cost ?? 0}, общая стоимость: {TotalCost ?? 0}, ";
    }
}
