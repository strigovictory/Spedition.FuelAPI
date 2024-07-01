namespace Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Office;

public class DivisionResponse
{
    [JsonInclude]
    public int Id { get; set; }

    [JsonInclude]
    public string Name { get; set; }

    [JsonInclude]
    public bool IsMainDivision { get; set; }
}
