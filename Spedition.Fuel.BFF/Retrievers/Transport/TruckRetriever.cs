using AutoMapper;
using Newtonsoft.Json.Linq;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Trips;
using Spedition.Fuel.Shared.Settings.Configs;

namespace Spedition.Fuel.BFF.Retrievers.Transport
{
    public class TruckRetriever : RetrieverBase, ITruckRetriever
    {
        protected override string UriDomen => "trucks";

        public TruckRetriever(IOptions<TripsConfigs> config, IMapper mapper)
            : base(config, mapper)
        {
        }

        public async Task<List<TruckResponse>> GetTrucks(CancellationToken token = default)
        {
            return await SendRequest<List<TruckResponse>>(token: token);
        }

        public async Task<TruckResponse> GetTruck(int id, CancellationToken token = default)
        {
            UriSegment = id.ToString();
            return await SendRequest<TruckResponse>(token: token);
        }

        public async Task<List<TrucksLicensePlateResponse>> GetTrucksLicensePlates(CancellationToken token = default)
        {

            UriSegment = "licenseplates";
            return await SendRequest<List<TrucksLicensePlateResponse>>(token: token);
        }

        public async Task<List<TrucksLicensePlatesStatusesResponse>> GetTrucksLicensePlatesStatuses(CancellationToken token = default)
        {
            UriSegment = "licenseplatesstatuses";
            return await SendRequest<List<TrucksLicensePlatesStatusesResponse>>(token: token);
        }
    }
}
