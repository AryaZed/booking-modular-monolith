using System.Linq.Expressions;
using Identity.Application.Interfaces;
using Identity.Domain.Aggregates.Role;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly IdentityContext _context;

    public RoleRepository(IdentityContext context)
    {
        _context = context;
    }

    public async Task<ApplicationRole> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.Tenant)
            .Include(r => r.ParentRole)
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<ApplicationRole> GetByNameAsync(string name, long? tenantId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.Tenant)
            .Include(r => r.ParentRole)
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.NormalizedName == name.ToUpperInvariant() && r.TenantId == tenantId, cancellationToken);
    }

    public async Task<IEnumerable<ApplicationRole>> GetRolesForTenantAsync(long tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Where(r => r.TenantId == tenantId)
            .Include(r => r.Permissions)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ApplicationRole>> GetSystemRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Where(r => r.TenantId == null)
            .Include(r => r.Permissions)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ApplicationRole>> FindAsync(Expression<Func<ApplicationRole, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Where(predicate)
            .Include(r => r.Tenant)
            .Include(r => r.Permissions)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ApplicationRole> AddAsync(ApplicationRole role, CancellationToken cancellationToken = default)
    {
        await _context.Roles.AddAsync(role, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return role;
    }

    public async Task UpdateAsync(ApplicationRole role, CancellationToken cancellationToken = default)
    {
        _context.Roles.Update(role);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ApplicationRole role, CancellationToken cancellationToken = default)
    {
        // Implement as soft delete
        role.SoftDelete();
        _context.Roles.Update(role);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<ApplicationRole, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Roles.AnyAsync(predicate, cancellationToken);
    }

    public async Task<IEnumerable<string>> GetPermissionsForRoleAsync(long roleId, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .Include(r => r.ParentRole)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role == null)
        {
            return Enumerable.Empty<string>();
        }

        return role.GetAllPermissions();
    }

    public async Task AddPermissionToRoleAsync(long roleId, string permissionName, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role == null)
        {
            throw new InvalidOperationException("Role not found");
        }

        role.AddPermission(permissionName);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePermissionFromRoleAsync(long roleId, string permissionName, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role == null)
        {
            throw new InvalidOperationException("Role not found");
        }

        role.RemovePermission(permissionName);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RoleHasPermissionAsync(long roleId, string permissionName, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .Include(r => r.ParentRole)
                .ThenInclude(pr => pr != null ? pr.Permissions : null)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role == null)
        {
            return false;
        }

        return role.HasPermission(permissionName);
    }
} 