using BuildingBlocks.Domain.Event;

namespace BuildingBlocks.Domain.Model;

/// <summary>
/// Interface for aggregate roots in the domain model
/// </summary>
public interface IAggregate : IEntity
{
    /// <summary>
    /// Gets the list of domain events that have been raised in this aggregate
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    
    /// <summary>
    /// Clears all domain events and returns them
    /// </summary>
    /// <returns>Array of events that were cleared</returns>
    IEvent[] ClearDomainEvents();
    
    /// <summary>
    /// Adds a domain event to this aggregate
    /// </summary>
    /// <param name="domainEvent">The domain event to add</param>
    void AddDomainEvent(IDomainEvent domainEvent);
    
    /// <summary>
    /// Gets or sets the version of the aggregate for optimistic concurrency
    /// </summary>
    long Version { get; set; }
}

/// <summary>
/// Generic interface for aggregate roots with a specific ID type
/// </summary>
/// <typeparam name="T">The type of the aggregate identifier</typeparam>
public interface IAggregate<out T> : IAggregate
{
    /// <summary>
    /// Gets the identifier of the aggregate
    /// </summary>
    T Id { get; }
}
