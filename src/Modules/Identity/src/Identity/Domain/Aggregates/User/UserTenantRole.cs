using BuildingBlocks.Domain.Model;
using Identity.Domain.Aggregates.Role;
using Identity.Domain.Aggregates.Tenant;

namespace Identity.Domain.Aggregates.User;

public class UserTenantRole : Entity, IAuditableEntity
{
    public long UserId { get; private set; }
    public long TenantId { get; private set; }
    public long RoleId { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public ApplicationUser User { get; private set; }
    public Tenant.Tenant Tenant { get; private set; }
    public ApplicationRole Role { get; private set; }

    private UserTenantRole()
    {
        // Required by EF Core
    }

    public static UserTenantRole Create(ApplicationUser user, Tenant.Tenant tenant, ApplicationRole role, bool isDefault = false)
    {
        var userTenantRole = new UserTenantRole
        {
            UserId = user.Id,
            TenantId = tenant.Id,
            RoleId = role.Id,
            IsDefault = isDefault,
            CreatedAt = DateTime.UtcNow,
            User = user,
            Tenant = tenant,
            Role = role
        };

        return userTenantRole;
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRole(ApplicationRole newRole)
    {
        RoleId = newRole.Id;
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }
} 