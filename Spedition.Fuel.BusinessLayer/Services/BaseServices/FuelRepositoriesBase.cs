using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Spedition.Fuel.BusinessLayer.Services.BaseServices;

public class FuelRepositoriesBase<TSuccess, TNotSuccess, TSearch> : GroupDoubleActionBase<TSuccess, TNotSuccess, TSearch>
{
    protected readonly FuelRepositories fuelRepositories;

    public FuelRepositoriesBase(
        FuelRepositories fuelRepositories,
        IWebHostEnvironment env,
        IConfiguration configuration,
        IMapper mapper)
        : base(env, configuration, mapper)
    {
        this.fuelRepositories = fuelRepositories;
    }
}
