using Spedition.Fuel.Shared.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

[AutoMap(typeof(FuelTransaction), ReverseMap = true)]
public class FuelTransactionShortResponse : IComparable
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
    public bool? IsDaily { get; set; }

    [JsonInclude]
    public bool? IsMonthly { get; set; }

    public override string ToString()
    {
        return $"Транзакция №{(string.IsNullOrEmpty(TransactionID) ? "б/н" : TransactionID)} " +
            $"от {OperationDate.ToString("dd.MM.yyyy HH:mm:ss")}, " +
            $"количество: {Quantity ?? 0}, " +
            $"цена: {Cost.FormatDecimalToString() ?? string.Empty}, " +
            $"сумма: {TotalCost.FormatDecimalToString() ?? string.Empty} " +
            $"{(IsDaily.HasValue && IsDaily.Value ? ", ежедневный отчет" : string.Empty)} " +
            $"{(IsMonthly.HasValue && IsMonthly.Value ? ", ежемесячный отчет" : string.Empty)} ";
    }

    public int CompareTo(object obj)
    {
        return obj is FuelTransactionShortResponse source
            && source.Id == Id
            && source.TransactionID == TransactionID
            && source.OperationDate == OperationDate
            && source.Quantity == Quantity
            && source.Cost == Cost
            && source.TotalCost == TotalCost ? 0 : -1;
    }
}
