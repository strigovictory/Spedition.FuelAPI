using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spedition.Fuel.Dependencies;
using Spedition.Fuel.Shared.Helpers;
using Spedition.Fuel.Shared.Providers;
using Spedition.Fuel.Shared.Providers.Interfaces;
using Spedition.Fuel.Shared.Settings.Configs;
using Spedition.FuelAPI.Accessors;
using Spedition.FuelAPI.Accessors.Interfaces;
using Spedition.FuelAPI.Filters;
using Spedition.FuelAPI.Helpers;
using Spedition.FuelAPI.Options;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");

builder.Services.AddHttpAccessor();

AddCorrelationId(builder.Services);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddHealthChecks();

builder.Services.AddRequestDecompression();

builder.Host.UseSerilog((ctx, lc) => lc
       .WriteTo.Console()
       .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.PostConfigure<ApiBehaviorOptions>(options =>
{
    var factory = options.InvalidModelStateResponseFactory;

    options.InvalidModelStateResponseFactory = context =>
    {
        Log.Warning("Invalid parameters");
        return factory(context);
    };
});

builder.Services.AddDBContext(builder.Configuration);

builder.Services.AddDatabaseConfigs(builder.Configuration);

builder.Services.AddRepositories();

builder.Services.AddServices(builder.Configuration);

builder.Services.AddRetrievers(builder.Configuration.GetSection("RGApi:OfficeApi"));

builder.Services.AddControllers(options => { options.Filters.Add<HttpResponseExceptionFilterAttribute>(); })
    .AddApplicationPart(typeof(Program).Assembly)
    .AddJsonOptions(jsonOption => jsonOption.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddRouting();

builder.Services.AddApiVersion(builder.Configuration.GetSection(nameof(ApiConfigs)));

builder.Services.AddSingleton<IFuelApiConfigurationProvider, FuelApiConfigurationProvider>();

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

builder.Services.AddSwaggerWithApiKeySecurity();

var app = builder.Build();

app.UseCorrelationId();

app.UseLogging();

app.ConfigureSeqSerilog(builder.Environment);

app.UseSerilogRequestLogging();

if (!app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(opt =>
    {
        foreach (var description in app.Services.GetRequiredService<IApiVersionDescriptionProvider>().ApiVersionDescriptions)
        {
            opt.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });
}

app.UseHttpsRedirection();

app.UseApiKey();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHealthChecks("/health");
});

app.Run();

static void AddCorrelationId(IServiceCollection services) => services.AddTransient<ICorrelationIdAccessor, CorrelationIdAccessor>();
