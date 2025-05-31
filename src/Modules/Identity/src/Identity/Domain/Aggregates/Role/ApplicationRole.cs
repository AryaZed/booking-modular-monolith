using BuildingBlocks.Domain.Model;
using Identity.Domain.Events;
using Microsoft.AspNetCore.Identity;

namespace Identity.Domain.Aggregates.Role;

public class ApplicationRole : IdentityRole<long>, IAggregateRoot, IAuditableEntity, ISoftDeletableEntity
{
    public long? TenantId { get; private set; }
    public string Description { get; private set; }
    public bool IsDefault { get; private set; }
    public long? ParentRoleId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    // Navigation properties
    public Tenant.Tenant Tenant { get; private set; }
    public ApplicationRole ParentRole { get; private set; }
    public ICollection<ApplicationRole> ChildRoles { get; private set; } = new List<ApplicationRole>();
    public ICollection<User.UserTenantRole> UserTenantRoles { get; private set; } = new List<User.UserTenantRole>();
    public ICollection<RolePermission> Permissions { get; private set; } = new List<RolePermission>();

    private ApplicationRole()
    {
        // Required by EF Core
    }

    public static ApplicationRole Create(string name, Tenant.Tenant tenant = null, string description = null, bool isDefault = false, ApplicationRole parentRole = null)
    {
        var role = new ApplicationRole
        {
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            TenantId = tenant?.Id,
            Tenant = tenant,
            Description = description,
            IsDefault = isDefault,
            ParentRoleId = parentRole?.Id,
            ParentRole = parentRole,
            CreatedAt = DateTime.UtcNow
        };

        role.AddDomainEvent(new RoleCreatedEvent(
            0, name, tenant?.Id, Array.Empty<string>()));

        return role;
    }

    public void Update(string name, string description, bool isDefault)
    {
        Name = name;
        NormalizedName = name.ToUpperInvariant();
        Description = description;
        IsDefault = isDefault;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddPermission(string permissionName)
    {
        if (!Permissions.Any(p => p.PermissionName == permissionName))
        {
            var permission = RolePermission.Create(this, permissionName);
            Permissions.Add(permission);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemovePermission(string permissionName)
    {
        var permission = Permissions.FirstOrDefault(p => p.PermissionName == permissionName);
        if (permission != null)
        {
            Permissions.Remove(permission);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public bool HasPermission(string permissionName)
    {
        return Permissions.Any(p => p.PermissionName == permissionName) ||
               (ParentRole != null && ParentRole.HasPermission(permissionName));
    }

    public IEnumerable<string> GetAllPermissions()
    {
        var allPermissions = Permissions.Select(p => p.PermissionName).ToList();
        
        if (ParentRole != null)
        {
            allPermissions.AddRange(ParentRole.GetAllPermissions());
        }
        
        return allPermissions.Distinct();
    }

    public void SetParentRole(ApplicationRole parentRole)
    {
        if (parentRole == this)
        {
            throw new InvalidOperationException("A role cannot be its own parent");
        }

        // Check for circular references
        var currentRole = parentRole;
        while (currentRole != null)
        {
            if (currentRole.ParentRoleId == Id)
            {
                throw new InvalidOperationException("Circular role hierarchy detected");
            }
            currentRole = currentRole.ParentRole;
        }

        ParentRoleId = parentRole?.Id;
        ParentRole = parentRole;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
    }
} 