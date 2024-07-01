using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Shared;

namespace Spedition.Fuel.BFF.Retrievers.Interfaces.Shared
{
    public interface IEventsTypesRetriever
    {
        Task<List<KitEventTypeResponse>> Get();
    }
}
