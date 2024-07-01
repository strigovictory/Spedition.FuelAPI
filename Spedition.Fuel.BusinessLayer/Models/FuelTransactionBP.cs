using Spedition.Fuel.BusinessLayer.Enums.ReportsHeaders;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.Shared.Attributes;

namespace Spedition.Fuel.BusinessLayer.Models;

public class FuelTransactionBP : IParsedItem
{
    [Name("NrTrans", "NrTrans")]
    public string TransactionNumber { get; set; }

    [Name(nameof(TablesHeaderBP.NrWystKart), nameof(TablesHeaderBP.NrWystKart))]
    public string Card_part_1 { get; set; }

    [Name(nameof(TablesHeaderBP.NrKlienta), nameof(TablesHeaderBP.NrKlienta))]
    public string Card_part_2 { get; set; }

    [Name(nameof(TablesHeaderBP.NrKarty), nameof(TablesHeaderBP.NrKarty))]
    public string Card_part_3 { get; set; }

    [Name(nameof(TablesHeaderBP.DataTrans), nameof(TablesHeaderBP.DataTrans))]
    public string OperationDate { get; set; }

    [Name("GodzinaTransakcji", "GodzinaTransakcji")]
    public string OperationTime { get; set; }

    [Name(nameof(TablesHeaderBP.OpisProduktu), nameof(TablesHeaderBP.OpisProduktu))]
    public string FuelType { get; set; }

    [Name("Znak", "Znak")]
    public string NumberSign { get; set; }

    [Name(nameof(TablesHeaderBP.IloscLitrow), nameof(TablesHeaderBP.IloscLitrow))]
    public decimal Quantity { get; set; }

    [Name(nameof(TablesHeaderBP.CenaNetto), nameof(TablesHeaderBP.CenaNetto))]
    public decimal Cost { get; set; }

    [Name(nameof(TablesHeaderBP.WartoscFakturyBrutto), nameof(TablesHeaderBP.WartoscFakturyBrutto))]
    public decimal TotalCost { get; set; }

    [Name("KodKraju", "KodKraju")]
    public int Country { get; set; }

    [Name("Waluta", "Waluta")]
    public string Currency { get; set; }

    public override string ToString()
    {
        return $"Транзакция № {TransactionNumber ?? string.Empty} " +
            $"по карте {Card_part_1 ?? string.Empty}{Card_part_2 ?? string.Empty}{Card_part_3 ?? string.Empty} " +
            $"от {OperationDate ?? string.Empty} {OperationTime ?? string.Empty}, " +
            $"количество: {NumberSign ?? string.Empty}{Quantity}, " +
            $"тип услуги: {FuelType ?? string.Empty}, " +
            $"цена: {Cost}, общая стоимость: {TotalCost}, валюта: {Currency ?? string.Empty}, " +
            $"место заправки: {Country} ";
    }
}
