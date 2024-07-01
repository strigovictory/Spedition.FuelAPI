using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;

namespace Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

[AutoMap(typeof(FuelCardRequest), ReverseMap = true)]
public class FuelCardFullResponse
{
    [JsonInclude]
    public int Id { get; set; }

    [JsonInclude]
    public string Number { get; set; } = string.Empty;

    [JsonInclude]
    public DateTime? ExpirationDate { get; set; }

    [JsonInclude]
    public DateTime? ReceiveDate { get; set; }

    [JsonInclude]
    public DateTime? IssueDate { get; set; }

    [JsonInclude]
    public bool IsReserved { get; set; }

    [JsonInclude]
    public string Note { get; set; } = string.Empty;

    [JsonInclude]
    public bool IsArchived { get; set; }

    [JsonInclude]
    public int? CarId { get; set; }

    [JsonInclude]
    public string CarName { get; set; }

    [JsonInclude]
    public int DivisionID { get; set; }

    [JsonInclude]
    public string DivisionName { get; set; }

    [JsonInclude]
    public int ProviderId { get; set; }

    [JsonInclude]
    public string ProviderName { get; set; }

    [JsonInclude]
    public int? EmployeeId { get; set; }

    [JsonInclude]
    public string EmployeeName { get; set; }

    public override string ToString()
    {
        return $"Заправочная карта «{Number ?? string.Empty}»";
    }
}
