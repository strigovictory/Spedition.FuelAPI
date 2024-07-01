using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Geo;

namespace Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Interfaces.Shared
{
    public interface ICountryService
    {
        Task<List<CountryResponse>> Get();
    }
}
