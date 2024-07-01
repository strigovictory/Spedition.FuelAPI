using Spedition.Fuel.Shared.Helpers;

namespace Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

[AutoMap(typeof(FuelTransaction), ReverseMap = true)]
public class NotParsedTransaction : FuelTransactionResponse
{
    [JsonInclude]
    public string CardNumber { get; set; } = string.Empty; // если номер топливной карты - самостоятельный идентификатор

    [JsonInclude]
    public string CarNumber { get; set; } = string.Empty; // если номер топливной карты соответсвует номеру тягача

    [JsonInclude]
    public string NotFuelType { get; set; } = string.Empty; // если услуга не относится к топливу - соответсвует наименованию услуги

    public override string ToString()
    {
        var message = !string.IsNullOrEmpty(CarNumber)
            ? $"Рег.номер авто «{CarNumber}» "
            : !string.IsNullOrEmpty(CardNumber)
            ? $"заправочная карта № «{CardNumber}» "
            : !string.IsNullOrEmpty(CardNumber)
            ? $"не учитываемая услуга «{NotFuelType}» "
            : string.Empty;

        return $"{base.ToString()} {message}";
    }
}