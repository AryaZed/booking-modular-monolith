using System.Collections.Generic;
using System.Threading.Tasks;

namespace Identity.Services;

/// <summary>
/// Service for checking and validating user permissions in the system
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Checks if a user has a specific permission
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="permission">The permission to check</param>
    /// <returns>True if the user has the permission, false otherwise</returns>
    Task<bool> HasPermissionAsync(long userId, string permission);
    
    /// <summary>
    /// Checks if a user has a specific permission for a specific tenant
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="permission">The permission to check</param>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>True if the user has the permission for the tenant, false otherwise</returns>
    Task<bool> HasPermissionForTenantAsync(long userId, string permission, long tenantId);
    
    /// <summary>
    /// Gets all permissions assigned to a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>A set of permission names</returns>
    Task<HashSet<string>> GetUserPermissionsAsync(long userId);
    
    /// <summary>
    /// Gets all permissions assigned to a user for a specific tenant
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>A set of permission names</returns>
    Task<HashSet<string>> GetUserPermissionsForTenantAsync(long userId, long tenantId);
} 