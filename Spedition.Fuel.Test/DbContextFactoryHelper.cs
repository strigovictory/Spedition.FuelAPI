using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Spedition.Fuel.Shared.Settings;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test;

public class DbContextFactoryHelper : FilesHelper, IDbContextFactory<SpeditionContext>
{
    protected IConfigurationRoot Configuration =>
        new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false)
        .Build();

    protected IConfigurationRoot ConfigurationDevelop =>
        new ConfigurationBuilder()
        .AddJsonFile("appsettings.Development.json", optional: false)
        .Build();

    private string ConnectionString => Configuration.GetConnectionString("SpeditionDb");

    public SpeditionContext CreateDbContext()
    {
        return CreateApplicationDbContext();
    }

    public async Task<SpeditionContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(CreateDbContext());
    }

    private SpeditionContext CreateApplicationDbContext()
    {
        return new SpeditionContext(
            new DbContextOptionsBuilder<SpeditionContext>()
           .UseSqlServer(Configuration.GetConnectionString("SpeditionDb"), builder => builder.EnableRetryOnFailure())
           .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
           .Options);
    }
}
