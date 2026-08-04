using TripUpdater.Domain.Enums;

namespace TripUpdater.Application.Trips.Queries.GetTrips;

public sealed record TripDto(
    int TripId,
    long LineId,
    int LineNo,
    DateTimeOffset DepartureTime,
    DateTimeOffset OriginalArrivalTime,
    DateTimeOffset? ArrivalTime,
    Status Status);
