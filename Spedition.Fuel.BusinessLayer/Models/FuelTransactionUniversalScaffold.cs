using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.Shared.Attributes;

namespace Spedition.Fuel.BusinessLayer.Models;

public class FuelTransactionUniversalScaffold: IParsedItem
{
    [Name("День", "День")]
    public DateTime? OperationDateTime_Day { get; set; }

    [Name("Час", "Час")]
    public int? OperationDateTime_Hour { get; set; }

    [Name("Минута", "Минута")]
    public int? OperationDateTime_Minute { get; set; }

    [Name("Топливная", "Топливная")]
    public string CarNum { get; set; }

    [Name("Количество", "Количество")]
    public decimal? Quantity { get; set; }

    [Name("Стоимость", "Стоимость")]
    public decimal? Cost { get; set; }

    [Name("Разновидность", "Разновидность")]
    public string FuelType { get; set; }

    public override string ToString()
    {
        return $"Транзакция " +
            $"по карте {CarNum ?? string.Empty} " +
            $"от {OperationDateTime_Day?.ToString("dd.MM.yyyy") ?? string.Empty} {OperationDateTime_Hour ?? 0}:{OperationDateTime_Minute ?? 0}, " +
            $"количество: {Quantity ?? 0}, " +
            $"тип услуги: {FuelType ?? string.Empty}, " +
            $"цена: {Cost ?? 0} ";
    }
}
