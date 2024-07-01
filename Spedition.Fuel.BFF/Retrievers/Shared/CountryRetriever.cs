using AutoMapper;
using Microsoft.Extensions.Configuration;
using Spedition.Fuel.Client.Helpers;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Geo;
using Spedition.Fuel.Shared.Settings.Configs;

namespace Spedition.Fuel.BFF.Retrievers.Shared
{
    public class CountryRetriever : RetrieverBase, ICountryRetriever
    {
        public CountryRetriever(IOptions<GeoConfigs> config, IMapper mapper) 
            : base(config, mapper)
        {
        }

        protected override string UriDomen => "countries";

        public async Task<List<CountryResponse>> Get(CancellationToken token = default)
        {
            return await SendRequest<List<CountryResponse>>(token: token);
        }
    }
}
