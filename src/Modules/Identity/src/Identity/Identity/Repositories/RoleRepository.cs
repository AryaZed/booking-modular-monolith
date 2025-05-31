using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Identity.Data;
using Identity.Identity.Models;
using Microsoft.EntityFrameworkCore;

namespace Identity.Identity.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly IdentityContext _context;

        public RoleRepository(IdentityContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ApplicationRole> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _context.Roles.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<ApplicationRole> GetByNameAsync(string roleName, CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
        }

        public async Task<IEnumerable<ApplicationRole>> GetRolesForTenantAsync(long tenantId, CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .Where(r => r.TenantId == tenantId || r.TenantId == null)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RolePermission>> GetPermissionsForRoleAsync(long roleId, CancellationToken cancellationToken = default)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
} 