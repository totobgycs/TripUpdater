using TripUpdater.Domain.Common;

namespace TripUpdater.Domain.Entities;

public sealed class Line : Entity
{
    public int LineNo { get; private set; }
    public long OperatorId { get; private set; }
    public string OperatorNo { get; private set; } = null!;
    public string LinePlanningNumber { get; private set; } = null!;
    public Operator? Operator { get; private set; }

    private Line() { }

    public static Line Create(int lineNo, long operatorId, string operatorNo, string linePlanningNumber, Operator? @operator = null) => new()
    {
        LineNo = lineNo,
        OperatorId = operatorId,
        OperatorNo = operatorNo,
        LinePlanningNumber = linePlanningNumber,
        Operator = @operator
    };
}
