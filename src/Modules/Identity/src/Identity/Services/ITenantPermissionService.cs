using System.Collections.Generic;
using System.Threading.Tasks;
using Identity.Identity.Models;

namespace Identity.Services;

/// <summary>
/// Service for managing tenant-specific permissions
/// </summary>
public interface ITenantPermissionService
{
    /// <summary>
    /// Gets allowed permissions for a tenant based on its type
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>A set of allowed permission names</returns>
    Task<HashSet<string>> GetAllowedPermissionsForTenantAsync(long tenantId);
    
    /// <summary>
    /// Gets the type of a tenant
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>The tenant type or null if tenant not found</returns>
    Task<TenantType?> GetTenantTypeAsync(long tenantId);
    
    /// <summary>
    /// Checks if a user can manage roles for a tenant
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="userId">The user ID</param>
    /// <returns>True if the user can manage roles for the tenant, false otherwise</returns>
    Task<bool> CanManageRolesForTenantAsync(long tenantId, long userId);
    
    /// <summary>
    /// Checks if a user can manage users for a tenant
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="userId">The user ID</param>
    /// <returns>True if the user can manage users for the tenant, false otherwise</returns>
    Task<bool> CanManageUsersForTenantAsync(long tenantId, long userId);
} 