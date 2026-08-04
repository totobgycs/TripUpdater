namespace TripUpdater.Domain.Common;

public abstract class Entity
{
    public long Id { get; protected set; }

    public override bool Equals(object? obj) =>
        obj is Entity other && other.GetType() == GetType() && other.Id == Id && other.Id > 0;

    public override int GetHashCode() => Id.GetHashCode();
}
