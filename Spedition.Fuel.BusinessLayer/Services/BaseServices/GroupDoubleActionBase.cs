using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Spedition.Fuel.BusinessLayer.Services.Interfaces;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.Fuel.BusinessLayer.Services.BaseServices;

public abstract class GroupDoubleActionBase<TSuccess, TNotSuccess, TSearch> : 
    GroupActionBase<TSuccess, TNotSuccess, TSearch>, IGroupDoubleAction<TSuccess, TNotSuccess>
{
    protected GroupDoubleActionBase(
        IWebHostEnvironment env,
        IConfiguration configuration,
        IMapper mapper)
        : base(env, configuration, mapper)
    {
    }

    public List<TSuccess> SecondarySuccessItems { get; protected set; } = new();

    protected override List<NotSuccessResponseItemDetailed<TNotSuccess>> GetNotSuccessItems()
    {
        return notSuccessItems;
    }
}
