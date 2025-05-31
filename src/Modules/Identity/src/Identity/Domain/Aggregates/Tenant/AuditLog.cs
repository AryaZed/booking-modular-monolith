using BuildingBlocks.Domain.Model;

namespace Identity.Domain.Aggregates.Tenant;

public class AuditLog : Entity
{
    public long? UserId { get; set; }
    public long? TenantId { get; set; }
    public string EntityName { get; set; }
    public string EntityId { get; set; }
    public string Action { get; set; }
    public string OldValues { get; set; }
    public string NewValues { get; set; }
    public string ClientIp { get; set; }
    public DateTime CreatedAt { get; set; }
} 