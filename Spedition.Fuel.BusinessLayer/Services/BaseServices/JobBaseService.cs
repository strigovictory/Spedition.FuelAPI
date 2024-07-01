using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Spedition.Fuel.BusinessLayer.Enums;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.Parsers.ParserBaseServices;
using Spedition.Fuel.Shared.DTO.ResponseModels;

namespace Spedition.Fuel.BusinessLayer.Services.BaseServices;

public abstract class JobBaseService<TParsed, TSuccess, TNotSuccess, TSearch> 
    : FuelParserBase<TParsed, TSuccess, TNotSuccess, TSearch>
    where TParsed : class, IParsedItem
{
    protected JobBaseService(
        IWebHostEnvironment env,
        FuelRepositories fuelRepositories,
        IConfiguration configuration,
        IMapper mapper,
        ICountryService countryService,
        ICurrencyService currencyService,
        IEventsTypeService eventsTypeService,
        IDivisionService divisionService,
        ITruckService truckService)
        : base(env, fuelRepositories, configuration, mapper, countryService, currencyService, eventsTypeService, divisionService, truckService, default)
    {
    }

    public abstract Task Do();
}
