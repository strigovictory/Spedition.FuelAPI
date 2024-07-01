using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Office;

namespace Spedition.Fuel.BFF.Retrievers.Interfaces.Office
{
    public interface IDivisionRetriever
    {
        Task<List<DivisionResponse>> GetDivisions();
    }
}
