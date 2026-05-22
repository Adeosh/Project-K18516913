using Krepim.SharedKernel.Domain.Abstractions;

namespace Krepim.SharedKernel.Domain
{
    public abstract class Entity<TId>(TId id) : IEquatable<Entity<TId>>
        where TId : notnull
    {
        public TId Id { get; protected init; } = id;

        private readonly List<IDomainEvent> _domainEvents = [];

        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void RaiseDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        public bool Equals(Entity<TId>? other)
        {
            if (other is null) 
                return false;

            if (ReferenceEquals(this, other)) 
                return true;

            return Id.Equals(other.Id);
        }

        public override bool Equals(object? obj) =>
            obj is Entity<TId> entity && Equals(entity);

        public override int GetHashCode() =>
            Id.GetHashCode();

        public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
            Equals(left, right);

        public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
            !Equals(left, right);
    }
}
