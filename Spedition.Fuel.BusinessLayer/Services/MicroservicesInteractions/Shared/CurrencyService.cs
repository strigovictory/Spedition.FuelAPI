using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Finance;

namespace Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Shared
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyRetriever retriever;

        public CurrencyService(ICurrencyRetriever retriever) => this.retriever = retriever;

        public async Task<List<CurrencyResponse>> Get()
        {
            return await retriever?.Get();
        }
    }
}
