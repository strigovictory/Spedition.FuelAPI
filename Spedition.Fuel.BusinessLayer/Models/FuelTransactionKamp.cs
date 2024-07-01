using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.Shared.Attributes;

namespace Spedition.Fuel.BusinessLayer.Models;

public class FuelTransactionKamp : IParsedItem
{
    [Name("Дата", "Дата")]
    public DateTime? OperationDateTime { get; set; }

    [Name("знак", "знак")]
    public string CarNum { get; set; }

    [Name("вид", "вид")]
    public string FuelType { get; set; }

    [Name("заправ", "заправ")]
    public decimal? Quantity { get; set; }

    [Name("цена", "цена")]
    public decimal? Cost { get; set; }

    [Name("сумма", "сумма")]
    public decimal? TotalCost { get; set; }

    public override string ToString()
    {
        return $"Транзакция " +
            $"по карте {CarNum ?? string.Empty} " +
            $"от {OperationDateTime?.ToString("dd.MM.yyyy HH:mm:ss") ?? string.Empty}, " +
            $"количество: {Quantity ?? 0}, " +
            $"тип услуги: {FuelType ?? string.Empty}, " +
            $"цена: {Cost ?? 0}, общая стоимость: {TotalCost ?? 0}, ";
    }
}
