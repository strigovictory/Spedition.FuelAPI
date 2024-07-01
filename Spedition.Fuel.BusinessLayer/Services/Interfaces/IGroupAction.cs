using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.Fuel.BusinessLayer.Services.Interfaces;

public interface IGroupAction<TSuccess, TNotSuccess> : INotify
{
    List<TSuccess> SuccessItems { get; }

    List<NotSuccessResponseItemDetailed<TNotSuccess>> NotSuccessItems { get; }
}
