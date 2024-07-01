using Microsoft.Extensions.Options;

namespace Spedition.Fuel.Shared.Settings.Configs
{
    public class ApiConfigs : ConfigBase
    {
        public string Key { get; set; }

        public string Version { get; set; }
    }
}
