namespace Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Shared
{
    [AutoMap(typeof(KitEventType), ReverseMap = true)]
    public class KitEventTypeResponse
    {
        [JsonInclude]
        public int Id { get; set; }

        [JsonInclude]
        public string Name { get; set; }
    }
}
