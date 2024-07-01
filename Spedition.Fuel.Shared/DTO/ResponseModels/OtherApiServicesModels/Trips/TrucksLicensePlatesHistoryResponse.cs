namespace Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Trips;

public class TrucksLicensePlatesHistoryResponse
{
    public int Id { get; set; }

    public string LicensePlate { get; set; } = null!;

    public DateTime BeginDate { get; set; }

    public DateTime? EndDate { get; set; }
}