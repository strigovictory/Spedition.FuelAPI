using Spedition.Fuel.Shared.Settings.Configs;

namespace Spedition.Fuel.Shared.Providers.AppConfigurationProvider;

public class ConfigBaseAuth : ConfigBase
{
    public string Login { get; set; }

    public string Password { get; set; }

    public string Key { get; set; }
}
