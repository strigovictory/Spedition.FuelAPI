using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Office;

namespace Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Office
{
    public class DivisionService : IDivisionService
    {
        private readonly IDivisionRetriever divisionRetriever;

        public DivisionService(IDivisionRetriever divisionRetriever)
            => this.divisionRetriever = divisionRetriever;

        public async Task<List<DivisionResponse>> GetDivisions()
        {
            return await divisionRetriever.GetDivisions();
        }
    }
}
