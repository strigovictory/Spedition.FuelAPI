namespace Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

public class ResponseSingleAction<T> : ResponseBase
{
    public ResponseSingleAction()
        : base()
    {
        Item = default;
    }

    public ResponseSingleAction(T item, string message)
        : base(message)
    {
        Item = item;
    }

    [JsonInclude]
    public T Item { get; set; }
}
