using Spedition.Fuel.Shared.DTO.RequestModels.Generic;

namespace Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

public class RequestGroupAction<T> : RequestBase
{
    public RequestGroupAction(string userName, List<T> items)
        : base(userName)
    {
        Items = items;
    }

    /// <summary>
    /// Коллекция экземпляров, над которыми нужно произвести групповую операцию.
    /// </summary>
    [JsonInclude]
    public List<T> Items { get; set; }
}
