using BuildingBlocks.Domain.Event;

namespace BuildingBlocks.Domain.Model
{
    /// <summary>
    /// Base aggregate root class for domain entities with long ID
    /// </summary>
    public abstract class Aggregate : Aggregate<long>
    {
    }

    /// <summary>
    /// Base aggregate root class for domain entities with generic ID type
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate identifier</typeparam>
    public abstract class Aggregate<TId> : Entity, IAggregate<TId>
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        
        /// <inheritdoc/>
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        /// <inheritdoc/>
        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            if (domainEvent == null)
                throw new ArgumentNullException(nameof(domainEvent));
                
            _domainEvents.Add(domainEvent);
        }

        /// <inheritdoc/>
        public IEvent[] ClearDomainEvents()
        {
            IEvent[] dequeuedEvents = _domainEvents.ToArray();
            _domainEvents.Clear();
            return dequeuedEvents;
        }

        /// <summary>
        /// Checks if the aggregate has any domain events
        /// </summary>
        /// <returns>True if the aggregate has domain events, false otherwise</returns>
        public bool HasDomainEvents() => _domainEvents.Any();

        /// <inheritdoc/>
        public long Version { get; set; } = -1;

        /// <inheritdoc/>
        public TId Id { get; protected set; }
        
        /// <summary>
        /// Checks if this aggregate is equal to another aggregate
        /// </summary>
        /// <param name="obj">The object to compare with</param>
        /// <returns>True if the aggregates are equal, false otherwise</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not Aggregate<TId> other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            if (Id.Equals(default(TId)) || other.Id.Equals(default(TId)))
                return false;

            return Id.Equals(other.Id);
        }

        /// <summary>
        /// Gets the hash code for the aggregate
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return (GetType().ToString() + Id).GetHashCode();
        }
    }
}
