using Spedition.Fuel.Shared.DTO.RequestModels.Generic;

namespace Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;

public class FuelParameters : RequestBase
{
    public FuelParameters(int providerId, string userName)
    : base(userName)
    {
        ProviderId = providerId;
    }

    [JsonInclude]
    public int ProviderId { get; set; }

    [JsonInclude]
    public bool IsMonthly { get; set; }
}
