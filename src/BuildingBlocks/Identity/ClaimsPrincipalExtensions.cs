using System;
using System.Security.Claims;
using BuildingBlocks.Authorization;
using BuildingBlocks.Constants;

namespace BuildingBlocks.Identity;

/// <summary>
/// Extension methods for ClaimsPrincipal to easily access common claims
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the user ID from claims
    /// </summary>
    public static long GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ?? 
                   principal.FindFirst(IdentityConstant.ClaimTypes.UserId);
        return claim != null ? long.Parse(claim.Value) : 0;
    }
    
    /// <summary>
    /// Gets the user's email from claims
    /// </summary>
    public static string GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    }
    
    /// <summary>
    /// Gets the user's name from claims
    /// </summary>
    public static string GetName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
    }
    
    /// <summary>
    /// Gets the current tenant ID from claims
    /// </summary>
    public static long? GetTenantId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(IdentityConstant.ClaimTypes.TenantId);
        return claim != null ? long.Parse(claim.Value) : null;
    }
    
    /// <summary>
    /// Gets the current tenant type from claims
    /// </summary>
    public static string GetTenantType(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(IdentityConstant.ClaimTypes.TenantType)?.Value;
    }
    
    /// <summary>
    /// Checks if the user has a specific permission
    /// </summary>
    public static bool HasPermission(this ClaimsPrincipal principal, string permission)
    {
        return principal.HasClaim(c => c.Type == IdentityConstant.ClaimTypes.Permission && c.Value == permission);
    }
    
    /// <summary>
    /// Checks if the user is in a specific role
    /// </summary>
    public static bool IsInRole(this ClaimsPrincipal principal, string role)
    {
        return principal.HasClaim(System.Security.Claims.ClaimTypes.Role, role);
    }
    
    /// <summary>
    /// Checks if the user is a system administrator
    /// </summary>
    public static bool IsSystemAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(IdentityConstant.Role.SystemAdmin) ||
               principal.IsInRole(IdentityConstant.Role.Admin) ||
               principal.HasPermission(PermissionsConstant.System.ManageRoles);
    }
    
    /// <summary>
    /// Checks if the user is a brand administrator
    /// </summary>
    public static bool IsBrandAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(IdentityConstant.Role.BrandAdmin) ||
               principal.HasPermission(PermissionsConstant.Brands.ManageBranches);
    }
    
    /// <summary>
    /// Checks if the user is a branch administrator
    /// </summary>
    public static bool IsBranchAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(IdentityConstant.Role.BranchAdmin) ||
               principal.HasPermission(PermissionsConstant.Branches.ManageCustomers);
    }
    
    /// <summary>
    /// Checks if the user has brand management permission
    /// </summary>
    public static bool CanManageBrandUsers(this ClaimsPrincipal principal)
    {
        return principal.IsSystemAdmin() || 
               principal.IsBrandAdmin() ||
               principal.HasPermission(PermissionsConstant.Brands.ManageBrandUsers);
    }
    
    /// <summary>
    /// Checks if the user has branch management permission
    /// </summary>
    public static bool CanManageBranchUsers(this ClaimsPrincipal principal)
    {
        return principal.IsSystemAdmin() || 
               principal.IsBrandAdmin() ||
               principal.HasPermission(PermissionsConstant.Branches.ManageBranchUsers);
    }
    
    /// <summary>
    /// Checks if the user belongs to a specific tenant
    /// </summary>
    public static bool BelongsToTenant(this ClaimsPrincipal principal, long tenantId)
    {
        var currentTenantId = principal.GetTenantId();
        return currentTenantId.HasValue && currentTenantId.Value == tenantId;
    }
    
    /// <summary>
    /// Checks if the user belongs to a specific tenant type
    /// </summary>
    public static bool BelongsToTenantType(this ClaimsPrincipal principal, string tenantType)
    {
        return principal.GetTenantType() == tenantType;
    }
} 