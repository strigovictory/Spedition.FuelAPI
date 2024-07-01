using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Shared;

namespace Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Interfaces.Shared
{
    public interface IEventsTypeService
    {
        Task<List<KitEventTypeResponse>> Get();
    }
}
