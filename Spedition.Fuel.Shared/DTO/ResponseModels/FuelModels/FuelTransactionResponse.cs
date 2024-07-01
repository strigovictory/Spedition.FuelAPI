using Spedition.Fuel.Shared.Helpers;

namespace Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

[AutoMap(typeof(FuelTransaction), ReverseMap = true)]
[AutoMap(typeof(NotParsedTransaction), ReverseMap = true)]
public class FuelTransactionResponse
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
    public bool? IsDayly { get; set; }

    [JsonInclude]
    public bool? IsMonthly { get; set; }

    public override string ToString()
    {
        return $"Транзакция №{(string.IsNullOrEmpty(TransactionID) ? "б/н" : TransactionID)} " +
            $"от {OperationDate.ToString("dd.MM.yyyy HH:mm:ss")}, " +
            $"количество: {Quantity ?? 0}, " +
            $"цена: {Cost.FormatDecimalToString() ?? string.Empty}, " +
            $"сумма: {TotalCost.FormatDecimalToString() ?? string.Empty} " +
            $"{(IsDayly.HasValue && IsDayly.Value ? ", ежедневный отчет" : string.Empty)} " +
            $"{(IsMonthly.HasValue && IsMonthly.Value ? ", ежемесячный отчет" : string.Empty)} ";
    }
}
