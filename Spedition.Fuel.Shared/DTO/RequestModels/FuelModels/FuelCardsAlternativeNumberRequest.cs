using Spedition.Fuel.Shared.DTO.ResponseModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;

[AutoMap(typeof(FuelCardsAlternativeNumberResponse), ReverseMap = true)]
    public class FuelCardsAlternativeNumberRequest : EntityModifiedRequest
    {
        [JsonInclude]
        public int Id { get; set; }

        [JsonInclude]
        public string Number { get; set; }

        [JsonInclude]
        public int CardId { get; set; }
    }