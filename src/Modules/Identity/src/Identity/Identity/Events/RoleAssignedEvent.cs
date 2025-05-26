using System;
using BuildingBlocks.Domain.Event;

namespace Identity.Identity.Events;

public class RoleAssignedEvent : IDomainEvent
{
    public long UserId { get; }
    public long RoleId { get; }
    public string RoleName { get; }
    public long? TenantId { get; }
    public string TenantType { get; }
    public DateTime AssignedAt { get; }
    public long AssignedBy { get; }
    
    public RoleAssignedEvent(
        long userId, 
        long roleId, 
        string roleName, 
        long? tenantId,
        string tenantType,
        long assignedBy)
    {
        UserId = userId;
        RoleId = roleId;
        RoleName = roleName;
        TenantId = tenantId;
        TenantType = tenantType;
        AssignedAt = DateTime.UtcNow;
        AssignedBy = assignedBy;
    }
} 