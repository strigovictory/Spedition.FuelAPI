using Newtonsoft.Json;

namespace Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Finance;

public class CurrencyResponse
{
    [JsonInclude]
    public int Id { get; set; }

    [JsonInclude]
    public string FullName { get; set; }

    [JsonInclude]
    public string Name { get; set; }

    [JsonInclude]
    public int Code { get; set; }

    [JsonInclude]
    public string FullNameEng { get; set; }
}
