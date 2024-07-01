using Spedition.Fuel.Shared.DTO.RequestModels.Generic;

namespace Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;

public class FuelReport : UploadedContent
{
    public FuelReport(int providerId, bool isMonthly, string fileName, byte[] content, string userName)
        : base(fileName, content, userName)
    {
        ProviderId = providerId;
        IsMonthly = isMonthly;
    }

    [JsonInclude]
    public int ProviderId { get; set; }

    [JsonInclude]
    public bool IsMonthly { get; set; }
}