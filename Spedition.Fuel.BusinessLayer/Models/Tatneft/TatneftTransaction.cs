using Spedition.Fuel.BusinessLayer.Models.Interfaces;

namespace Spedition.Fuel.BusinessLayer.Models.Tatneft;

public class TatneftTransaction : DivisionBase, IParsedItem
{
    public string id { get; set; } // "11111111111"

    public string date { get; set; } // "2022-07-18 13:39:53"

    public string card_num { get; set; } // "11111111111111"

    public string car_id { get; set; } // "1"

    public string address { get; set; } // "RUS;Russia Novousmanskij r-n TN AZS 496"

    public decimal volume { get; set; } // "350"

    public decimal price { get; set; } // "50.69"

    public decimal sum { get; set; } // "17741.5"

    public string fuel_type { get; set; } // "ДТ"

    public override string ToString()
    {
        return $"Транзакция " +
            $"от {date}, " +
            $"количество: {volume}, " +
            $"цена: {price.FormatDecimalToString() ?? string.Empty}, " +
            $"сумма: {sum.FormatDecimalToString() ?? string.Empty}, " +
            $"услуга: {fuel_type ?? string.Empty}, " +
            $"место заправки: {address ?? string.Empty}, " +
            $"карта: {card_num}, " +
            $"авто: {car_id ?? string.Empty}, " +
            $"идентификатор: {id}. ";
    }
}