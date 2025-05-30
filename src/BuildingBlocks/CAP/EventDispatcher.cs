using System;
using System.Threading.Tasks;
using BuildingBlocks.Contracts.Identity;
using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Event;
using DotNetCore.CAP;
using Mapster;

namespace BuildingBlocks.CAP;

/// <summary>
/// Service responsible for dispatching domain events to CAP message bus
/// </summary>
public class EventDispatcher : IEventDispatcher
{
    private readonly ICapPublisher _capPublisher;

    public EventDispatcher(ICapPublisher capPublisher)
    {
        _capPublisher = capPublisher;
    }

    /// <summary>
    /// Dispatches a domain event to the message bus, mapping it to an integration event
    /// </summary>
    /// <param name="event">The domain event to dispatch</param>
    public async Task DispatchAsync(IDomainEvent @event)
    {
        // Get the event type name
        var eventTypeName = @event.GetType().Name;

        // Map domain event to integration event
        var integrationEvent = MapToIntegrationEvent(@event);

        // If mapping exists, publish the integration event
        if (integrationEvent != null)
        {
            await _capPublisher.PublishAsync(eventTypeName, integrationEvent);
        }
        else
        {
            // If no mapping exists, publish the original event
            // This is a fallback and should be avoided in production
            await _capPublisher.PublishAsync(eventTypeName, @event);
        }
    }

    /// <summary>
    /// Maps a domain event to an integration event
    /// </summary>
    private object MapToIntegrationEvent(IDomainEvent @event)
    {
        var eventType = @event.GetType();
        var eventTypeName = eventType.Name;

        // Map to appropriate integration event based on event type
        return eventTypeName switch
        {
            "UserCreatedEvent" => @event.Adapt<UserCreatedIntegrationEvent>(),
            "UserUpdatedEvent" => @event.Adapt<UserUpdatedIntegrationEvent>(),
            "UserDeletedEvent" => @event.Adapt<UserDeletedIntegrationEvent>(),
            "UserRoleChangedEvent" => @event.Adapt<UserRoleChangedIntegrationEvent>(),
            _ => null // Return null for unknown event types
        };
    }
}
