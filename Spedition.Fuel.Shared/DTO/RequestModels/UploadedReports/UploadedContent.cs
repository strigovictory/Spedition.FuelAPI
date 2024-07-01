using Spedition.Fuel.Shared.DTO.RequestModels.Generic;

namespace Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;

public class UploadedContent : RequestBase
{
    public UploadedContent(string fileName, byte[] content, string userName)
        : base(userName)
    {
        FileName = fileName;
        Content = content;
    }

    [JsonInclude]
    public string FileName { get; }

    [JsonInclude]
    public byte[] Content { get; }
}
