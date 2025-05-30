using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Constants;
using Identity.Data;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Services;

public class PermissionValidator : IPermissionValidator
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityContext _context;

    public PermissionValidator(
        UserManager<ApplicationUser> userManager,
        IdentityContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<bool> CanManageRolesForTenantAsync(long tenantId, long userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;

        // System admins can manage roles for any tenant
        if (await _userManager.IsInRoleAsync(user, IdentityConstant.Role.Admin))
            return true;

        // Check if user has permission to manage roles for this tenant
        var hasPermission = await UserHasPermissionForTenantAsync(
            userId,
            PermissionsConstant.RoleManagement.CreateRole,
            tenantId);

        return hasPermission;
    }

    public async Task<bool> CanManageUsersForTenantAsync(long tenantId, long userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;

        // System admins can manage users for any tenant
        if (await _userManager.IsInRoleAsync(user, IdentityConstant.Role.Admin))
            return true;

        // Check if user has permission to manage roles for this tenant
        var hasPermission = await UserHasPermissionForTenantAsync(
            userId,
            PermissionsConstant.RoleManagement.AssignRole,
            tenantId);

        return hasPermission;
    }

    public async Task<HashSet<string>> GetAllowedPermissionsForTenantAsync(long tenantId)
    {
        // First check if it's a brand
        var brand = await _context.Tenants.FirstOrDefaultAsync(b => b.Id == tenantId && b.Type == TenantType.Brand);
        if (brand != null)
        {
            return PermissionsConstant.BrandLevelPermissions;
        }

        // Check if it's a branch
        var branch = await _context.Tenants.FirstOrDefaultAsync(b => b.Id == tenantId && b.Type == TenantType.Branch);
        if (branch != null)
        {
            return PermissionsConstant.BranchLevelPermissions;
        }

        // Default to empty set if tenant type is unknown
        return new HashSet<string>();
    }

    public async Task<bool> UserHasPermissionAsync(long userId, string permission)
    {
        // Get all permissions assigned to the user through roles
        var permissions = await GetUserPermissionsAsync(userId);

        return permissions.Contains(permission);
    }

    private async Task<bool> UserHasPermissionForTenantAsync(long userId, string permission, long tenantId)
    {
        // Get user's tenant roles for this specific tenant
        var userTenantRoles = await _context.UserTenantRoles
            .Where(utr => utr.UserId == userId && utr.TenantId == tenantId && utr.IsActive)
            .Select(utr => utr.RoleId)
            .ToListAsync();

        if (!userTenantRoles.Any())
            return false;

        // Check if any of these roles has the required permission
        var hasPermission = await _context.RolePermissions
            .AnyAsync(rp => userTenantRoles.Contains(rp.RoleId) && rp.Permission == permission);

        return hasPermission;
    }

    private async Task<HashSet<string>> GetUserPermissionsAsync(long userId)
    {
        // Get all user's active tenant roles
        var roleIds = await _context.UserTenantRoles
            .Where(utr => utr.UserId == userId && utr.IsActive)
            .Select(utr => utr.RoleId)
            .ToListAsync();

        if (!roleIds.Any())
            return new HashSet<string>();

        // Get permissions from these roles
        var permissions = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission)
            .ToListAsync();

        return new HashSet<string>(permissions);
    }
}
