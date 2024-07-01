using Newtonsoft.Json;

namespace Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Geo;

public class CountryResponse
{
    [JsonInclude]
    public int Id { get; set; }

    [JsonInclude]
    public string Name { get; set; }

    [JsonInclude]
    public string NameEng { get; set; }

    [JsonInclude]
    [JsonPropertyName("Code")]
    [JsonProperty("Code")]
    public string CountryCode { get; set; }

    [JsonInclude]
    public string BeltranssatName { get; set; }

    public string NameOfficial { get; set; }

    [JsonInclude]
    public string NameOfficialEng { get; set; }
}