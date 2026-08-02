using NodaTime;
using TripUpdater.Domain.Common;
using TripUpdater.Domain.Enums;

namespace TripUpdater.Domain.Entities;

public sealed class UpdateLog : Entity
{
    public int TripId { get; private set; }
    public Instant UpdateTimestamp { get; private set; }
    public Status Status { get; private set; }
    public Trip Trip { get; private set; } = null!;

    private UpdateLog() { }

    public static UpdateLog Create(Instant updateTimestamp, Status status, Trip trip) => new()
    {
        TripId = trip.TripId,
        UpdateTimestamp = updateTimestamp,
        Status = status,
        Trip = trip
    };
}
