using System.ComponentModel;
using Spedition.Fuel.Shared.Interfaces;

namespace Spedition.Fuel.Shared.DTO.RequestModels.Print;

[DisplayName("Топливные карты")]
public class CardPrintRequest : IPrint
{
    [DisplayName("Идентификатор в БД")]
    [JsonInclude]
    public int Id { get; set; }

    [DisplayName("Номер карты")]
    [JsonInclude]
    public string Number { get; set; }

    [DisplayName("Дата поступления")]
    [JsonInclude]
    public DateTime? ReceiveDate { get; set; }

    [DisplayName("Дата ввода в эксплуатацию")]
    [JsonInclude]
    public DateTime? IssueDate { get; set; }

    [DisplayName("Дата окончания срока действия")]
    [JsonInclude]
    public DateTime? ExpirationDate { get; set; }

    [DisplayName("В резерве")]
    [JsonInclude]
    public bool IsReserve { get; set; }

    [DisplayName("В архиве")]
    [JsonInclude]
    public bool IsArchive { get; set; }

    [DisplayName("Поставщик топлива")]
    [JsonInclude]
    public string ProviderName { get; set; }

    [DisplayName("Автомобиль")]
    [JsonInclude]
    public string CarName { get; set; }

    [DisplayName("Водитель")]
    [JsonInclude]
    public string EmployeeName { get; set; }

    [DisplayName("Подразделение")]
    [JsonInclude]
    public string DivisionName { get; set; }

    [DisplayName("Примечание")]
    [JsonInclude]
    public string Note { get; set; }
}
