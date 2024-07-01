using Microsoft.Extensions.Configuration;
using IConfigurationProvider = Microsoft.Extensions.Configuration.IConfigurationProvider;

namespace Spedition.Fuel.Shared.Providers.AppConfigurationProvider;

public class AppConfigurationSource : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new AppConfigurationProvider();
    }
}
