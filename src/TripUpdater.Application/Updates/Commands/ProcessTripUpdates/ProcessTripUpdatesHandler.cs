using Mediator;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TripUpdater.Application.Common.Interfaces;
using TripUpdater.Domain.Common;
using TripUpdater.Domain.Entities;
using TripUpdater.Domain.Enums;

namespace TripUpdater.Application.Updates.Commands.ProcessTripUpdates;

public sealed class ProcessTripUpdatesHandler(
    IAppDbContext db,
    TimeProvider clock) : IRequestHandler<ProcessTripUpdatesCommand, Result<UpdateSummaryDto>>
{
    public async ValueTask<Result<UpdateSummaryDto>> Handle(
        ProcessTripUpdatesCommand request,
        CancellationToken cancellationToken)
    {
        var updateTimestamp = Instant.FromDateTimeOffset(clock.GetUtcNow());
        var processedTripIds = new List<int>(request.Updates.Count);
        var counters = new Dictionary<Status, int>
        {
            [Status.Ontime] = 0,
            [Status.Early] = 0,
            [Status.Late] = 0,
            [Status.Cancelled] = 0,
            [Status.Invalid] = 0
        };

        var tripIds = request.Updates.Select(u => u.TripId).Distinct().ToList();
        var existingTrips = await db.Trips
            .Where(t => tripIds.Contains(t.TripId))
            .ToListAsync(cancellationToken);
        var tripsById = existingTrips.ToDictionary(t => t.TripId);

        var maxUpdateLogId = await db.UpdateLogs.AnyAsync(cancellationToken)
            ? await db.UpdateLogs.MaxAsync(l => l.UpdateLogId, cancellationToken)
            : 0;

        foreach (var update in request.Updates)
        {
            if (!tripsById.TryGetValue(update.TripId, out var trip))
            {
                counters[Status.Invalid]++;
                processedTripIds.Add(update.TripId);
                continue;
            }

            var actualArrival = update.ActualArrivalTime.HasValue
                ? Instant.FromDateTimeOffset(update.ActualArrivalTime.Value)
                : (Instant?)null;

            trip.ApplyUpdate(update.Status, actualArrival, updateTimestamp);
            counters[trip.Status]++;
            processedTripIds.Add(update.TripId);

            var log = UpdateLog.Create(++maxUpdateLogId, trip.TripId, updateTimestamp, trip.Status, trip);
            db.UpdateLogs.Add(log);
        }

        await db.SaveChangesAsync(cancellationToken);

        var summary = new UpdateSummaryDto(
            TotalUpdates: request.Updates.Count,
            Ontime: counters[Status.Ontime],
            Early: counters[Status.Early],
            Late: counters[Status.Late],
            Cancelled: counters[Status.Cancelled],
            Invalid: counters[Status.Invalid],
            ProcessedTripIds: processedTripIds);

        return Result<UpdateSummaryDto>.Success(summary);
    }
}
