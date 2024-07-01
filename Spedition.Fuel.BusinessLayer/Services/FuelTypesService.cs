using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Spedition.Fuel.BusinessLayer.Services.BaseServices;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.BusinessLayer.Services
{
    public class FuelTypesService : FuelRepositoriesBase<FuelTypeResponse, FuelTypeResponse, FuelType>, IFuelTypesService
    {
        public FuelTypesService(
            IWebHostEnvironment env,
            FuelRepositories fuelRepositories,
            IConfiguration configuration,
            IMapper mapper)
            : base(fuelRepositories, env, configuration, mapper)
        {
        }

        public async Task<List<FuelTypeResponse>> GetFuelTypes(CancellationToken token = default)
        {
            return (await fuelRepositories?.FuelTypes?.GetAsync(token))?.Select(ftype => mapper?.Map<FuelTypeResponse>(ftype))?.ToList() ?? new();
        }
    }
}
