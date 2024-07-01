namespace Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

public class NotSuccessResponseItemDetailed<T>
{
    public NotSuccessResponseItemDetailed()
    {
    }

    public NotSuccessResponseItemDetailed(T item, string reason)
    {
        NotSuccessItem = item;
        Reason = reason ?? string.Empty;
    }

    [JsonInclude]
    public T NotSuccessItem { get; set; }

    [JsonInclude]
    public string Reason { get; set; } = string.Empty;
}
