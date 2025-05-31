using System.Linq.Expressions;
using Identity.Domain.Aggregates.Role;

namespace Identity.Application.Interfaces;

public interface IRoleRepository
{
    Task<ApplicationRole> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ApplicationRole> GetByNameAsync(string name, long? tenantId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationRole>> GetRolesForTenantAsync(long tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationRole>> GetSystemRolesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationRole>> FindAsync(Expression<Func<ApplicationRole, bool>> predicate, CancellationToken cancellationToken = default);
    Task<ApplicationRole> AddAsync(ApplicationRole role, CancellationToken cancellationToken = default);
    Task UpdateAsync(ApplicationRole role, CancellationToken cancellationToken = default);
    Task DeleteAsync(ApplicationRole role, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<ApplicationRole, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetPermissionsForRoleAsync(long roleId, CancellationToken cancellationToken = default);
    Task AddPermissionToRoleAsync(long roleId, string permissionName, CancellationToken cancellationToken = default);
    Task RemovePermissionFromRoleAsync(long roleId, string permissionName, CancellationToken cancellationToken = default);
    Task<bool> RoleHasPermissionAsync(long roleId, string permissionName, CancellationToken cancellationToken = default);
} 