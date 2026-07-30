using Microsoft.EntityFrameworkCore;
using TripUpdater.Domain.Entities;

namespace TripUpdater.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Trip> Trips { get; }
    DbSet<Line> Lines { get; }
    DbSet<UpdateLog> UpdateLogs { get; }
    DbSet<Operator> Operators { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
