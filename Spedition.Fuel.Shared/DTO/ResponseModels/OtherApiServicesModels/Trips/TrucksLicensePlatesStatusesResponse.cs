namespace Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Trips;

public class TrucksLicensePlatesStatusesResponse
{
    public int TruckId { get; set; }

    public int DivisionId { get; set; }

    public List<TrucksLicensePlatesHistoryResponse> TrucksLicensePlates { get; set; }

    public List<TrucksStatusesHistoryResponse> TrucksStatuses { get; set; }
}