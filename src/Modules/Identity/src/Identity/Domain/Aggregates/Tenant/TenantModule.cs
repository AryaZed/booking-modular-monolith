using BuildingBlocks.Domain.Model;

namespace Identity.Domain.Aggregates.Tenant;

public class TenantModule : Entity, IAuditableEntity
{
    public long TenantId { get; private set; }
    public long ModuleId { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public Tenant Tenant { get; private set; }
    public Module Module { get; private set; }

    private TenantModule()
    {
        // Required by EF Core
    }

    public static TenantModule Create(Tenant tenant, Module module, bool isEnabled = true)
    {
        var tenantModule = new TenantModule
        {
            TenantId = tenant.Id,
            ModuleId = module.Id,
            IsEnabled = isEnabled,
            CreatedAt = DateTime.UtcNow,
            Tenant = tenant,
            Module = module
        };

        return tenantModule;
    }

    public void SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
        UpdatedAt = DateTime.UtcNow;
    }
} 