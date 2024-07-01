using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.Shared.Attributes;
using Spedition.Fuel.Shared.Helpers;

namespace Spedition.Fuel.BusinessLayer.Models;

public class FuelTransactionAdnoc : IParsedItem
{
    [Name("№", "№")]
    public string TrucksNumber { get; set; }

    [Name("Контрагент", "Контрагент")]
    public string DivisionName { get; set; }

    [Name("Дата", "Дата")]
    public DateTime OperationDate { get; set; }

    [Name("Время", "Время")]
    public DateTime OperationTime { get; set; }

    public string Place { get; set; }

    [Name("Вид", "Вид")]
    public string FuelType { get; set; }

    [Name("Количество", "Количество")]
    public decimal Quantity { get; set; }

    [Name("Цена", "Цена")]
    public decimal Cost { get; set; }

    [Name("Сумма", "Сумма")]
    public decimal TotalCost { get; set; }

    public override string ToString()
    {
        return $"Транзакция " +
            $"от {OperationDate.ToString("dd.MM.yyyy")} {OperationTime.ToString("HH:mm:ss")}, " +
            $"количество: {Quantity}, " +
            $"цена: {Cost.FormatDecimalToString() ?? string.Empty}, " +
            $"сумма: {TotalCost.FormatDecimalToString() ?? string.Empty}, " +
            $"услуга: {FuelType ?? string.Empty}, " +
            $"место заправки: {Place ?? string.Empty}, " +
            $"подразделение: {DivisionName ?? string.Empty}, " +
            $"авто: {TrucksNumber ?? string.Empty}. ";
    }
}
