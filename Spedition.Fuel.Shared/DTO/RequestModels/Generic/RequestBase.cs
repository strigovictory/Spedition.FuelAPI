namespace Spedition.Fuel.Shared.DTO.RequestModels.Generic;

public abstract class RequestBase
{
    public RequestBase(string userName)
    {
        UserName = userName;
    }

    [JsonInclude]
    public string UserName { get; set; }
}