namespace Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;

public class FuelTransactionRequest
{
    [JsonInclude]
    public int Id { get; set; }

    [JsonInclude]
    public string TransactionID { get; set; }

    [JsonInclude]
    public DateTime OperationDate { get; set; }

    [JsonInclude]
    public decimal? Quantity { get; set; }

    [JsonInclude]
    public decimal? Cost { get; set; }

    [JsonInclude]
    public decimal? TotalCost { get; set; }

    [JsonInclude]
    public bool IsCheck { get; set; }

    [JsonInclude]
    public int ProviderId { get; set; }

    [JsonInclude]
    public int FuelTypeId { get; set; }

    [JsonInclude]
    public int? CurrencyId { get; set; }

    [JsonInclude]
    public int CardId { get; set; }

    [JsonInclude]
    public int? CountryId { get; set; }

    [JsonInclude]
    public int? DriverReportId { get; set; }

    [JsonInclude]
    public bool? IsDaily { get; set; }

    [JsonInclude]
    public bool? IsMonthly { get; set; }
}
