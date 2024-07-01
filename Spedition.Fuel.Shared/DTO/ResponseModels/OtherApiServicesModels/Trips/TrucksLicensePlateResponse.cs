namespace Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Trips;

public class TrucksLicensePlateResponse
{
    [JsonInclude]
    public int Id { get; set; }

    [JsonInclude]
    public int TruckId { get; set; }

    [JsonInclude]
    public string LicensePlate { get; set; }

    [JsonInclude]
    public DateTime BeginDate { get; set; }

    [JsonInclude]
    public DateTime? EndDate { get; set; }
}
