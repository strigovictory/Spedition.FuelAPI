using NPOI.OpenXmlFormats.Wordprocessing;
using Spedition.Fuel.BusinessLayer.Enums;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.Shared.Attributes;

namespace Spedition.Fuel.BusinessLayer.Models;

public class FuelTransactionLider : IParsedItem
{
    [Name(nameof(ExcelColumnsNames.A), nameof(ExcelColumnsNames.A))]
    public DateTime? OperationDateTime { get; set; }

    [Name(nameof(ExcelColumnsNames.J), nameof(ExcelColumnsNames.J))]
    public string Card { get; set; }

    [Name(nameof(ExcelColumnsNames.E), nameof(ExcelColumnsNames.E))]
    public string FuelType { get; set; }

    [Name(nameof(ExcelColumnsNames.G), nameof(ExcelColumnsNames.G))]
    public decimal? Quantity { get; set; }

    [Name(nameof(ExcelColumnsNames.F), nameof(ExcelColumnsNames.F))]
    public decimal? Cost { get; set; }

    [Name(nameof(ExcelColumnsNames.H), nameof(ExcelColumnsNames.H))]
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
