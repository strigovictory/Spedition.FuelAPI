using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Spedition.Fuel.BusinessLayer.Configs;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.BaseServices;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.Settings.Configs;

namespace Spedition.Fuel.BusinessLayer.Services.ProvidersApi;

public abstract class ProvidersApiBase<TParsed>
    : JobBaseService<TParsed, FuelTransactionShortResponse, NotParsedTransaction, FuelTransaction>,
    IProvidersApiBase<FuelTransactionShortResponse, NotParsedTransaction>, IPeriodic
    where TParsed : class, IParsedItem
{
    private readonly IOptions<ConfigBase> options;

    public ProvidersApiBase(
        IWebHostEnvironment env,
        FuelRepositories fuelRepositories,
        IConfiguration configuration,
        IMapper mapper,
        ICountryService countryService,
        ICurrencyService currencyService,
        IEventsTypeService eventsTypeService,
        IDivisionService divisionService,
        ITruckService truckService,
        IOptions<ConfigBase> options)
        : base(env, fuelRepositories, configuration, mapper, countryService, currencyService, eventsTypeService, divisionService, truckService)
    {
        this.options = options;
    }

    public int? Periodicity { get; set; }

    public override List<int> ProvidersId => new List<int> { ProviderId };

    public IOptions<ConfigBase> Options => options;

    public string ProviderName => Options?.Value?.Name ?? string.Empty;

    protected List<ProvidersAccount> ProvidersAccounts { get; private set; }

    protected async Task InitProvidersAccounts()
    {
        ProvidersAccounts = (await fuelRepositories?.ProvidersAccounts?.GetAsync())?
            .Where(account => account.ProviderId == ProviderId)?.ToList() ?? new();
    }

    public abstract Task<bool> GetTransactions();

    public override async Task Do()
    {
        var startTime = DateTime.Now;
        ServiceStart();

        try
        {
            //Log.Error($"Start method «{nameof(GetTransactions)}» {DateTime.Now.ToString("HH:mm:ss")}");
            if (await GetTransactions())
            {
                //Log.Error($"Start method «{nameof(MappingParsedToDB)}» {DateTime.Now.ToString("HH:mm:ss")}");
                await MappingParsedToDB();

                //Log.Error($"Start method «{nameof(SaveItemsChanges)}» {DateTime.Now.ToString("HH:mm:ss")}");
                await SaveItemsChanges();
            }
        }
        catch(Exception exc)
        {
            exc.LogError(GetType().Name, nameof(Do));
            throw;
        }
        finally
        {
            ServiceFinish();
        }
    }
}
