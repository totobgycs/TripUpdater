using Microsoft.EntityFrameworkCore;
using TripUpdater.Application.Common.Interfaces;
using TripUpdater.Domain.Entities;

namespace TripUpdater.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Line> Lines => Set<Line>();
    public DbSet<UpdateLog> UpdateLogs => Set<UpdateLog>();
    public DbSet<Operator> Operators => Set<Operator>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
