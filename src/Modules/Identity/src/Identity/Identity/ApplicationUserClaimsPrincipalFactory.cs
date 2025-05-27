using System.Security.Claims;
using BuildingBlocks.Constants;
using Identity.Data;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Identity.Identity;

public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    private readonly IdentityContext _context;

    public ApplicationUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> options,
        IdentityContext context)
        : base(userManager, roleManager, options)
    {
        _context = context;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // Add standard user claims if not already present
        if (!identity.HasClaim(c => c.Type == ClaimTypes.GivenName))
            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty));

        if (!identity.HasClaim(c => c.Type == ClaimTypes.Surname))
            identity.AddClaim(new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty));

        // Get user's active tenant roles
        var userTenantRoles = await _context.UserTenantRoles
            .Where(utr => utr.UserId == user.Id && utr.IsActive)
            .Include(utr => utr.Role)
            .Include(utr => utr.Tenant)
            .ToListAsync();

        // Group user tenant roles by tenant type
        var tenantRolesByType = userTenantRoles
            .GroupBy(utr => utr.Tenant.Type)
            .ToDictionary(g => g.Key, g => g.Select(utr => utr.TenantId.ToString()).ToList());

        // Add tenant claims for each tenant type
        foreach (var tenantType in tenantRolesByType.Keys)
        {
            var claimType = GetClaimTypeForTenantType(tenantType);
            foreach (var tenantId in tenantRolesByType[tenantType])
            {
                identity.AddClaim(new Claim(claimType, tenantId));
            }
        }

        // Add role IDs
        var roleIds = userTenantRoles.Select(utr => utr.RoleId).Distinct().ToList();

        // Get permissions from these roles
        var permissions = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync();

        // Add permission claims
        foreach (var permission in permissions)
        {
            identity.AddClaim(new Claim(IdentityConstant.ClaimTypes.Permission, permission));
        }

        // Add current tenant context claims
        // Note: These would be added during authentication/token generation
        // when a specific tenant context is selected

        return identity;
    }

    private static string GetClaimTypeForTenantType(TenantType tenantType)
    {
        return tenantType switch
        {
            TenantType.Brand => $"{IdentityConstant.TenantType.Brand.ToLower()}_id",
            TenantType.Branch => $"{IdentityConstant.TenantType.Branch.ToLower()}_id",
            TenantType.System => $"{IdentityConstant.TenantType.System.ToLower()}_id",
            TenantType.Department => $"{IdentityConstant.TenantType.Department.ToLower()}_id",
            TenantType.Team => $"{IdentityConstant.TenantType.Team.ToLower()}_id",
            TenantType.Project => $"{IdentityConstant.TenantType.Project.ToLower()}_id",
            TenantType.Custom => $"{IdentityConstant.TenantType.Custom.ToLower()}_id",
            _ => $"{tenantType.ToString().ToLower()}_id"
        };
    }
}
