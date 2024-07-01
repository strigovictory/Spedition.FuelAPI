namespace Spedition.Fuel.Shared.DTO.ResponseModels;

public class EntityModifiedResponse
{
    [JsonInclude]
    public DateTime? ModifiedOn { get; set; }

    [JsonInclude]
    public string ModifiedBy { get; set; }
}
