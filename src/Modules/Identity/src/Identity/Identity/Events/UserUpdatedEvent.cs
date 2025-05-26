using System;
using BuildingBlocks.Domain;

namespace Identity.Identity.Events;

/// <summary>
/// Event published when a user is updated
/// </summary>
public record UserUpdatedEvent : IDomainEvent
{
    public long UserId { get; init; }
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public bool IsActive { get; init; }
    public DateTime UpdatedAt { get; init; }
} 