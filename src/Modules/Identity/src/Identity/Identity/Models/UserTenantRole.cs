using System;
using BuildingBlocks.Domain.Model;

namespace Identity.Identity.Models;

public class UserTenantRole : IAuditableEntity
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public long TenantId { get; private set; }
    public long RoleId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public long? CreatedBy { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }
    public long? LastModifiedBy { get; private set; }

    // Navigation properties
    public virtual ApplicationUser User { get; private set; }
    public virtual ApplicationRole Role { get; private set; }
    public virtual Tenant Tenant { get; private set; }

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

    // Factory method
    public static UserTenantRole Create(long userId, long tenantId, long roleId, long createdBy)
    {
        return new UserTenantRole
        {
            UserId = userId,
            TenantId = tenantId,
            RoleId = roleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }
}
