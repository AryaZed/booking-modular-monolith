using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Constants;
using BuildingBlocks.Exception;
using Identity.Data;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Services;

/// <summary>
/// Service that validates user permissions against various resources and operations
/// </summary>
public class PermissionValidator : IPermissionValidator
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityContext _context;
    private readonly ILogger<PermissionValidator> _logger;

    public PermissionValidator(
        UserManager<ApplicationUser> userManager,
        IdentityContext context,
        ILogger<PermissionValidator> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<bool> CanManageRolesForTenantAsync(long tenantId, long userId)
    {
        try
        {
            _logger.LogDebug("Checking if user {UserId} can manage roles for tenant {TenantId}", userId, tenantId);
            
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when checking role management permission", userId);
                return false;
            }

            // System admins can manage roles for any tenant
            if (await _userManager.IsInRoleAsync(user, IdentityConstant.Role.Admin))
            {
                _logger.LogDebug("User {UserId} is a system admin, granting role management permission", userId);
                return true;
            }

            // Check if user has permission to manage roles for this tenant
            var hasPermission = await UserHasPermissionForTenantAsync(
                userId,
                PermissionsConstant.RoleManagement.CreateRole,
                tenantId);

            _logger.LogDebug("User {UserId} permission check for role management: {HasPermission}", userId, hasPermission);
            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking role management permission for user {UserId} on tenant {TenantId}", userId, tenantId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> CanManageUsersForTenantAsync(long tenantId, long userId)
    {
        try
        {
            _logger.LogDebug("Checking if user {UserId} can manage users for tenant {TenantId}", userId, tenantId);
            
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when checking user management permission", userId);
                return false;
            }

            // System admins can manage users for any tenant
            if (await _userManager.IsInRoleAsync(user, IdentityConstant.Role.Admin))
            {
                _logger.LogDebug("User {UserId} is a system admin, granting user management permission", userId);
                return true;
            }

            // Check if user has permission to manage users for this tenant
            var hasPermission = await UserHasPermissionForTenantAsync(
                userId,
                PermissionsConstant.RoleManagement.AssignRole,
                tenantId);

            _logger.LogDebug("User {UserId} permission check for user management: {HasPermission}", userId, hasPermission);
            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user management permission for user {UserId} on tenant {TenantId}", userId, tenantId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<HashSet<string>> GetAllowedPermissionsForTenantAsync(long tenantId)
    {
        try
        {
            _logger.LogDebug("Getting allowed permissions for tenant {TenantId}", tenantId);
            
            // Get the tenant with its type
            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId);
                
            if (tenant == null)
            {
                _logger.LogWarning("Tenant {TenantId} not found when getting allowed permissions", tenantId);
                return new HashSet<string>();
            }

            // Return permissions based on tenant type
            switch (tenant.Type)
            {
                case TenantType.Brand:
                    return PermissionsConstant.BrandLevelPermissions;
                    
                case TenantType.Branch:
                    return PermissionsConstant.BranchLevelPermissions;
                    
                // Handle other tenant types with their specific permissions
                case TenantType.Department:
                case TenantType.Team:
                case TenantType.Project:
                    // These could have more specific permission sets in the future
                    return PermissionsConstant.RegularUserPermissions;
                    
                case TenantType.System:
                    return PermissionsConstant.SystemAdminPermissions;
                    
                default:
                    _logger.LogWarning("Unknown tenant type {TenantType} for tenant {TenantId}", tenant.Type, tenantId);
                    return new HashSet<string>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting allowed permissions for tenant {TenantId}", tenantId);
            return new HashSet<string>();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UserHasPermissionAsync(long userId, string permission)
    {
        if (string.IsNullOrEmpty(permission))
        {
            throw new ArgumentNullException(nameof(permission));
        }
        
        try
        {
            _logger.LogDebug("Checking if user {UserId} has permission {Permission}", userId, permission);
            
            // Get all permissions assigned to the user through roles
            var permissions = await GetUserPermissionsAsync(userId);

            var hasPermission = permissions.Contains(permission);
            _logger.LogDebug("User {UserId} permission check for {Permission}: {HasPermission}", userId, permission, hasPermission);
            
            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", permission, userId);
            return false;
        }
    }

    /// <summary>
    /// Checks if a user has a specific permission for a specific tenant
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="permission">The permission to check</param>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>True if the user has the permission for the tenant, false otherwise</returns>
    private async Task<bool> UserHasPermissionForTenantAsync(long userId, string permission, long tenantId)
    {
        if (string.IsNullOrEmpty(permission))
        {
            throw new ArgumentNullException(nameof(permission));
        }
        
        try
        {
            // Get user's tenant roles for this specific tenant
            var userTenantRoles = await _context.UserTenantRoles
                .AsNoTracking()
                .Where(utr => utr.UserId == userId && utr.TenantId == tenantId && utr.IsActive)
                .Select(utr => utr.RoleId)
                .ToListAsync();

            if (!userTenantRoles.Any())
            {
                _logger.LogDebug("User {UserId} has no roles for tenant {TenantId}", userId, tenantId);
                return false;
            }

            // Check if any of these roles has the required permission
            var hasPermission = await _context.RolePermissions
                .AsNoTracking()
                .AnyAsync(rp => userTenantRoles.Contains(rp.RoleId) && rp.Permission == permission);

            _logger.LogDebug("User {UserId} permission check for {Permission} on tenant {TenantId}: {HasPermission}", 
                userId, permission, tenantId, hasPermission);
                
            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking tenant-specific permission {Permission} for user {UserId} on tenant {TenantId}", 
                permission, userId, tenantId);
            return false;
        }
    }

    /// <summary>
    /// Gets all permissions assigned to a user through their roles
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>A set of permission names</returns>
    private async Task<HashSet<string>> GetUserPermissionsAsync(long userId)
    {
        try
        {
            // Get all user's active tenant roles
            var roleIds = await _context.UserTenantRoles
                .AsNoTracking()
                .Where(utr => utr.UserId == userId && utr.IsActive)
                .Select(utr => utr.RoleId)
                .ToListAsync();

            // Also get global roles (not tenant-specific)
            var systemRoleIds = await _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();
                
            // Combine both role sets
            var allRoleIds = new HashSet<long>(roleIds.Concat(systemRoleIds));

            if (!allRoleIds.Any())
            {
                _logger.LogDebug("User {UserId} has no roles with permissions", userId);
                return new HashSet<string>();
            }

            // Get permissions from these roles
            var permissions = await _context.RolePermissions
                .AsNoTracking()
                .Where(rp => allRoleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission)
                .ToListAsync();

            _logger.LogDebug("User {UserId} has {PermissionCount} permissions", userId, permissions.Count);
            return new HashSet<string>(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for user {UserId}", userId);
            return new HashSet<string>();
        }
    }
}
