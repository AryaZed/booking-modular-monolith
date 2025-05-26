using System.Collections.Generic;
using System.Threading.Tasks;
using Identity.Identity.Models;

namespace Identity.Services;

public interface ITenantRoleService
{
    Task<ApplicationRole> CreateRoleAsync(
        long tenantId, 
        string roleName, 
        string description,
        IEnumerable<string> permissions,
        long createdById);
        
    Task<ApplicationRole> UpdateRoleAsync(
        long roleId,
        string description,
        IEnumerable<string> permissions,
        long updatedById);
        
    Task<bool> DeleteRoleAsync(long roleId, long deletedById);
    
    Task<IEnumerable<ApplicationRole>> GetRolesForTenantAsync(long tenantId);
    
    Task<IEnumerable<string>> GetPermissionsForRoleAsync(long roleId);
} 