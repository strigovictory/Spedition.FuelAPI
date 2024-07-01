using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.Fuel.BusinessLayer.Services.Interfaces;

public interface IGroupDoubleAction<TSuccess, TNotSuccess> : IGroupAction<TSuccess, TNotSuccess>
{
    List<TSuccess> SecondarySuccessItems { get; }
}
