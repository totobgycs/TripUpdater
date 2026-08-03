using TripUpdater.Domain.Enums;

namespace TripUpdater.Application.UpdateLogs.Queries.GetUpdateLogs;

public sealed record UpdateLogDto(
    Guid Id,
    Guid TripId,
    int TripNo,
    DateTimeOffset UpdateTimestamp,
    Status Status);
