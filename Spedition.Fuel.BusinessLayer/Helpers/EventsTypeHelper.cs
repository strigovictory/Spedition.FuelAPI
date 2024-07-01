using Spedition.Fuel.BusinessLayer.Enums;
using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Shared;

namespace Spedition.Fuel.BusinessLayer.Helpers
{
    public static class EventsTypeHelper
    {
        public static int GetEventTypeId(this EventsTypeName eventsTypeName, IEnumerable<KitEventTypeResponse> eventsTypes)
        {
            return eventsTypes?.FirstOrDefault(eventsType => eventsType.Name.ToLower() == eventsTypeName.ToString().ToLower())?.Id ?? default;
        }
    }
}
