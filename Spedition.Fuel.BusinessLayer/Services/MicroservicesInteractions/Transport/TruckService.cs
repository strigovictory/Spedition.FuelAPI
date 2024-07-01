using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Trips;

namespace Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Transport
{
    public class TruckService : ITruckService
    {
        private readonly ITruckRetriever retriever;

        public TruckService(ITruckRetriever retriever) => this.retriever = retriever;

        public async Task<TruckResponse> GetTruck(int id, CancellationToken token = default)
        {
            return await retriever.GetTruck(id, token);
        }

        public async Task<List<TruckResponse>> GetTrucks(CancellationToken token = default)
        {
            return await retriever.GetTrucks();
        }

        public async Task<List<TrucksLicensePlateResponse>> GetTrucksLicensePlates(CancellationToken token = default)
        {
            return await retriever.GetTrucksLicensePlates();
        }

        public async Task<List<TrucksLicensePlatesStatusesResponse>> GetTrucksLicensePlatesStatuses(CancellationToken token = default)
        {
            return await retriever.GetTrucksLicensePlatesStatuses(token);
        }
    }
}
