using System.ComponentModel;
using System.Drawing;
using Spedition.Fuel.Shared.DTO.ResponseModels;
using Spedition.Fuel.Shared.Interfaces;

namespace Spedition.Fuel.Shared.DTO.RequestModels.Print;

[DisplayName("Транзакции по топливным картам")]
public class TransactionPrintRequest : IPrint
{
    [DisplayName("Идентификатор транзакции в БД")]
    [JsonInclude]
    public int Id { get; set; }

    [DisplayName("Идентификатор транзакции из отчета о реализации")]
    [JsonInclude]
    public string TransactionID { get; set; }

    [DisplayName("Дата транзакции")]
    [JsonInclude]
    public DateTime OperationDate { get; set; }

    [DisplayName("Количество")]
    [JsonInclude]
    public decimal? Quantity { get; set; }

    [DisplayName("Цена")]
    [JsonInclude]
    public decimal? Cost { get; set; }

    [DisplayName("Общая стоимость")]
    [JsonInclude]
    public decimal? TotalCost { get; set; }

    [DisplayName("Транзакция проверена")]
    [JsonInclude]
    public bool IsCheck { get; set; }

    [DisplayName("Подразделение")]
    [JsonInclude]
    public string DivisionName { get; set; }

    [DisplayName("Поставщик топлива")]
    [JsonInclude]
    public string ProviderName { get; set; }

    [DisplayName("Разновидность услуги")]
    [JsonInclude]
    public string FuelTypeName { get; set; }

    [DisplayName("Валюта")]
    [JsonInclude]
    public string CurrencyName { get; set; }

    [DisplayName("Топливная карта")]
    [JsonInclude]
    public string CardName { get; set; }

    [DisplayName("Страна")]
    [JsonInclude]
    public string CountryName { get; set; }

    [DisplayName("Ссылка на водит. отчет")]
    [JsonInclude]
    public int? DriverReportId { get; set; }

    [DisplayName("Ежедневный отчет")]
    [JsonInclude]
    public bool? IsDaily { get; set; }

    [DisplayName("Ежемесячный отчет")]
    [JsonInclude]
    public bool? IsMonthly { get; set; }
}
