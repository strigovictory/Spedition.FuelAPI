using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.Shared.DTO.ResponseModels;

namespace Spedition.Fuel.BusinessLayer.Models.BPApi;

[AutoMap(typeof(BPTransaction), ReverseMap = false)]
public class BPApiTransaction : IParsedItem
{
    public string Id { get; set; }

    public string OriginalCurrency { get; set; }

    public string OriginalCountryCode { get; set; }

    public decimal? OriginalVatRate { get; set; }

    public decimal? OriginalVatAmount { get; set; }

    public decimal? OriginalGrossAmount { get; set; }

    public decimal? OriginalNettoAmount { get; set; }

    public string ReverseCharge { get; set; }

    public string TransactionKey { get; set; }

    public int? InvoiceType { get; set; }

    public string LocalCurrency { get; set; }

    public DateTime? InvoiceNotifyDate { get; set; }

    public string StateCurrency { get; set; }

    public string BunkPriceInd { get; set; }

    public int? VehDriverCode { get; set; }

    public int? OriginalValue { get; set; }

    public decimal? ScheduledUnitPrice { get; set; }

    public string PriceAreaCode { get; set; }

    public int? FeeItemCount { get; set; }

    public string Info { get; set; }

    public int? Odometer { get; set; }

    public decimal? SupplierVatRate { get; set; }

    public decimal? SupplierGrossValue { get; set; }

    public decimal? SupplierGrossRebate { get; set; }

    public decimal? SupplierVatValue { get; set; }

    public decimal? SupplierNettoValue { get; set; }

    public decimal? SupplierUnitPrice { get; set; }

    public string Currency { get; set; }

    public string CountryCode { get; set; }

    public decimal? VatRate { get; set; }

    public decimal? GrossValue { get; set; }

    public decimal? GrossRebate { get; set; }

    public decimal? VatValue { get; set; }

    public decimal? NettoValue { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? Quantity { get; set; }

    public string ProdCat { get; set; }

    public string ProdDesc { get; set; } // тип топлива

    public int? ProdCode { get; set; }

    public int? LosType { get; set; }

    public string LosNo { get; set; }

    public string VentPart { get; set; }

    public string LosName { get; set; }

    public string PickupType { get; set; }

    public string TxnWorkday { get; set; }

    public string TxnNo { get; set; }

    public DateTime? TxnDatetime { get; set; }

    public string IccInvoiceNo { get; set; }

    public string VehicleReg { get; set; }

    public string CostCentreName { get; set; }

    public string CardHolderName { get; set; }

    public string Cc2 { get; set; }

    public int? CardSerialNo { get; set; }

    public string InvoiceNo { get; set; }

    public string AuthorityId { get; set; }

    public string IssueNo { get; set; }

    public string TxnId2 { get; set; }

    public string SubCostCentre { get; set; }

    public int? CostCentreNo { get; set; }

    public string CustomerId { get; set; }

    public string ParentId { get; set; }

    public string OpuCode { get; set; }
}