using BuildingBlocks.Domain.Event;

namespace Identity.Domain.Events;

public record RoleCreatedEvent : IDomainEvent
{
    public long RoleId { get; }
    public string RoleName { get; }
    public long? TenantId { get; }
    public IEnumerable<string> Permissions { get; }
    public DateTime CreatedAt { get; }

    public RoleCreatedEvent(long roleId, string roleName, long? tenantId, IEnumerable<string> permissions)
    {
        RoleId = roleId;
        RoleName = roleName;
        TenantId = tenantId;
        Permissions = permissions;
        CreatedAt = DateTime.UtcNow;
    }
} 