using AutoMapper;
using divisionsGRPC;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Office;
using Spedition.Fuel.Shared.Settings.Configs;
using static divisionsGRPC.DivisionsGRPCService;

namespace Spedition.Fuel.BFF.Retrievers.Office
{
    public class DivisionRetriever : RetrieverBase, IDivisionRetriever
    {
        protected override string UriDomen => "divisions";

        public DivisionRetriever(IOptions<OfficeConfigs> config, IMapper mapper)
            : base(config, mapper)
        {
        }

        public async Task<List<DivisionResponse>> GetDivisions()
        {
            return await SendRequest<List<DivisionResponse>>();
        }
    }
}
