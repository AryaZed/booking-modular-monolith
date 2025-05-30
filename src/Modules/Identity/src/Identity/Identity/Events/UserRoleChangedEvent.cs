using System;
using BuildingBlocks.Domain.Event;

namespace Identity.Identity.Events;

/// <summary>
/// Event published when a user's role is changed (added or removed)
/// </summary>
public class UserRoleChangedEvent : IDomainEvent
{
    public long UserId { get; }
    public string Email { get; init; }
    public long RoleId { get; }
    public string RoleName { get; }
    public long? TenantId { get; }
    public string TenantType { get; init; }
    public bool IsRoleAdded { get; init; } // true if role was added, false if removed
    public DateTime ChangedAt { get; }
    public RoleChangeType ChangeType { get; }

    public UserRoleChangedEvent(long userId, long roleId, string roleName, RoleChangeType changeType, long? tenantId = null)
    {
        UserId = userId;
        RoleId = roleId;
        RoleName = roleName;
        ChangeType = changeType;
        TenantId = tenantId;
        ChangedAt = DateTime.UtcNow;
    }
}

public enum RoleChangeType
{
    Added,
    Removed
} 