namespace Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

public class ResponseDoubleGroupActionDetailed<TSuccess, TNotSuccess> : ResponseGroupActionDetailed<TSuccess, TNotSuccess>
{
    public ResponseDoubleGroupActionDetailed()
    : base()
    {
        SecondarySuccessItems = new();
    }

    public ResponseDoubleGroupActionDetailed(string result)
        : base(result)
    {
        SecondarySuccessItems = new();
    }

    public ResponseDoubleGroupActionDetailed(
        string result,
        List<TSuccess> successItems,
        List<TSuccess> secondarySuccessItems,
        List<NotSuccessResponseItemDetailed<TNotSuccess>> notSuccessItems)
        : base(result, successItems, notSuccessItems)
    {
        SecondarySuccessItems = secondarySuccessItems;
    }

    public List<TSuccess> SecondarySuccessItems { get; }
}
