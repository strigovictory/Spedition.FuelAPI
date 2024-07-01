using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Trips;

namespace Spedition.Fuel.BFF.Retrievers.Interfaces.Transport
{
    public interface ITruckRetriever
    {
        Task<TruckResponse> GetTruck(int id, CancellationToken token = default);

        Task<List<TruckResponse>> GetTrucks(CancellationToken token = default);

        Task<List<TrucksLicensePlateResponse>> GetTrucksLicensePlates(CancellationToken token = default);

        Task<List<TrucksLicensePlatesStatusesResponse>> GetTrucksLicensePlatesStatuses(CancellationToken token = default);
    }
}
