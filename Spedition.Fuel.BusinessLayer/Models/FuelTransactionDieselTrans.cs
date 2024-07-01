using Spedition.Fuel.BusinessLayer.Enums.ReportsHeaders;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.Shared.Attributes;
using static Spedition.Fuel.BusinessLayer.Services.Parsers.ParserDieselTrans;

namespace Spedition.Fuel.BusinessLayer.Models;

public class FuelTransactionDieselTrans : IParsedItem
{
    [Name(nameof(TableHeaderDieselTrans.Дата), nameof(TableHeaderDieselTrans.Дата))]
    public DateTime? OperationDateTime { get; set; }

    [Name(nameof(TableHeaderDieselTrans.Номер), nameof(TableHeaderDieselTrans.Номер))]
    public string Card { get; set; }

    [Name(nameof(TableHeaderDieselTrans.Вид), nameof(TableHeaderDieselTrans.Вид))]
    public string FuelType { get; set; }

    [Name(nameof(TableHeaderDieselTrans.Количество), nameof(TableHeaderDieselTrans.Количество))]
    public decimal? Quantity { get; set; }

    [Name(nameof(TableHeaderDieselTrans.Цена), nameof(TableHeaderDieselTrans.Цена))]
    public decimal? Cost { get; set; }

    [Name(nameof(TableHeaderDieselTrans.Стоимость), nameof(TableHeaderDieselTrans.Стоимость))]
    public decimal? TotalCost { get; set; }

    public override string ToString()
    {
        return $"Транзакция " +
            $"по карте {Card ?? string.Empty} " +
            $"от {OperationDateTime?.ToString("dd.MM.yyyy HH:mm:ss") ?? string.Empty}, " +
            $"количество: {Quantity ?? 0}, " +
            $"тип услуги: {FuelType ?? string.Empty}, " +
            $"цена: {Cost ?? 0}, общая стоимость: {TotalCost ?? 0} ";
    }
}
