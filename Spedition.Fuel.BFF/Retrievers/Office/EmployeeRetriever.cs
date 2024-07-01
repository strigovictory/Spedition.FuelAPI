using AutoMapper;
using Newtonsoft.Json.Linq;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Office;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Trips;
using Spedition.Fuel.Shared.Settings.Configs;

namespace Spedition.Fuel.BFF.Retrievers.Office;

public class EmployeeRetriever : RetrieverBase, IEmployeeRetriever
{
    protected override string UriDomen => "employees";

    public EmployeeRetriever(IOptions<OfficeConfigs> config, IMapper mapper)
        : base(config, mapper)
    {
    }

    public async Task<List<EmployeeResponse>> GetEmployees()
    {
        return await SendRequest<List<EmployeeResponse>>();
    }
}
