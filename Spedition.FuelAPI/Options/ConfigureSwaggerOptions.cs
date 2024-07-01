using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Spedition.Fuel.Shared.Providers.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Spedition.FuelAPI.Options
{
    /// <summary>
    /// Configure Swagger generator options.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private const string ApiName = "Spedition.Fuel.API";

        private readonly IApiVersionDescriptionProvider provider;

        private readonly string apiVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
        /// </summary>
        /// <param Name="provider">API version information.</param>
        /// <param Name="apiVersionProvider">Api version provider.</param>
        public ConfigureSwaggerOptions(
            IApiVersionDescriptionProvider provider,
            IFuelApiConfigurationProvider apiVersionProvider)
        {
            this.provider = provider;
            apiVersion = apiVersionProvider.GetApiVersion();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add documents to the swagger generator options.
        /// </summary>
        /// <param Name="options">Swagger generator options.</param>
        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    description.GroupName,
                    new OpenApiInfo
                    {
                        Title = ApiName,
                        Version = apiVersion,
                    });

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var filePath = xmlFilename != null ? Path.Combine(AppContext.BaseDirectory, xmlFilename) : string.Empty;
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    options.IncludeXmlComments(filePath);
            }
        }
    }
}
