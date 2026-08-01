using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TripUpdater.Application.Common.Interfaces;
using TripUpdater.Infrastructure.Persistence;

namespace TripUpdater.Infrastructure;

public static class DependencyInjection
{
    public const string DatabaseName = "TripUpdater";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        _ = configuration;

        // EF Core InMemory: the internal store is a singleton keyed by the
        // database name, so state survives across scoped requests within a run
        // even though each scope gets its own (non-thread-safe) DbContext.
        // This is the demo's "mock database": volatile (resets on restart),
        // seeded on start. Swap to a real provider for production.
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(DatabaseName));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}
