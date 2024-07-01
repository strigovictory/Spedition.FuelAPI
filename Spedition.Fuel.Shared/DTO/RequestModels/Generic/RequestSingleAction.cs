using Spedition.Fuel.Shared.DTO.RequestModels.Generic;

namespace Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

public class RequestSingleAction<T> : RequestBase
{
    public RequestSingleAction(string userName, T item)
        : base(userName)
    {
        Item = item;
    }

    /// <summary>
    /// Экземпляр, над которым нужно произвести операцию.
    /// </summary>
    [JsonInclude]
    public T Item { get; set; }
}
