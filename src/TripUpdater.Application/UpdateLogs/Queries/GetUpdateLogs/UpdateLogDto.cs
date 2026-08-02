using TripUpdater.Domain.Enums;

namespace TripUpdater.Application.UpdateLogs.Queries.GetUpdateLogs;

public sealed record UpdateLogDto(
    Guid Id,
    int TripId,
    DateTimeOffset UpdateTimestamp,
    Status Status);
