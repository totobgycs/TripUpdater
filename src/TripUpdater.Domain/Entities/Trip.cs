using NodaTime;
using TripUpdater.Domain.Common;
using TripUpdater.Domain.Enums;

namespace TripUpdater.Domain.Entities;

public sealed class Trip : Entity
{
    public int TripNo { get; private set; }
    public Guid LineId { get; private set; }
    public int LineNo { get; private set; }
    public Instant DepartureTime { get; private set; }
    public Instant OriginalArrivalTime { get; private set; }
    public Instant? ArrivalTime { get; private set; }
    public Status Status { get; private set; }
    public Line Line { get; private set; }

    private Trip() { Line = null!; }

    public static Trip Create(
        int tripNo,
        Instant departure,
        Instant originalArrival,
        Line line) => new()
        {
            Id = Guid.NewGuid(),
            TripNo = tripNo,
            LineId = line.Id,
            LineNo = line.LineNo,
            DepartureTime = departure,
            OriginalArrivalTime = originalArrival,
            ArrivalTime = originalArrival,
            Status = Status.Ontime,
            Line = line
        };

    public Status CalculateStatus(Instant? actualArrivalTime)
    {

        if (Status is Status.Cancelled or Status.Invalid)
        {
            return Status;
        }

        if (actualArrivalTime is null)
        {
            return Status.Cancelled;
        }

        var diff = (actualArrivalTime.Value - OriginalArrivalTime).TotalMinutes;

        return diff switch
        {
            <= -2 => Status.Early,
            >= 2 => Status.Late,
            _ => Status.Ontime
        };
    }

    public void ApplyUpdate(Instant? actualArrivalTime)
    {
        ArrivalTime = actualArrivalTime;
        Status = CalculateStatus(actualArrivalTime);
    }
}
