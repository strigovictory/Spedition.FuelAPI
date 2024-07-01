using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Office;

namespace Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Interfaces.Office
{
    public interface IDivisionService
    {
        Task<List<DivisionResponse>> GetDivisions();
    }
}
