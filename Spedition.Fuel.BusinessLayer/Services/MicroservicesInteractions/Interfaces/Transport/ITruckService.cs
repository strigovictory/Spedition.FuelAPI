using System.Threading.Tasks;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Trips;

namespace Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Interfaces.Transport
{
    public interface ITruckService
    {
        Task<List<TruckResponse>> GetTrucks(CancellationToken token = default);

        Task<TruckResponse> GetTruck(int id, CancellationToken token = default);

        Task<List<TrucksLicensePlateResponse>> GetTrucksLicensePlates(CancellationToken token = default);

        Task<List<TrucksLicensePlatesStatusesResponse>> GetTrucksLicensePlatesStatuses(CancellationToken token = default);
    }
}
