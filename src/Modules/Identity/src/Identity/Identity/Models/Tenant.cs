using System;
using System.Collections.Generic;
using BuildingBlocks.Domain.Model;

namespace Identity.Identity.Models;

public class Tenant : IAuditableEntity, ISoftDeletableEntity
{
    public long Id { get; private set; }
    public string Name { get; private set; }
    public TenantType Type { get; private set; }
    public string Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public long? ParentTenantId { get; private set; }

    // Audit properties
    public DateTime CreatedAt { get; private set; }
    public long? CreatedBy { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }
    public long? LastModifiedBy { get; private set; }

    // Soft delete properties
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public long? DeletedBy { get; private set; }

    // Navigation properties
    public virtual Tenant ParentTenant { get; private set; }
    public virtual ICollection<Tenant> ChildTenants { get; private set; } = new List<Tenant>();
    public virtual ICollection<UserTenantRole> UserRoles { get; private set; } = new List<UserTenantRole>();

    // Domain methods
    public void Activate(long modifiedBy)
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void Deactivate(long modifiedBy)
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void UpdateName(string name, long modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Tenant name cannot be empty");

        Name = name;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void UpdateDescription(string description, long modifiedBy)
    {
        Description = description;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void UpdateParent(long? newParentId = null, long? modifiedBy = null)
    {
        ParentTenantId = newParentId;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void SoftDelete(long deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    // Factory method
    public static Tenant Create(string name, TenantType type, long? parentTenantId, string description, long? createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Tenant name cannot be empty");

        return new Tenant
        {
            Name = name,
            Type = type,
            ParentTenantId = parentTenantId,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }
}
