using System.Collections.Generic;
using System.Threading.Tasks;

namespace Identity.Services;

public interface IPermissionValidator
{
    Task<bool> CanManageRolesForTenantAsync(long tenantId, long userId);
    Task<bool> CanManageUsersForTenantAsync(long tenantId, long userId);
    Task<HashSet<string>> GetAllowedPermissionsForTenantAsync(long tenantId);
    Task<bool> UserHasPermissionAsync(long userId, string permission);
} 