using Krepim.SharedKernel.Domain.Abstractions;

namespace Krepim.SharedKernel.Domain
{
    public abstract class Entity<TId>(TId id) : IEquatable<Entity<TId>>
        where TId : notnull
    {
        public TId Id { get; protected init; } = id;

        public bool Equals(Entity<TId>? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Id.Equals(other.Id);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType())
                return false;

            if (obj is not Entity<TId> entity)
                return false;

            return Equals(entity);
        }

        public override int GetHashCode()
        {
            if (EqualityComparer<TId>.Default.Equals(Id, default!))
            {
                return base.GetHashCode();
            }

            return Id.GetHashCode();
        }

        public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
            Equals(left, right);

        public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
            !Equals(left, right);
    }
}
