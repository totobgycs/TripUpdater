using TripUpdater.Domain.Enums;

namespace TripUpdater.Application.UpdateLogs.Queries.GetUpdateLogs;

public sealed record UpdateLogDto(
    int UpdateLogId,
    int TripId,
    DateTimeOffset UpdateTimestamp,
    Status Status);
