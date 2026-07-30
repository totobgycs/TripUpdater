using TripUpdater.Domain.Common;
using TripUpdater.Domain.Enums;

namespace TripUpdater.Domain.Entities;

public sealed class UpdateLog : Entity
{
    public int UpdateLogId { get; private set; }
    public int TripId { get; private set; }
    public DateTimeOffset UpdateTimestamp { get; private set; }
    public Status Status { get; private set; }
    public Trip Trip { get; private set; } = null!;

    private UpdateLog() { }

    public static UpdateLog Create(int updateLogId, int tripId, DateTimeOffset updateTimestamp, Status status, Trip trip) => new()
    {
        Id = Guid.NewGuid(),
        UpdateLogId = updateLogId,
        TripId = tripId,
        UpdateTimestamp = updateTimestamp,
        Status = status,
        Trip = trip
    };
}
