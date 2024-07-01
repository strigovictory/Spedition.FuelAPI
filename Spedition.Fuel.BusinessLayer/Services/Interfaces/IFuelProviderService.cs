using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.BusinessLayer
{
    public interface IFuelProviderService
    {
        Task<List<FuelProviderResponse>> GetProviders(CancellationToken token = default);
    }
}
