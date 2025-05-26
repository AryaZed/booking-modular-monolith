using System;
using BuildingBlocks.Domain;

namespace Identity.Identity.Events;

/// <summary>
/// Event published when a user is deleted
/// </summary>
public record UserDeletedEvent : IDomainEvent
{
    public long UserId { get; init; }
    public DateTime DeletedAt { get; init; }
} 