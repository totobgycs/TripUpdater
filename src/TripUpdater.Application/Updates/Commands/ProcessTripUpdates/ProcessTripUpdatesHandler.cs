using Mediator;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TripUpdater.Application.Common.Interfaces;
using TripUpdater.Domain.Common;

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
        var unprocessedUpdates = new List<TripUpdateDto>();

        var tripIds = request.Updates.Select(u => u.TripId).Distinct().ToList();
        var existingTrips = await db.Trips
            .Where(t => tripIds.Contains(t.TripNo))
            .ToListAsync(cancellationToken);
        var tripsById = existingTrips.ToDictionary(t => t.TripNo);

        foreach (var update in request.Updates)
        {
            if (!tripsById.TryGetValue(update.TripId, out var trip))
            {
                unprocessedUpdates.Add(update);
                continue;
            }

            var departureTime = GetInstant(update.DepartureTime);
            var actualArrivalTime = GetInstant(update.ActualArrivalTime);

            trip.ApplyUpdate(departureTime, actualArrivalTime);
            processedTripIds.Add(update.TripId);
        }

        await db.SaveChangesAsync(cancellationToken);

        var summary = new UpdateSummaryDto(
            TotalUpdates: request.Updates.Count,
            ProcessedTripIds: processedTripIds,
            UnprocessedUpdates: unprocessedUpdates);

        return Result<UpdateSummaryDto>.Success(summary);

        static Instant? GetInstant(DateTimeOffset? dateTimeOffset)
        {
            return dateTimeOffset.HasValue
                ? Instant.FromDateTimeOffset(dateTimeOffset.Value)
                : (Instant?)null;
        }
    }
}
