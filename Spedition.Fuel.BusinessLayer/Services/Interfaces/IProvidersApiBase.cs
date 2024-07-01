namespace Spedition.Fuel.BusinessLayer.Services.Interfaces;

public interface IProvidersApiBase<TSuccess, TNotSuccess> : IJobBaseService<TSuccess, TNotSuccess>
{
    string ProviderName { get; }
}
