using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Geo;

namespace Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Shared
{
    public class CountryService : ICountryService
    {
        private readonly ICountryRetriever retriever;

        public CountryService(ICountryRetriever retriever) => this.retriever = retriever;

        public async Task<List<CountryResponse>> Get()
        {
            return await retriever?.Get();
        }
    }
}
