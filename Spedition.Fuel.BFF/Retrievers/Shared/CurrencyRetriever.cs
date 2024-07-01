using AutoMapper;
using Microsoft.Extensions.Configuration;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Finance;
using Spedition.Fuel.Shared.Settings.Configs;
using Spedition.Office.Shared.Entities;

namespace Spedition.Fuel.BFF.Retrievers.Shared
{
    public class CurrencyRetriever : RetrieverBase, ICurrencyRetriever
    {
        public CurrencyRetriever(IOptions<FinanceConfigs> config, IMapper mapper)
            : base(config, mapper)
        {
        }

        protected override string UriDomen => "currencies";

        public async Task<List<CurrencyResponse>> Get(CancellationToken token = default)
        {
            return await SendRequest<List<CurrencyResponse>>(token: token);
        }
    }
}
