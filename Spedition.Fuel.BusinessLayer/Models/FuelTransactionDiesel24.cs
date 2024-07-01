using Spedition.Fuel.BusinessLayer.Enums.ReportsHeaders.Daily;
using Spedition.Fuel.BusinessLayer.Enums.ReportsHeaders.Monthly;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.Shared.Attributes;

namespace Spedition.Fuel.BusinessLayer.Models;

public class FuelTransactionDiesel24 : IParsedItem
{
    [Name(nameof(MonthlyTablesHeaderDiesel24.Card), nameof(DailyTablesHeaderDiesel24.Card))]
    public string Card { get; set; }

    [Name(nameof(MonthlyTablesHeaderDiesel24.Datum), nameof(DailyTablesHeaderDiesel24.Date))]
    public DateTime OperationDate { get; set; }

    [Name(nameof(MonthlyTablesHeaderDiesel24.Zeit), nameof(DailyTablesHeaderDiesel24.Time))]
    public string OperationTime { get; set; }

    [Name(nameof(MonthlyTablesHeaderDiesel24.Artikelgruppe), nameof(DailyTablesHeaderDiesel24.ArtCd))]
    public string FuelType { get; set; }

    [Name(nameof(MonthlyTablesHeaderDiesel24.Menge), nameof(DailyTablesHeaderDiesel24.Qty))]
    public decimal Quantity { get; set; }

    //[Name("Einzelpreis")]
    //public decimal Cost { get; set; } // с 17.10.22 Метик сказал этого столбца в отчетах не будет

    [Name(nameof(MonthlyTablesHeaderDiesel24.USt), nameof(DailyTablesHeaderDiesel24.Station))]
    public string Country { get; set; }

    public override string ToString()
    {
        return $"Транзакция по карте {Card ?? string.Empty}" +
            $"от {OperationDate.ToString("dd.MM.yyyy")}{OperationTime}, " +
            $"количество: {Quantity}, " +
            $"тип услуги: {FuelType ?? string.Empty} " +
            $"место заправки: {Country} ";
    }
}
