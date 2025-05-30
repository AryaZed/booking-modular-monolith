using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.Data;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Services;

public class TenantRoleService : ITenantRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IdentityContext _context;
    private readonly IPermissionValidator _permissionValidator;

    public TenantRoleService(
        RoleManager<ApplicationRole> roleManager,
        IdentityContext context,
        IPermissionValidator permissionValidator)
    {
        _roleManager = roleManager;
        _context = context;
        _permissionValidator = permissionValidator;
    }

    public async Task<ApplicationRole> CreateRoleAsync(
        long tenantId,
        string roleName,
        string description,
        IEnumerable<string> permissions,
        long createdById)
    {
        // Verify the user has permission to create roles for this tenant
        if (!await _permissionValidator.CanManageRolesForTenantAsync(tenantId, createdById))
        {
            throw new UnauthorizedAccessException("User does not have permission to manage roles for this tenant");
        }

        // Validate tenant can only assign permissions they have access to
        var allowedPermissions = await _permissionValidator.GetAllowedPermissionsForTenantAsync(tenantId);
        foreach (var permission in permissions)
        {
            if (!allowedPermissions.Contains(permission))
            {
                throw new UnauthorizedAccessException($"Tenant cannot assign permission {permission}");
            }
        }

        // Create the role with tenant prefix to ensure uniqueness
        var role = new ApplicationRole
        {
            Name = $"{tenantId}_{roleName}",
            TenantId = tenantId,
            IsCustom = true,
            Description = description,
            CreatedById = createdById
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Assign permissions to the role
        foreach (var permission in permissions)
        {
            await _context.RolePermissions.AddAsync(new RolePermission
            {
                RoleId = role.Id,
                Permission = permission,
                CreatedById = createdById
            });
        }

        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<ApplicationRole> UpdateRoleAsync(
        long roleId,
        string description,
        IEnumerable<string> permissions,
        long updatedById)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            throw new Exception($"Role with ID {roleId} not found");
        }

        // Verify the user has permission to update roles for this tenant
        if (!role.TenantId.HasValue || !await _permissionValidator.CanManageRolesForTenantAsync(role.TenantId.Value, updatedById))
        {
            throw new UnauthorizedAccessException("User does not have permission to manage roles for this tenant");
        }

        // Validate tenant can only assign permissions they have access to
        var allowedPermissions = await _permissionValidator.GetAllowedPermissionsForTenantAsync(role.TenantId.Value);
        foreach (var permission in permissions)
        {
            if (!allowedPermissions.Contains(permission))
            {
                throw new UnauthorizedAccessException($"Tenant cannot assign permission {permission}");
            }
        }

        // Update role properties
        role.Description = description;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to update role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Update permissions
        // First, remove existing permissions
        var existingPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(existingPermissions);

        // Then add the new permissions
        foreach (var permission in permissions)
        {
            await _context.RolePermissions.AddAsync(new RolePermission
            {
                RoleId = role.Id,
                Permission = permission,
                CreatedById = updatedById
            });
        }

        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<bool> DeleteRoleAsync(long roleId, long deletedById)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            throw new Exception($"Role with ID {roleId} not found");
        }

        // Verify the user has permission to delete roles for this tenant
        if (!role.TenantId.HasValue || !await _permissionValidator.CanManageRolesForTenantAsync(role.TenantId.Value, deletedById))
        {
            throw new UnauthorizedAccessException("User does not have permission to manage roles for this tenant");
        }

        // Check if role is in use
        var isRoleInUse = await _context.UserTenantRoles
            .AnyAsync(utr => utr.RoleId == roleId && utr.IsActive);

        if (isRoleInUse)
        {
            throw new Exception("Cannot delete role because it is assigned to users");
        }

        var result = await _roleManager.DeleteAsync(role);
        return result.Succeeded;
    }

    public async Task<IEnumerable<ApplicationRole>> GetRolesForTenantAsync(long tenantId)
    {
        return await _context.Roles
            .Where(r => r.TenantId == tenantId && r.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetPermissionsForRoleAsync(long roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission)
            .ToListAsync();
    }
}
