using System.Threading.Tasks;
using BuildingBlocks.Domain.Event;

namespace BuildingBlocks.Domain;

/// <summary>
/// Interface for event dispatching services
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatches a domain event
    /// </summary>
    /// <param name="event">The domain event to dispatch</param>
    Task DispatchAsync(IDomainEvent @event);
} 