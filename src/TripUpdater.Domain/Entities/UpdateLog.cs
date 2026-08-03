using NodaTime;
using TripUpdater.Domain.Common;
using TripUpdater.Domain.Enums;

namespace TripUpdater.Domain.Entities;

public sealed class UpdateLog : Entity
{
    public Guid TripId { get; private set; }
    public int TripNo { get; private set; }
    public Instant UpdateTimestamp { get; private set; }
    public Status Status { get; private set; }
    public Trip Trip { get; private set; } = null!;

    private UpdateLog() { }

    public static UpdateLog Create(Guid tripId, int tripNo, Instant updateTimestamp, Status status, Trip trip) => new()
    {
        Id = Guid.NewGuid(),
        TripId = tripId,
        TripNo = tripNo,
        UpdateTimestamp = updateTimestamp,
        Status = status,
        Trip = trip
    };
}
