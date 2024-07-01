using System.Text.Json.Serialization;
using NPOI.SS.Formula.Functions;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Spedition.Fuel.BusinessLayer.Models.E100;

public class E100Transaction : IParsedItem
{
    public string UnId { get; set; }

    public string Card { get; set; }

    public string Card_shortname { get; set; }

    public string Auto { get; set; }

    public bool Confirmed { get; set; }

    public decimal Price { get; set; }

    public decimal Volume { get; set; }

    public decimal Sum { get; set; }

    public int Service_id { get; set; }

    public int Category { get; set; }

    public DateTime? Datetime_insert { get; set; }

    public DateTime Date { get; set; }

    public string Service_name { get; set; }

    public string Station_id { get; set; }

    public string Address { get; set; }

    string Brand { get; set; }

    public string Currency { get; set; }

    public override string ToString()
    {
        return $"Транзакция " +
            $"от {Date}, " +
            $"количество: {Volume}, " +
            $"цена: {Price.FormatDecimalToString() ?? string.Empty}, " +
            $"сумма: {Sum.FormatDecimalToString() ?? string.Empty}, " + 
            $"услуга: {Service_name ?? string.Empty}, " +
            $"место заправки: {Address ?? string.Empty}, " +
            $"карта: {Card}, " +
            $"авто: {Auto ?? string.Empty}, " +
            $"идентификатор: {UnId}. ";
    }
}