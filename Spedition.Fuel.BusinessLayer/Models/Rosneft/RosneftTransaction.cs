using System;
using System.Collections.Generic;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Models.Rosneft;

namespace Spedition.Fuel.BusinessLayer.Models.Rosneft;

public class RosneftTransaction :  RosneftErrorBase, IParsedItem
{
    public string Date { get; set; } // "2016-07-01T20:50:17"

    public string Card { get; set; } // "7826010000000436"

    public RosneftOperationsType Type { get; set; } // 24

    public decimal Value { get; set; } // 3.0

    public decimal Sum { get; set; } // 29.70

    public decimal DSum { get; set; } // 1.50,

    public decimal Price { get; set; } // 9.90,

    public decimal DPrice { get; set; } // 0.50,

    public string Contract { get; set; } // "ISS01TEST",

    public string Address { get; set; } // "Россия, Московская область, г. Балашиха, Балашихинский рн, ш.Энтузиастов, вл. 1, трасса М7, 18 км, Москва-Казань-Уфа (\"Волга\"), справа"
    
    public string GCode { get; set; } // "gkafe",
    
    public string GName { get; set; } // "Кафе",
    
    public string Holder { get; set; } // "",
    
    public string Code { get; set; } // "9422",
    
    public string DTL { get; set; } // "Молоко 50мл",
    
    public string Ref { get; set; } // "9418",
    
    public string GCat { get; set; } // "SERVICE",
    
    public string PosCode { get; set; } // "AZS102328",
    
    public decimal Vat { get; set; } // 20.0

    public override string ToString()
    {
        return $"Транзакция " +
            $"от {Date}, " +
            $"количество: {Value}, " +
            $"цена: {Price.FormatDecimalToString() ?? string.Empty}, " +
            $"сумма: {Sum.FormatDecimalToString() ?? string.Empty}, " +
            $"услуга: {GName ?? string.Empty}, " +
            $"место заправки: {Address ?? string.Empty}, " +
            $"карта: {Card}, " +
            $"идентификатор: {Code}. ";
    }
}