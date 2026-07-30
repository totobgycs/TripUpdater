using TripUpdater.Domain.Common;
using TripUpdater.Domain.Enums;

namespace TripUpdater.Domain.Entities;

public sealed class Trip : Entity
{
    public int TripId { get; private set; }
    public int LineNo { get; private set; }
    public DateTimeOffset DepartureTime { get; private set; }
    public DateTimeOffset OriginalArrivalTime { get; private set; }
    public DateTimeOffset? ArrivalTime { get; private set; }
    public Status Status { get; private set; }
    public Line? Line { get; private set; }

    private Trip() { }

    public static Trip Create(
        int tripId,
        int lineNo,
        DateTimeOffset departure,
        DateTimeOffset originalArrival,
        Line? line = null) => new()
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            LineNo = lineNo,
            DepartureTime = departure,
            OriginalArrivalTime = originalArrival,
            ArrivalTime = null,
            Status = Status.Ontime,
            Line = line
        };

    public Status CalculateStatus(DateTimeOffset? actualArrivalTime)
    {
        if (actualArrivalTime is null)
        {
            return Status.Cancelled;
        }

        if (actualArrivalTime < DepartureTime)
        {
            return Status.Invalid;
        }

        var diff = (actualArrivalTime.Value - OriginalArrivalTime).TotalMinutes;

        return Math.Abs(diff) switch
        {
            <= 2 => Status.Ontime,
            _ => diff < -2 ? Status.Early : Status.Late
        };
    }

    public void ApplyUpdate(DateTimeOffset? actualArrivalTime, DateTimeOffset updateTimestamp)
    {
        ArrivalTime = actualArrivalTime;
        Status = CalculateStatus(actualArrivalTime);
        _ = updateTimestamp;
    }
}
