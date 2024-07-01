using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;
using Spedition.FuelAPI.Middlewares;

namespace Spedition.FuelAPI.Helpers
{
    /// <summary>
    /// Extension methods for IApplication builder.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Enables apiKey Middleware.
        /// </summary>
        /// <param Name="builder">IApplication builder.</param>
        public static void UseApiKey(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<ApiKeyMiddleware>();
        }

        /// <summary>
        /// Enables Log Middleware.
        /// </summary>
        /// <param Name="builder">IApplication builder.</param>
        public static void UseLogging(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<LogMiddleware>();
        }

        /// <summary>
        /// Enables correlation id middleware.
        /// </summary>
        /// <param Name="builder">IApplication builder.</param>
        public static void UseCorrelationId(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<CorrelationIdMiddleware>();
        }

        public static void ConfigureSeqSerilog(this WebApplication app, IWebHostEnvironment env)
        {
            var seqHost = "http://seqevent.rgroup-cargo.com";
#if DEBUG
            seqHost = "http://localhost:5341";
#endif

            // Логгирование Serilog + Seq
            if (env.IsProduction())
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Warning()
                    .Enrich.WithExceptionDetails()
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithProcessId()
                    .Enrich.WithProperty("Application", env.ApplicationName)
                    .Enrich.WithProperty("Environment", env.EnvironmentName)
                    .WriteTo.Seq(seqHost)
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{NotifyMessage:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code).CreateLogger();
            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .Enrich.WithExceptionDetails()
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithProcessId()
                    .Enrich.WithProperty("Application", env.ApplicationName)
                    .Enrich.WithProperty("Environment", env.EnvironmentName)
                    .WriteTo.Seq(seqHost)
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{NotifyMessage:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code).CreateLogger();
            }
        }
    }
}
