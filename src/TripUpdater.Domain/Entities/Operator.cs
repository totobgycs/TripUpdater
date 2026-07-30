using TripUpdater.Domain.Common;

namespace TripUpdater.Domain.Entities;

public sealed class Operator : Entity
{
    public string OperatorNo { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    private Operator() { }

    public static Operator Create(string operatorNo, string name) => new()
    {
        Id = Guid.NewGuid(),
        OperatorNo = operatorNo,
        Name = name
    };
}
