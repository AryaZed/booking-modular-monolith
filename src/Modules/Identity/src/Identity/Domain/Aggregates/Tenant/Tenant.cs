using BuildingBlocks.Domain.Model;
using Identity.Domain.Events;
using Identity.Domain.Exceptions;
using Identity.Domain.ValueObjects;

namespace Identity.Domain.Aggregates.Tenant;

public class Tenant : AggregateRoot, IAuditableEntity, ISoftDeletableEntity
{
    public string Name { get; private set; }
    private string _key;
    public string Key 
    { 
        get => _key;
        private set => _key = value.ToLowerInvariant();
    }
    public TenantType Type { get; private set; }
    public long? ParentTenantId { get; private set; }
    public string DatabaseConnectionString { get; private set; }
    public string SchemaName { get; private set; }
    public TenantStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    // Navigation properties
    public Tenant ParentTenant { get; private set; }
    public ICollection<Tenant> ChildTenants { get; private set; } = new List<Tenant>();
    public ICollection<User.UserTenantRole> UserRoles { get; private set; } = new List<User.UserTenantRole>();
    public ICollection<TenantModule> Modules { get; private set; } = new List<TenantModule>();

    private Tenant()
    {
        // Required by EF Core
    }

    public static Tenant Create(
        string name, 
        TenantKey key, 
        TenantType type, 
        Tenant parentTenant = null, 
        string databaseConnectionString = null, 
        string schemaName = null)
    {
        var tenant = new Tenant
        {
            Name = name,
            Key = key,
            Type = type,
            ParentTenantId = parentTenant?.Id,
            ParentTenant = parentTenant,
            DatabaseConnectionString = databaseConnectionString,
            SchemaName = schemaName ?? key,
            Status = TenantStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        tenant.AddDomainEvent(new TenantCreatedEvent(
            0, name, key, type.ToString(), parentTenant?.Id));

        return tenant;
    }

    public void Update(string name, TenantStatus status)
    {
        Name = name;
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status == TenantStatus.Suspended)
        {
            Status = TenantStatus.Active;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Suspend()
    {
        if (Status == TenantStatus.Active)
        {
            Status = TenantStatus.Suspended;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Archive()
    {
        if (Status != TenantStatus.Archived)
        {
            Status = TenantStatus.Archived;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void SoftDelete()
    {
        if (ChildTenants.Any(t => !t.IsDeleted))
        {
            throw new InvalidTenantOperationException("Cannot delete a tenant that has active child tenants");
        }

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        if (ParentTenant != null && ParentTenant.IsDeleted)
        {
            throw new InvalidTenantOperationException("Cannot restore a tenant whose parent is deleted");
        }

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddModule(TenantModule module)
    {
        if (Modules.Any(m => m.ModuleId == module.ModuleId))
        {
            throw new InvalidTenantOperationException("Module is already assigned to this tenant");
        }

        Modules.Add(module);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveModule(long moduleId)
    {
        var module = Modules.FirstOrDefault(m => m.ModuleId == moduleId);
        if (module != null)
        {
            Modules.Remove(module);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void SetModuleEnabled(long moduleId, bool isEnabled)
    {
        var module = Modules.FirstOrDefault(m => m.ModuleId == moduleId);
        if (module != null)
        {
            module.SetEnabled(isEnabled);
            UpdatedAt = DateTime.UtcNow;
        }
    }
} 