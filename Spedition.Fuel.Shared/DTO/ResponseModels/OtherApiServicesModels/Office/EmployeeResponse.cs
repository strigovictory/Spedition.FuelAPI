namespace Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Office;

public class EmployeeResponse
{
    [JsonInclude]
    public int Id { get; set; }

    [JsonIgnore]
    public string ShortName => $"{LastName} {FirstName.Substring(0, 1)}.{MiddleName.Substring(0, 1)}.";

    [JsonInclude]
    public string LastName { get; set; }

    [JsonInclude]
    public string FirstName { get; set; }

    [JsonInclude]
    public string MiddleName { get; set; }
}
