namespace Spedition.Fuel.BusinessLayer.Services.BaseServices;

public abstract class NotifyBase : TimeTracker, INotify
{
    public string NotifyMessage { get; protected set; } = string.Empty;
}
