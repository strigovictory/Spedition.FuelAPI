using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.BusinessLayer.Services.Interfaces
{
    public interface IFuelTypesService
    {
        Task<List<FuelTypeResponse>> GetFuelTypes(CancellationToken token = default);
    }
}
