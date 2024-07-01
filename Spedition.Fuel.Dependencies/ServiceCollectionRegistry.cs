using divisionsGRPC;
using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Spedition.Fuel.BusinessLayer.Configs;
using Spedition.Fuel.BusinessLayer.Models;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.Parsers;
using Spedition.Fuel.BusinessLayer.Services.Print;
using Spedition.Fuel.BusinessLayer.Services.ProvidersApi;
using Spedition.Fuel.DataAccess.Infrastructure.Repositories;
using Spedition.Fuel.Shared.DTO.RequestModels.Print;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.Entities;
using Spedition.Fuel.Shared.Interfaces;
using Spedition.Fuel.Shared.Providers.AppConfigurationProvider;
using Spedition.Fuel.Shared.Settings.Configs;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Spedition.Fuel.Dependencies
{
    /// <summary>
    /// Service collection registry.
    /// </summary>
    public static class ServiceCollectionRegistry
    {
        public static void AddHttpAccessor(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
        }

        /// <summary>
        /// Adds DbContext to service collection.
        /// </summary>
        /// <param Name="services">Service collection.</param>
        public static void AddDBContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SpeditionContext>(
                options =>
                {
                    options.EnableSensitiveDataLogging();
                    options.UseSqlServer(configuration.GetConnectionString("SpeditionDb"), builder => builder.EnableRetryOnFailure());
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                },
                ServiceLifetime.Transient);

            services.AddDbContextFactory<SpeditionContext>(
                options =>
                {
                    options.EnableSensitiveDataLogging();
                    options.UseSqlServer(configuration.GetConnectionString("SpeditionDb"), builder => builder.EnableRetryOnFailure());
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                },
                ServiceLifetime.Transient);
        }

        /// <summary>
        /// Adds services to service collection.
        /// </summary>
        /// <param Name="services">Service collection.</param>
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            // fuel-services
            services.AddTransient<IFuelTransactionService, FuelTransactionService>();
            services.AddTransient<IFuelCardsService, FuelCardsService>();
            services.AddTransient<IFuelProviderService, FuelProvidersService>();
            services.AddTransient<IFuelTypesService, FuelTypesService>();
            services.AddTransient<IFuelCardsEventsService, FuelCardsEventsService>();

            // not fuel-services
            services.AddTransient<IDivisionService, DivisionService>();
            services.AddTransient<ITruckService, TruckService>();
            services.AddTransient<ICountryService, CountryService>();
            services.AddTransient<ICurrencyService, CurrencyService>();
            services.AddTransient<IEventsTypeService, EventsTypeService>();
            services.AddTransient<IEmployeeService, EmployeeService>();

            // parsers
            services.AddScoped<ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction>, ParserAdnoc>();
            services.AddScoped<ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction>, ParserBP>();
            services.AddScoped<ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction>, ParserDiesel24>();
            services.AddScoped<ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction>, ParserGazProm>();
            services.AddScoped<ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction>, ParserHelios>();
            services.AddScoped<ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction>, ParserKamp>();
            services.AddScoped<ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction>, ParserLider>();
            services.AddScoped<ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction>, ParserUniversalScaffold>();
            services.AddScoped<ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction>, ParserDieselTrans>();

            // Other
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Print
            services.AddTransient<PrintExcelBase<IPrint>, PrintExcelTransactions<TransactionPrintRequest>> ();
            services.AddTransient<PrintExcelBase<IPrint>, PrintExcelCards<CardPrintRequest>>();
            services.AddTransient<PrintExcelBase<IPrint>, PrintExcelNotFoundCards<CardNotFoundPrintRequest>>();

            // ProvidersApi
            services.AddScoped<IProvidersApiBase<FuelTransactionShortResponse, NotParsedTransaction>, TatneftService>();
            services.AddScoped<IProvidersApiBase<FuelTransactionShortResponse, NotParsedTransaction>, E100Service>();
            services.AddScoped<IProvidersApiBase<FuelTransactionShortResponse, NotParsedTransaction>, NeftikaService>();
            services.AddScoped<IProvidersApiBase<FuelTransactionShortResponse, NotParsedTransaction>, GPNService>();
            services.AddScoped<IProvidersApiBase<FuelTransactionShortResponse, NotParsedTransaction>, RosneftService>();
            services.AddScoped<IProvidersApiBase<FuelTransactionShortResponse, NotParsedTransaction>, BPService>();

            // Configs
            // RGApi Api
            services.Configure<FinanceConfigs>(configuration.GetSection("RGApi:FinanceApi"));
            services.Configure<OfficeConfigs>(configuration.GetSection("RGApi:OfficeApi"));
            services.Configure<GeoConfigs>(configuration.GetSection("RGApi:GeoApi"));
            services.Configure<TripsConfigs>(configuration.GetSection("RGApi:TripsApi"));

            // Providers Api
            // GPN
            services.Configure<GPNConfig>(configuration.GetSection("FuelProvidersApi:GPN"));
            services.Configure<GPNConfigDemo>(configuration.GetSection("FuelProvidersApi:GPNDemo"));
            services.AddScoped<GPNService>();

            // E100
            services.Configure<E100Config>(configuration.GetSection("FuelProvidersApi:E100"));
            services.AddScoped<E100Service>();

            // Neftika
            services.Configure<NeftikaConfig>(configuration.GetSection("FuelProvidersApi:Neftika"));
            services.AddScoped<NeftikaService>();

            // Rosneft
            services.Configure<RosneftConfig>(configuration.GetSection("FuelProvidersApi:Rosneft"));
            services.AddScoped<RosneftService>();

            // Tatneft
            services.Configure<TatneftConfig>(configuration.GetSection("FuelProvidersApi:Tatneft"));
            services.AddScoped<TatneftService>();

            // BP
            services.Configure<BPConfig>(configuration.GetSection("FuelProvidersApi:BP"));
            services.AddScoped<BPService>();
        }

        /// <summary>
        /// Adds repositories to service collection.
        /// </summary>
        /// <param Name="services">Service collection.</param>
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<FuelRepositories>(); 
            services.AddTransient<FuelTransactionRepository>();
            services.AddTransient<FuelCardRepository>();
            services.AddTransient<IRepository<FuelCard>, Repository<FuelCard>>(); 
            services.AddTransient<IRepository<FuelTransaction>, Repository<FuelTransaction>>();
            services.AddTransient<IRepository<FuelCardsEvent>, Repository<FuelCardsEvent>>();
            services.AddTransient<IRepository<NotFoundFuelCard>, Repository<NotFoundFuelCard>>();
            services.AddTransient<IRepository<FuelCardsCountry>, Repository<FuelCardsCountry>>();
            services.AddTransient<IRepository<FuelProvider>, Repository<FuelProvider>>();
            services.AddTransient<IRepository<FuelType>, Repository<FuelType>>();
            services.AddTransient<IRepository<FuelCardsAlternativeNumber>, Repository<FuelCardsAlternativeNumber>>(); 
            services.AddTransient<IRepository<ProvidersAccount>, Repository<ProvidersAccount>>();
            services.AddTransient<IRepository<BPTransaction>, Repository<BPTransaction>>();
        }

        /// <summary>
        /// Adds database configs to service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        public static void AddDatabaseConfigs(this IServiceCollection services, IConfiguration configuration)
        {
            // todo: services.Configure<DatabaseConfigs>(configuration.GetSection(nameof(DatabaseConfigs)));
            services.AddSingleton<IDatabaseConfigsProvider, DatabaseConfigsProvider>();
        }

        public static void AddApiVersion(this IServiceCollection services, IConfigurationSection apiConfigSection)
        {
            services.AddApiVersioning(opt =>
            {
                var version = new Version(apiConfigSection[nameof(Version)]);
                opt.DefaultApiVersion = new ApiVersion(version.Major, version.Minor);
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.ReportApiVersions = true;
            });

            services.AddVersionedApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });

            services.Configure<ApiConfigs>(apiConfigSection);
        }

        public static void AddSwaggerWithApiKeySecurity(this IServiceCollection services)
        {
            const string ApiKeyHeaderName = "x-api-key";

            services.AddSwaggerGen(setup =>
            {
                setup.AddSecurityDefinition(ApiKeyHeaderName, new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = ApiKeyHeaderName,
                    Type = SecuritySchemeType.ApiKey,
                });

                setup.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Name = ApiKeyHeaderName,
                            Type = SecuritySchemeType.ApiKey,
                            In = ParameterLocation.Header,
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = ApiKeyHeaderName,
                            },
                        },
                        new List<string>()
                    },
                });
            });
        }

        public static void AddRetrievers(this IServiceCollection services, IConfigurationSection fuelConfig)
        {
            // Retrievers
            services.AddTransient<IDivisionRetriever, DivisionRetriever>();
            services.AddTransient<ITruckRetriever, TruckRetriever>();
            services.AddTransient<ICountryRetriever, CountryRetriever>();
            services.AddTransient<ICurrencyRetriever, CurrencyRetriever>();
            services.AddTransient<IEventsTypesRetriever, EventsTypesRetriever>();
            services.AddTransient<IEmployeeRetriever, EmployeeRetriever>();

            services.AddGRPC(fuelConfig);
        }

        private static void AddGRPC(this IServiceCollection services, IConfigurationSection fuelConfig)
        {
            services.AddGrpcClient<DivisionsGRPCService.DivisionsGRPCServiceClient>((services, options) =>
            {
                options.Address = new Uri(fuelConfig["BaseUrl"]);
                options.ChannelOptionsActions.Add((opt) =>
                {
                    opt.MaxReceiveMessageSize = 10 * 1024 * 1024 * 10; //  Если задано значение null, то размер сообщения не ограничен.
                    opt.MaxSendMessageSize = 10 * 1024 * 1024 * 10; //  Если задано значение null, то размер сообщения не ограничен.
                });
            })
                .ConfigureChannel(ch =>
                {
                    ch.ServiceConfig = new ServiceConfig()
                    {
                        MethodConfigs =
                        {
                                        new MethodConfig
                                        {
                                            Names = { MethodName.Default },
                                            RetryPolicy = new RetryPolicy
                                            {
                                                MaxAttempts = 5,
                                                InitialBackoff = TimeSpan.FromSeconds(1),
                                                MaxBackoff = TimeSpan.FromSeconds(5),
                                                BackoffMultiplier = 1.5,
                                                RetryableStatusCodes = { StatusCode.Unavailable },
                                            },
                                        },
                        },
                    };
                })
                .EnableCallContextPropagation(o => o.SuppressContextNotFoundErrors = true);
        }
    }
}
