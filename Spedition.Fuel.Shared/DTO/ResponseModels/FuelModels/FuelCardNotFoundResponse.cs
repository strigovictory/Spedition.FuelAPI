namespace Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

public class FuelCardNotFoundResponse
{
    [JsonInclude]
    public int Id { get; set; }

    [JsonInclude]
    public string Number { get; set; }

    [JsonInclude]
    public string UserName { get; set; }

    [JsonInclude]
    public DateTime ImportDate { get; set; }

    [JsonInclude]
    public int FuelProviderId { get; set; }

    [JsonInclude]
    public string FuelProviderName { get; set; }

    public override string ToString()
    {
        return $"Заправочная карта «{Number ?? string.Empty}»";
    }
}
