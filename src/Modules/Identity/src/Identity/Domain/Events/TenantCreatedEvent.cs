using BuildingBlocks.Domain.Event;

namespace Identity.Domain.Events;

public record TenantCreatedEvent : IDomainEvent
{
    public long TenantId { get; }
    public string TenantName { get; }
    public string TenantKey { get; }
    public string TenantType { get; }
    public long? ParentTenantId { get; }
    public DateTime CreatedAt { get; }

    public TenantCreatedEvent(long tenantId, string tenantName, string tenantKey, string tenantType, long? parentTenantId)
    {
        TenantId = tenantId;
        TenantName = tenantName;
        TenantKey = tenantKey;
        TenantType = tenantType;
        ParentTenantId = parentTenantId;
        CreatedAt = DateTime.UtcNow;
    }
} 