using System.Collections.Generic;
using BuildingBlocks.Domain.Event;

namespace BuildingBlocks.Domain
{
    /// <summary>
    /// Interface for entities that can raise domain events
    /// </summary>
    public interface IEntityWithDomainEvents
    {
        /// <summary>
        /// Gets the domain events raised by this entity
        /// </summary>
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
        
        /// <summary>
        /// Clears all domain events from this entity
        /// </summary>
        void ClearDomainEvents();
    }
} 