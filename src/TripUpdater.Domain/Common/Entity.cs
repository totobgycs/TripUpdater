namespace TripUpdater.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; }

    public override bool Equals(object? obj) =>
        obj is Entity other && other.GetType() == GetType() && other.Id == Id && other.Id != Guid.Empty;

    public override int GetHashCode() => Id.GetHashCode();
}
