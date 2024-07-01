using Spedition.Fuel.Shared.DTO;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.Entities;

namespace Spedition.Fuel.Client.Connectors.Interfaces
{
    public interface IFuelProvidersClientConnector
    {
        Task<List<FuelProviderResponse>> GetProviders(CancellationToken token = default);
    }
}
