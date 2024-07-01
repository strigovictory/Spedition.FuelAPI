using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;

public class NotParsedTransactionFilled : NotParsedTransaction
{
    [JsonPropertyName(nameof(FuelCardIdSelected))]
    [JsonInclude]
    public int FuelCardIdSelected { get; set; }

    [JsonPropertyName(nameof(CarIdSelected))]
    [JsonInclude]
    public int CarIdSelected { get; set; }
}
