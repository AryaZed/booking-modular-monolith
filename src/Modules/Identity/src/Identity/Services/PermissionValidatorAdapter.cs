using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Identity.Services;

/// <summary>
/// Adapter class that implements IPermissionValidator but delegates to the new permission services
/// This provides backward compatibility while we transition to the new service architecture
/// </summary>
public class PermissionValidatorAdapter : IPermissionValidator
{
    private readonly IPermissionService _permissionService;
    private readonly ITenantPermissionService _tenantPermissionService;
    private readonly ILogger<PermissionValidatorAdapter> _logger;

    public PermissionValidatorAdapter(
        IPermissionService permissionService,
        ITenantPermissionService tenantPermissionService,
        ILogger<PermissionValidatorAdapter> logger)
    {
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _tenantPermissionService = tenantPermissionService ?? throw new ArgumentNullException(nameof(tenantPermissionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task<bool> CanManageRolesForTenantAsync(long tenantId, long userId)
    {
        _logger.LogDebug("Adapter: Checking if user {UserId} can manage roles for tenant {TenantId}", userId, tenantId);
        return _tenantPermissionService.CanManageRolesForTenantAsync(tenantId, userId);
    }

    /// <inheritdoc/>
    public Task<bool> CanManageUsersForTenantAsync(long tenantId, long userId)
    {
        _logger.LogDebug("Adapter: Checking if user {UserId} can manage users for tenant {TenantId}", userId, tenantId);
        return _tenantPermissionService.CanManageUsersForTenantAsync(tenantId, userId);
    }

    /// <inheritdoc/>
    public Task<HashSet<string>> GetAllowedPermissionsForTenantAsync(long tenantId)
    {
        _logger.LogDebug("Adapter: Getting allowed permissions for tenant {TenantId}", tenantId);
        return _tenantPermissionService.GetAllowedPermissionsForTenantAsync(tenantId);
    }

    /// <inheritdoc/>
    public Task<bool> UserHasPermissionAsync(long userId, string permission)
    {
        _logger.LogDebug("Adapter: Checking if user {UserId} has permission {Permission}", userId, permission);
        return _permissionService.HasPermissionAsync(userId, permission);
    }
} 