using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Exception;
using Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Services;

/// <summary>
/// Service for checking and validating user permissions in the system
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly IdentityContext _context;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(
        IdentityContext context,
        ILogger<PermissionService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<bool> HasPermissionAsync(long userId, string permission)
    {
        if (string.IsNullOrEmpty(permission))
        {
            throw new ArgumentNullException(nameof(permission));
        }

        try
        {
            _logger.LogDebug("Checking if user {UserId} has permission {Permission}", userId, permission);

            // Get all permissions assigned to the user
            var permissions = await GetUserPermissionsAsync(userId);

            var hasPermission = permissions.Contains(permission);
            _logger.LogDebug("User {UserId} permission check for {Permission}: {HasPermission}",
                userId, permission, hasPermission);

            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", permission, userId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> HasPermissionForTenantAsync(long userId, string permission, long tenantId)
    {
        if (string.IsNullOrEmpty(permission))
        {
            throw new ArgumentNullException(nameof(permission));
        }

        try
        {
            _logger.LogDebug("Checking if user {UserId} has permission {Permission} for tenant {TenantId}",
                userId, permission, tenantId);

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

    /// <inheritdoc/>
    public async Task<HashSet<string>> GetUserPermissionsAsync(long userId)
    {
        try
        {
            _logger.LogDebug("Getting all permissions for user {UserId}", userId);

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

    /// <inheritdoc/>
    public async Task<HashSet<string>> GetUserPermissionsForTenantAsync(long userId, long tenantId)
    {
        try
        {
            _logger.LogDebug("Getting permissions for user {UserId} on tenant {TenantId}", userId, tenantId);

            // Get user's tenant roles for this specific tenant
            var roleIds = await _context.UserTenantRoles
                .AsNoTracking()
                .Where(utr => utr.UserId == userId && utr.TenantId == tenantId && utr.IsActive)
                .Select(utr => utr.RoleId)
                .ToListAsync();

            if (!roleIds.Any())
            {
                _logger.LogDebug("User {UserId} has no roles for tenant {TenantId}", userId, tenantId);
                return new HashSet<string>();
            }

            // Get permissions from these roles
            var permissions = await _context.RolePermissions
                .AsNoTracking()
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission)
                .ToListAsync();

            _logger.LogDebug("User {UserId} has {PermissionCount} permissions for tenant {TenantId}",
                userId, permissions.Count, tenantId);
            return new HashSet<string>(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant-specific permissions for user {UserId} on tenant {TenantId}",
                userId, tenantId);
            return new HashSet<string>();
        }
    }
}
