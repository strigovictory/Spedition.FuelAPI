using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Moq;
using Spedition.Fuel.DataAccess.Infrastructure.Repositories.Interfaces;
using Spedition.Fuel.DataAccess.Infrastructure.Repositories;
using Spedition.Fuel.Shared.Entities;
using Spedition.Fuel.DataAccess;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;
using Spedition.Fuel.Shared.DTO.ResponseModels;
using NPOI.SS.Formula.Functions;
using Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;
using Xunit.Abstractions;
using Spedition.Fuel.Shared.Settings;

namespace Spedition.Fuel.Test;

public class TestsHelper: DbContextFactoryHelper
{
    private readonly ITestOutputHelper output;

    public TestsHelper(ITestOutputHelper output)
    {
        this.output = output;
    }

    protected ITestOutputHelper Output => output;

    protected IWebHostEnvironment Environment => GetEnvironment();

    protected IMapper Mapper => GetMapper();

    private IWebHostEnvironment GetEnvironment()
    {
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(m => m.EnvironmentName).Returns("Test");
        mockEnvironment.Setup(m => m.WebRootPath).Returns(ContentPath);
        return mockEnvironment.Object;
    }

    private IMapper GetMapper()
    {
        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new SourceMappingProfile());
        });

        return mappingConfig.CreateMapper();
    }

    protected FuelRepositories GetFuelRepositories<T>() where T : class
    {
        var services = new ServiceCollection();

        services.AddDbContext<SpeditionContext>(
            options => options.UseSqlServer(Configuration.GetConnectionString("SpeditionDb")),
            ServiceLifetime.Singleton);

        services.AddDbContextFactory<SpeditionContext>(
            options => options.UseSqlServer(Configuration.GetConnectionString("SpeditionDb")),
            ServiceLifetime.Singleton);

        var dbFactory = new DbContextFactoryHelper();

        var repository = new Repository<T>(dbFactory);

        services.AddSingleton<IRepository<T>>(prov => repository);

        var servicesProvider = services.BuildServiceProvider();

        return new FuelRepositories(servicesProvider);
    }

    protected FuelRepositories GetFuelRepositories()
    {
        var services = new ServiceCollection();

        services.AddDbContext<SpeditionContext>(
            options => options.UseSqlServer(Configuration.GetConnectionString("SpeditionDb")),
            ServiceLifetime.Singleton);

        services.AddDbContextFactory<SpeditionContext>(
            options => options.UseSqlServer(Configuration.GetConnectionString("SpeditionDb")),
            ServiceLifetime.Singleton);

        var dbFactory = new DbContextFactoryHelper();

        services.AddSingleton<IRepository<FuelCard>>(prov => new Repository<FuelCard>(dbFactory));
        services.AddSingleton<IRepository<FuelTransaction>>(prov => new Repository<FuelTransaction>(dbFactory));
        services.AddSingleton<IRepository<FuelCardsEvent>>(prov => new Repository<FuelCardsEvent>(dbFactory));
        services.AddSingleton<IRepository<NotFoundFuelCard>>(prov => new Repository<NotFoundFuelCard>(dbFactory));
        services.AddSingleton<IRepository<FuelType>>(prov => new Repository<FuelType>(dbFactory));
        services.AddSingleton<IRepository<ProvidersAccount>>(prov => new Repository<ProvidersAccount>(dbFactory));
        services.AddSingleton<IRepository<FuelCardsCountry>>(prov => new Repository<FuelCardsCountry>(dbFactory));
        services.AddSingleton<IRepository<FuelProvider>>(prov => new Repository<FuelProvider>(dbFactory));
        services.AddSingleton<IRepository<FuelCardsAlternativeNumber>>(prov => new Repository<FuelCardsAlternativeNumber>(dbFactory));

        var servicesProvider = services.BuildServiceProvider();

        return new FuelRepositories(servicesProvider);
    }
}
