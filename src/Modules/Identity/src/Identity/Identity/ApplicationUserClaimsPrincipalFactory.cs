using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
            .ToListAsync();
            
        // Group by tenant type for claims
        var brandIds = userTenantRoles
            .Where(utr => utr.TenantType == TenantType.Brand)
            .Select(utr => utr.TenantId.ToString())
            .ToList();
            
        var branchIds = userTenantRoles
            .Where(utr => utr.TenantType == TenantType.Branch)
            .Select(utr => utr.TenantId.ToString())
            .ToList();
            
        // Add tenant claims
        foreach (var brandId in brandIds)
        {
            identity.AddClaim(new Claim("brand_id", brandId));
        }
        
        foreach (var branchId in branchIds)
        {
            identity.AddClaim(new Claim("branch_id", branchId));
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
            identity.AddClaim(new Claim("permission", permission));
        }
        
        // Add current tenant context claims
        // Note: These would be added during authentication/token generation
        // when a specific tenant context is selected
        
        return identity;
    }
} 