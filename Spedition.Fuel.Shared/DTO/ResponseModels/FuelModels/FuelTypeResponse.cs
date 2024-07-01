namespace Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

[AutoMap(typeof(FuelType), ReverseMap = true)]
public class FuelTypeResponse
{
    [JsonInclude]
    public int Id { get; set; }

    [JsonInclude]
    public string Name { get; set; }

    public override string ToString()
    {
        return $"Тип услуги «{Name ?? string.Empty}»";
    }
}
