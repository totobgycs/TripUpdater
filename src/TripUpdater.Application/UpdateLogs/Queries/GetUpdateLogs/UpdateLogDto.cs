using TripUpdater.Domain.Enums;

namespace TripUpdater.Application.UpdateLogs.Queries.GetUpdateLogs;

public sealed record UpdateLogDto(
    long Id,
    long TripId,
    int TripNo,
    DateTimeOffset UpdateTimestamp,
    DateTimeOffset PreviousDepartureTime,
    DateTimeOffset? PreviousArrivalTime,
    Status PreviousStatus);
