using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Finance;

namespace Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Interfaces.Shared
{
    public interface ICurrencyService
    {
        Task<List<CurrencyResponse>> Get();
    }
}
