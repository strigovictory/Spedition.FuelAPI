using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NPOI.SS.Formula.Functions;
using Spedition.Fuel.BusinessLayer.Services.BaseServices;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.BusinessLayer.Services
{
    public class FuelProvidersService : FuelRepositoriesBase<FuelProviderResponse, FuelProviderResponse, FuelProvider>, IFuelProviderService
    {
        public FuelProvidersService(
            IWebHostEnvironment env,
            FuelRepositories fuelRepositories,
            IConfiguration configuration,
            IMapper mapper)
            : base(fuelRepositories, env, configuration, mapper)
        {
        }

        public async Task<List<FuelProviderResponse>> GetProviders(CancellationToken token = default)
        {
            return (await fuelRepositories?.Providers?.GetAsync(token))?.Select(prov => mapper?.Map<FuelProviderResponse>(prov))?.ToList() ?? new();
        }
    }
}
