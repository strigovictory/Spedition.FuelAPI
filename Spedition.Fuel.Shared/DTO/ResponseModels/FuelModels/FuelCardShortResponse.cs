namespace Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

public class FuelCardShortResponse : IComparable
{
    [JsonInclude]
    public int Id { get; set; }

    [JsonInclude]
    public string Number { get; set; } = string.Empty;

    public int CompareTo(object obj)
    {
        return obj is FuelCardShortResponse source
            && source.Id == Id
            && source.Number == Number ? 0 : -1;
    }

    public override string ToString()
    {
        return $"Топливная карта с номером «{Number ?? string.Empty}» ";
    }
}
