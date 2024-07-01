namespace Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

public class FuelCardsAlternativeNumberResponse : EntityModifiedResponse, IComparable
{
    [JsonInclude]
    public int Id { get; set; }

    [JsonInclude]
    public string Number { get; set; }

    [JsonInclude]
    public int CardId { get; set; }

    public override string ToString()
    {
        return $"Альтернативный номер заправочной карты «{Number ?? string.Empty}»";
    }

    public int CompareTo(object obj)
    {
        return obj is FuelCardsAlternativeNumberResponse item
            && item.Id == Id
            && item.Number == Number
            && item.CardId == CardId
            ? 0
            : -1;
    }
}
