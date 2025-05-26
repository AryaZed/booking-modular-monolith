using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.Data;
using Identity.Identity.Dtos;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Services;

public class UserTenantService : IUserTenantService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityContext _context;
    private readonly IPermissionValidator _permissionValidator;
    
    public UserTenantService(
        UserManager<ApplicationUser> userManager,
        IdentityContext context,
        IPermissionValidator permissionValidator)
    {
        _userManager = userManager;
        _context = context;
        _permissionValidator = permissionValidator;
    }
    
    public async Task<UserTenantRole> AssignUserToTenantAsync(
        long userId, 
        long tenantId,
        TenantType tenantType,
        long roleId,
        long assignedById)
    {
        // Verify the assigning user has permission to manage users for this tenant
        if (!await _permissionValidator.CanManageUsersForTenantAsync(tenantId, assignedById))
        {
            throw new UnauthorizedAccessException("User does not have permission to manage users for this tenant");
        }
        
        // Verify the role belongs to the tenant
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null || role.TenantId != tenantId)
        {
            throw new Exception("Invalid role for the specified tenant");
        }
        
        // Check if user already has this tenant role
        var existingAssignment = await _context.UserTenantRoles
            .FirstOrDefaultAsync(utr => 
                utr.UserId == userId && 
                utr.TenantId == tenantId && 
                utr.TenantType == tenantType);
                
        if (existingAssignment != null)
        {
            // Update existing assignment
            existingAssignment.RoleId = roleId;
            existingAssignment.IsActive = true;
            await _context.SaveChangesAsync();
            return existingAssignment;
        }
        
        // Create new tenant association
        var userTenantRole = new UserTenantRole
        {
            UserId = userId,
            TenantId = tenantId,
            TenantType = tenantType,
            RoleId = roleId,
            CreatedById = assignedById
        };
        
        await _context.UserTenantRoles.AddAsync(userTenantRole);
        await _context.SaveChangesAsync();
        
        return userTenantRole;
    }
    
    public async Task<bool> RemoveUserFromTenantAsync(
        long userTenantRoleId,
        long removedById)
    {
        var userTenantRole = await _context.UserTenantRoles.FindAsync(userTenantRoleId);
        if (userTenantRole == null)
        {
            throw new Exception("User tenant role not found");
        }
        
        // Verify the removing user has permission
        if (!await _permissionValidator.CanManageUsersForTenantAsync(userTenantRole.TenantId, removedById))
        {
            throw new UnauthorizedAccessException("User does not have permission to manage users for this tenant");
        }
        
        // Soft delete by marking as inactive
        userTenantRole.IsActive = false;
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<IEnumerable<UserTenantRoleDto>> GetUserTenantsAsync(long userId)
    {
        var userTenantRoles = await _context.UserTenantRoles
            .Where(utr => utr.UserId == userId && utr.IsActive)
            .Include(utr => utr.Role)
            .ToListAsync();
            
        var result = new List<UserTenantRoleDto>();
        
        foreach (var utr in userTenantRoles)
        {
            string tenantName = null;
            
            if (utr.TenantType == TenantType.Brand)
            {
                tenantName = await _context.Brands
                    .Where(b => b.Id == utr.TenantId)
                    .Select(b => b.Name)
                    .FirstOrDefaultAsync();
            }
            else if (utr.TenantType == TenantType.Branch)
            {
                tenantName = await _context.Branches
                    .Where(b => b.Id == utr.TenantId)
                    .Select(b => b.Name)
                    .FirstOrDefaultAsync();
            }
            
            result.Add(new UserTenantRoleDto
            {
                Id = utr.Id,
                TenantId = utr.TenantId,
                TenantType = utr.TenantType,
                TenantName = tenantName,
                RoleId = utr.RoleId,
                RoleName = utr.Role.Name
            });
        }
        
        return result;
    }
} 