using System.Net;

namespace Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

public class ResponseBase
{
    public ResponseBase()
    {
        Result = string.Empty;
    }

    public ResponseBase(string result)
    {
        Result = result ?? string.Empty;
    }

    [JsonInclude]
    public string Result { get; set; } = string.Empty;
}