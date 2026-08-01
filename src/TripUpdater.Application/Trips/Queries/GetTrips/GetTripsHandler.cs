using Mediator;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TripUpdater.Application.Common.Interfaces;
using TripUpdater.Domain.Common;

namespace TripUpdater.Application.Trips.Queries.GetTrips;

public sealed class GetTripsHandler(IAppDbContext db) : IRequestHandler<GetTripsQuery, Result<List<TripDto>>>
{
    public async ValueTask<Result<List<TripDto>>> Handle(GetTripsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.Trip> query = db.Trips.AsNoTracking();

        if (request.LineNo is not null)
        {
            query = query.Where(t => t.LineNo == request.LineNo);
        }

        if (request.From is not null)
        {
            var fromInstant = Instant.FromDateTimeOffset(request.From.Value);
            query = query.Where(t => t.DepartureTime >= fromInstant);
        }

        if (request.To is not null)
        {
            var toInstant = Instant.FromDateTimeOffset(request.To.Value);
            query = query.Where(t => t.DepartureTime <= toInstant);
        }

        var trips = await query
            .OrderBy(t => t.DepartureTime)
            .Select(t => new TripDto(
                t.TripId,
                t.LineNo,
                t.DepartureTime.ToDateTimeOffset(),
                t.OriginalArrivalTime.ToDateTimeOffset(),
                t.ArrivalTime.HasValue ? t.ArrivalTime.Value.ToDateTimeOffset() : (DateTimeOffset?)null,
                t.Status))
            .ToListAsync(cancellationToken);

        return Result<List<TripDto>>.Success(trips);
    }
}
