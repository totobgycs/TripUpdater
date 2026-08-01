using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TripUpdater.Infrastructure;
using TripUpdater.Infrastructure.Persistence;

namespace TripUpdater.Tests.Fixtures;

public sealed class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Unique in-memory database name per fixture instance — the InMemory store
    // is keyed by name and shared across all uses of that name within the process,
    // so a unique name guarantees test isolation.
    private readonly string _dbName = $"TripUpdater-Test-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the production AppDbContext registration so we can replace
            // the in-memory database name with a unique one per fixture.
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }

    public async ValueTask InitializeAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        SeedData.Seed(db);
    }

    // No file cleanup needed — the InMemory store is GC'd with the fixture.
}
