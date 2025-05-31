using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Identity.Identity.Models;

namespace Identity.Identity.Repositories
{
    public interface IRoleRepository
    {
        Task<ApplicationRole> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ApplicationRole> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);
        Task<IEnumerable<ApplicationRole>> GetRolesForTenantAsync(long tenantId, CancellationToken cancellationToken = default);
        Task<IEnumerable<RolePermission>> GetPermissionsForRoleAsync(long roleId, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
} 