using NodaTime;
using TripUpdater.Domain.Common;
using TripUpdater.Domain.Enums;

namespace TripUpdater.Domain.Entities;

public sealed class Trip : Entity
{
    public int TripId { get; private set; }
    public int LineNo { get; private set; }
    public Instant DepartureTime { get; private set; }
    public Instant OriginalArrivalTime { get; private set; }
    public Instant? ArrivalTime { get; private set; }
    public Status Status { get; private set; }
    public Line? Line { get; private set; }

    private Trip() { }

    public static Trip Create(
        int tripId,
        int lineNo,
        Instant departure,
        Instant originalArrival,
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

    public Status ValidateStatus(Status reportedStatus, Instant? actualArrivalTime)
    {
        if (reportedStatus is Status.Cancelled or Status.Invalid)
        {
            return reportedStatus;
        }

        if (actualArrivalTime is null)
        {
            return Status.Invalid;
        }

        var diff = (actualArrivalTime.Value - OriginalArrivalTime).TotalMinutes;

        return reportedStatus switch
        {
            Status.Ontime => Math.Abs(diff) <= 2 ? Status.Ontime : Status.Invalid,
            Status.Early => diff < -2 ? Status.Early : Status.Invalid,
            Status.Late => diff > 2 ? Status.Late : Status.Invalid,
            _ => Status.Invalid
        };
    }

    public void ApplyUpdate(Status reportedStatus, Instant? actualArrivalTime, Instant updateTimestamp)
    {
        ArrivalTime = actualArrivalTime;
        Status = ValidateStatus(reportedStatus, actualArrivalTime);
        _ = updateTimestamp;
    }
}
