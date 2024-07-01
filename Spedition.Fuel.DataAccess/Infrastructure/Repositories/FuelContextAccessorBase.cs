using Spedition.Fuel.Shared.Settings;

namespace Spedition.Fuel.DataAccess.Infrastructure.Repositories;

public class FuelContextAccessorBase
{
    protected readonly SpeditionContext context;
    protected string notifyMessage = string.Empty;

    public FuelContextAccessorBase(SpeditionContext context)
    {
        this.context = context;
    }

    public virtual string NotifyMessage => notifyMessage ?? string.Empty;
}
