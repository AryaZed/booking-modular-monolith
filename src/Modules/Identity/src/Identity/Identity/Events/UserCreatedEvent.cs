using System;
using BuildingBlocks.Domain;

namespace Identity.Identity.Events;

/// <summary>
/// Event published when a new user is created
/// </summary>
public record UserCreatedEvent : IDomainEvent
{
    public long UserId { get; init; }
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public long? TenantId { get; init; }
    public string TenantType { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
} 