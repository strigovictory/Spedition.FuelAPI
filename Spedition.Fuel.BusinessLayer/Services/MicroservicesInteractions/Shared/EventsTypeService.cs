using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Shared;

namespace Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Shared;

public class EventsTypeService : IEventsTypeService
{
    private readonly IEventsTypesRetriever eventsTypesRetriever;

    public EventsTypeService(IEventsTypesRetriever eventsTypesRetriever)
        => this.eventsTypesRetriever = eventsTypesRetriever;

    public async Task<List<KitEventTypeResponse>> Get()
    {
        return await eventsTypesRetriever.Get();
    }
}
