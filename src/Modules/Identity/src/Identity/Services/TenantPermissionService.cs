using System;
using System.Collections.Generic;
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
/// Service for managing tenant-specific permissions
/// </summary>
public class TenantPermissionService : ITenantPermissionService
{
    private readonly IdentityContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<TenantPermissionService> _logger;

    public TenantPermissionService(
        IdentityContext context,
        UserManager<ApplicationUser> userManager,
        IPermissionService permissionService,
        ILogger<TenantPermissionService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<HashSet<string>> GetAllowedPermissionsForTenantAsync(long tenantId)
    {
        try
        {
            _logger.LogDebug("Getting allowed permissions for tenant {TenantId}", tenantId);
            
            // Get the tenant type
            var tenantType = await GetTenantTypeAsync(tenantId);
            if (!tenantType.HasValue)
            {
                _logger.LogWarning("Tenant {TenantId} not found when getting allowed permissions", tenantId);
                return new HashSet<string>();
            }

            // Return permissions based on tenant type
            var permissions = tenantType.Value switch
            {
                TenantType.Brand => PermissionsConstant.BrandLevelPermissions,
                TenantType.Branch => PermissionsConstant.BranchLevelPermissions,
                TenantType.Department or TenantType.Team or TenantType.Project => PermissionsConstant.RegularUserPermissions,
                TenantType.System => PermissionsConstant.SystemAdminPermissions,
                _ => new HashSet<string>()
            };

            _logger.LogDebug("Tenant {TenantId} of type {TenantType} has {PermissionCount} allowed permissions", 
                tenantId, tenantType.Value, permissions.Count);
                
            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting allowed permissions for tenant {TenantId}", tenantId);
            return new HashSet<string>();
        }
    }

    /// <inheritdoc/>
    public async Task<TenantType?> GetTenantTypeAsync(long tenantId)
    {
        try
        {
            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId);
                
            if (tenant == null)
            {
                _logger.LogWarning("Tenant {TenantId} not found when getting tenant type", tenantId);
                return null;
            }

            _logger.LogDebug("Tenant {TenantId} has type {TenantType}", tenantId, tenant.Type);
            return tenant.Type;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant type for tenant {TenantId}", tenantId);
            return null;
        }
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
            var hasPermission = await _permissionService.HasPermissionForTenantAsync(
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
            var hasPermission = await _permissionService.HasPermissionForTenantAsync(
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
} 