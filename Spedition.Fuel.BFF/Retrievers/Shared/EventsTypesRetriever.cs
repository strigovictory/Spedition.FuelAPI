using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Shared;

namespace Spedition.Fuel.BFF.Retrievers.Shared
{
    public class EventsTypesRetriever : IEventsTypesRetriever
    {
        public async Task<List<KitEventTypeResponse>> Get()
        {
            return new (); // TODO : wait baget
        }
    }
}
