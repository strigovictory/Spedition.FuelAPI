namespace Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

public class ResponseGroupActionDetailed<TSuccess, TNotSuccess> : ResponseBase
{
    public ResponseGroupActionDetailed()
        : base()
    {
        SuccessItems = new ();
        NotSuccessItems = new ();
    }

    public ResponseGroupActionDetailed(string result)
        : base(result)
    {
        SuccessItems = new ();
        NotSuccessItems = new ();
    }

    public ResponseGroupActionDetailed(
        string result,
        List<TSuccess> successItems,
        List<NotSuccessResponseItemDetailed<TNotSuccess>> notSuccessItems)
        : base(result)
    {
        SuccessItems = successItems;
        NotSuccessItems = notSuccessItems;
    }

    /// <summary>
    /// Коллекция экземпляров, над которыми операция была успешно произведена.
    /// </summary>
    [JsonInclude]
    public List<TSuccess> SuccessItems { get; set; } = new ();

    /// <summary>
    /// Коллекция-словарь экземпляров, над которыми операция не была произведена.
    /// </summary>
    [JsonInclude]
    public List<NotSuccessResponseItemDetailed<TNotSuccess>> NotSuccessItems { get; set; } = new ();
}
