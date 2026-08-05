using NodaTime;
using TripUpdater.Domain.Common;
using TripUpdater.Domain.Enums;

namespace TripUpdater.Domain.Entities;

public sealed class UpdateLog : Entity
{
    public long TripId { get; private set; }
    public int TripNo { get; private set; }
    public Instant UpdateTimestamp { get; private set; }
    public Status PreviousStatus { get; private set; }
    public Instant PreviousDepartureTime { get; private set; }
    public Instant? PreviousArrivalTime { get; private set; }
    public Trip Trip { get; private set; } = null!;

    private UpdateLog() { }

    public static UpdateLog Create(Instant updateTimestamp, Trip trip) => new()
    {
        TripId = trip.Id,
        TripNo = trip.TripNo,
        UpdateTimestamp = updateTimestamp,
        PreviousStatus = trip.Status,
        PreviousArrivalTime = trip.ArrivalTime,
        PreviousDepartureTime = trip.DepartureTime,
        Trip = trip
    };
}
