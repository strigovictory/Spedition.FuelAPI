namespace Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

public class FuelProviderResponse
{
    [JsonInclude]
    public int Id { get; set; }

    [JsonInclude]
    public string Name { get; set; }

    [JsonInclude]
    public int? CountryId { get; set; }

    public override string ToString()
    {
        return $"Провайдер «{Name ?? string.Empty}»";
    }
}
