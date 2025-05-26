using System.Collections.Generic;
using System.Threading.Tasks;
using Identity.Identity.Dtos;
using Identity.Identity.Models;

namespace Identity.Services;

public interface IUserTenantService
{
    Task<UserTenantRole> AssignUserToTenantAsync(
        long userId, 
        long tenantId,
        TenantType tenantType,
        long roleId,
        long assignedById);
        
    Task<bool> RemoveUserFromTenantAsync(
        long userTenantRoleId,
        long removedById);
        
    Task<IEnumerable<UserTenantRoleDto>> GetUserTenantsAsync(long userId);
} 