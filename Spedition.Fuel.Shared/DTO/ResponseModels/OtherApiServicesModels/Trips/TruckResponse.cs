using Newtonsoft.Json;

namespace Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Trips;

public class TruckResponse
{
    [JsonInclude]
    public int Id { get; set; }

    [JsonInclude]
    public string LicensePlate { get; set; }

    [JsonInclude]
    public bool IsDisabled { get; set; }

    [JsonInclude]
    public int? DivisionId { get; set; }
}