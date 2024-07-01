using System.Diagnostics;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.Helpers;

namespace Spedition.Fuel.Shared.Entities
{
    [Table("FuelCardsEntries", Schema = "dbo")]
    [AutoMap(typeof(FuelTransactionRequest), ReverseMap = true)]
    [AutoMap(typeof(FuelTransactionFullResponse), ReverseMap = true)]
    [AutoMap(typeof(FuelTransactionResponse), ReverseMap = true)]
    [AutoMap(typeof(FuelTransactionShortResponse), ReverseMap = true)]
    [AutoMap(typeof(NotParsedTransaction), ReverseMap = true)]
    public class FuelTransaction
    {
        [Key]
        public int Id { get; set; }

        [Column("CardTypeID")]
        public int ProviderId { get; set; }

        public DateTime OperationDate { get; set; }

        [Column("FuelType")]
        public int FuelTypeId { get; set; }

        public decimal? Quantity { get; set; }

        public decimal? Cost { get; set; }

        public int? CurrencyId { get; set; }

        public decimal? TotalCost { get; set; }

        public int CardId { get; set; }

        public bool IsCheck { get; set; }

        public int? CountryId { get; set; }

        public int? DriverReportId { get; set; }

        public string TransactionID { get; set; }

        public virtual FuelCard Card { get; set; }

        public virtual FuelProvider Provider { get; set; }

        public virtual FuelType FuelType { get; set; }

        public bool? IsDaily { get; set; }

        public bool? IsMonthly { get; set; }

        public override string ToString()
        {
            return $"Транзакция id = {Id}" +
                $" №{(string.IsNullOrEmpty(TransactionID) ? "б/н" : TransactionID)} " +
                $"от {OperationDate.ToString("dd.MM.yyyy HH:mm:ss")}, " +
                $"количество: {Quantity ?? 0}, " +
                $"цена: {Cost.FormatDecimalToString() ?? string.Empty}, " +
                $"сумма: {TotalCost.FormatDecimalToString() ?? string.Empty} " +
                $"{(IsDaily.HasValue && IsDaily.Value ? ", ежедневный отчет" : string.Empty)} " +
                $"{(IsMonthly.HasValue && IsMonthly.Value ? ", ежемесячный отчет" : string.Empty)} ";
        }
    }
}
