using System;
using System.Collections.Generic;
using BuildingBlocks.Domain.Event;

namespace Identity.Identity.Events;

public enum PermissionUpdateType
{
    Added,
    Removed
}

public class PermissionUpdatedEvent : IDomainEvent
{
    public long RoleId { get; }
    public string RoleName { get; }
    public long? TenantId { get; }
    public PermissionUpdateType UpdateType { get; }
    public IEnumerable<string> Permissions { get; }
    public DateTime UpdatedAt { get; }
    public long UpdatedBy { get; }
    
    public PermissionUpdatedEvent(
        long roleId,
        string roleName,
        long? tenantId,
        PermissionUpdateType updateType,
        IEnumerable<string> permissions,
        long updatedBy)
    {
        RoleId = roleId;
        RoleName = roleName;
        TenantId = tenantId;
        UpdateType = updateType;
        Permissions = permissions;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
} 