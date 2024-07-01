using Spedition.Fuel.Shared.Settings.Configs;

namespace Spedition.Fuel.Client.Infrastructure.Interfaces;

public interface IApiConfigsProvider
{
    ApiConfigs GetApiConfigs();
}
