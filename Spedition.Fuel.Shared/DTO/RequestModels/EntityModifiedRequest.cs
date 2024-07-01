namespace Spedition.Fuel.Shared.DTO.ResponseModels;

public class EntityModifiedRequest
{
    [JsonInclude]
    public DateTime? ModifiedOn { get; set; }

    [JsonInclude]
    public string ModifiedBy { get; set; }
}
