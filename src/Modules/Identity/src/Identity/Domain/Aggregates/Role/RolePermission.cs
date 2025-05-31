using BuildingBlocks.Domain.Model;

namespace Identity.Domain.Aggregates.Role;

public class RolePermission : Entity, IAuditableEntity
{
    public long RoleId { get; private set; }
    public string PermissionName { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public ApplicationRole Role { get; private set; }

    private RolePermission()
    {
        // Required by EF Core
    }

    public static RolePermission Create(ApplicationRole role, string permissionName)
    {
        var rolePermission = new RolePermission
        {
            RoleId = role.Id,
            PermissionName = permissionName,
            CreatedAt = DateTime.UtcNow,
            Role = role
        };

        return rolePermission;
    }
} 