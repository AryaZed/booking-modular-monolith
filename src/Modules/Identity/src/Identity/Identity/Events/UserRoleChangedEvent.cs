using System;
using BuildingBlocks.Domain;

namespace Identity.Identity.Events;

/// <summary>
/// Event published when a user's role is changed (added or removed)
/// </summary>
public record UserRoleChangedEvent : IDomainEvent
{
    public long UserId { get; init; }
    public string Email { get; init; }
    public long RoleId { get; init; }
    public string RoleName { get; init; }
    public long TenantId { get; init; }
    public string TenantType { get; init; }
    public bool IsRoleAdded { get; init; } // true if role was added, false if removed
    public DateTime ChangedAt { get; init; }
} 