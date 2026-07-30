using TripUpdater.Domain.Common;

namespace TripUpdater.Domain.Entities;

public sealed class Line : Entity
{
    public int LineId { get; private set; }
    public string OperatorNo { get; private set; } = null!;
    public string LinePlanningNumber { get; private set; } = null!;
    public Operator Operator { get; private set; } = null!;

    private Line() { }

    public static Line Create(int lineId, string operatorNo, string linePlanningNumber, Operator? @operator = null) => new()
    {
        Id = Guid.NewGuid(),
        LineId = lineId,
        OperatorNo = operatorNo,
        LinePlanningNumber = linePlanningNumber,
        Operator = @operator!
    };
}
