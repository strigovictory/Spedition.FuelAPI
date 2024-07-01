using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.Helpers;
using Spedition.Fuel.Shared.Settings.Configs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Spedition.Fuel.Shared.Providers.AppConfigurationProvider;

public class AppConfigurationProvider : ConfigurationProvider
{
    private static string[] StringConfigs { get; set; }

    // This method gets called each time a value is requested from the ConfigurationProvider.
    public override bool TryGet(string key, out string value)
    {
        if (Data.TryGetValue(key, out value))
        {
            return true;
        }

        return false;
    }

    // This method is optional, but is used to load the configuration provider with data from a source.
    public override void Load()
    {
        // The Load event can house custom code to load data from a source, such as reading a file or a database.
        var assemblyDir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(FuelCardRequest)).Location);
        var configFilePath = string.Empty;
#if DEBUG
        configFilePath = Path.Combine(assemblyDir, "appsettings.Development.json");
#else
        configFilePath = Path.Combine(assemblyDir, "appsettings.json");
#endif

        var fileLenth = new FileInfo(configFilePath).Length;

        if (File.Exists(configFilePath) && fileLenth > 0)
        {
            using FileStream fs = new(configFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using BinaryReader binaryReader = new(fs);
            var content = binaryReader.ReadBytes((int)fileLenth);
            StringConfigs = Encoding.Default.GetString(content).Split('\n');

            // 1 - ApiConfigs
            SetApiConfigs();

            // 2 - ConnectionStrings
            SetConnectionStrings();

            // 3 - RGApi
            SetRGApi();

            // 4 - GPN
            SetConfigBase("GPN");

            // 4 - GPNDemo
            SetGPNDemo();

            // 5 - E100
            SetConfigBase("E100", 1);

            // 6 - BP
            SetConfigBase("BP", 1);

            // 7 - Neftika
            SetConfigBase("Neftika");

            // 8 - Rosneft
            SetConfigBase("Rosneft");

            // 9 - Tatneft
            SetConfigBase("Tatneft");

            // 10 - GeoService
            SetApiConfigs("GeoApi");

            // 11 - FinanceService
            SetApiConfigs("FinanceApi");
        }
    }

    private void SetApiConfigs()
    {
        var apiConfigs = GetValue<ApiConfigs>(nameof(ApiConfigs), 4);
        Data.Add(nameof(apiConfigs.Name), apiConfigs.Name);
        Data.Add($"{nameof(ApiConfigs)}:{nameof(apiConfigs.Name)}", apiConfigs.Name);
        Data.Add(nameof(apiConfigs.BaseUrl), apiConfigs.BaseUrl);
        Data.Add($"{nameof(ApiConfigs)}:{nameof(apiConfigs.BaseUrl)}", apiConfigs.BaseUrl);
        Data.Add(nameof(apiConfigs.Key), apiConfigs.Key);
        Data.Add($"{nameof(ApiConfigs)}:{nameof(apiConfigs.Key)}", apiConfigs.Key);
        Data.Add(nameof(apiConfigs.Version), apiConfigs.Version);
        Data.Add($"{nameof(ApiConfigs)}:{nameof(apiConfigs.Version)}", apiConfigs.Version);
    }

    private void SetConnectionStrings()
    {
        var connectionString = GetValue<ConnectionStrings>(nameof(ConnectionStrings), 1);
        Data.Add(nameof(connectionString.SpeditionDb), connectionString.SpeditionDb);
        Data.Add($"{nameof(ConnectionStrings)}:{nameof(connectionString.SpeditionDb)}", connectionString.SpeditionDb);
    }

    private void SetRGApi()
    {
        var rGApi = GetValue<RGApi>(nameof(RGApi), 1);
        Data.Add(nameof(rGApi.OfficeApi), rGApi.OfficeApi);
        Data.Add($"{nameof(RGApi)}:{nameof(rGApi.OfficeApi)}", rGApi.OfficeApi);
    }

    private void SetGPNDemo()
    {
        var gpnDemo = GetValue<ConfigBaseAuth>("GPNDemo", 5);
        Data.Add($"GPNDemo:{nameof(gpnDemo.Name)}", gpnDemo.Name);
        Data.Add($"GPNDemo:{nameof(gpnDemo.BaseUrl)}", gpnDemo.BaseUrl);
        Data.Add($"GPNDemo:{nameof(gpnDemo.Login)}", gpnDemo.Login);
        Data.Add($"GPNDemo:{nameof(gpnDemo.Password)}", gpnDemo.Password);
        Data.Add($"GPNDemo:{nameof(gpnDemo.Key)}", gpnDemo.Key);
    }

    private void SetConfigBase(string name, int rowsNum = 2)
    {
        var item = GetValue<ConfigBase>(name, rowsNum);
        Data.Add($"{name}:{nameof(item.Name)}", item.Name);
        Data.Add($"{name}:{nameof(item.BaseUrl)}", item.BaseUrl);
    }

    private void SetApiConfigs(string apiName)
    {
        var apiConfigs = GetValue<ApiConfigs>(apiName, 4);
        Data.Add($"{apiName}:{nameof(apiConfigs.Name)}", apiConfigs.Name);
        Data.Add($"{apiName}:{nameof(apiConfigs.BaseUrl)}", apiConfigs.BaseUrl);
        Data.Add($"{apiName}:{nameof(apiConfigs.Key)}", apiConfigs.Key);
        Data.Add($"{apiName}:{nameof(apiConfigs.Version)}", apiConfigs.Version);
    }

    private T GetValue<T>(string name, int rowsNum)
    {
        var foundConfigs = new List<string>();
        for (int i = 0; i < StringConfigs.Length; i++)
        {
            if (StringConfigs[i].Contains(name))
            {
                var rowNum = i + 1;
                for (int j = rowNum; j < rowNum + rowsNum; j++)
                {
                    var row = StringConfigs[j];
                    foundConfigs.Add(row);
                }

                break;
            }
        }

        var foundConfigsStr = string.Empty;
        foundConfigs?.ForEach(row => foundConfigsStr += row);
        foundConfigsStr = foundConfigsStr.Replace("\r", string.Empty);
        foundConfigsStr = "{" + foundConfigsStr + "}";
        T result = default;
        try
        {
            result = foundConfigsStr.Length > 2 ? JsonConvert.DeserializeObject<T>(foundConfigsStr) : default;
        }
        catch (Exception exc)
        {
            exc.LogError(GetType().Name, nameof(GetValue));
            throw;
        }

        return result;
    }
}
