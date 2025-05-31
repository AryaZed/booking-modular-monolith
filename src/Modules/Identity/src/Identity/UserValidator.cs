using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Constants;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Identity.Data;
using Identity.Identity.Models;
using Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity;

public class UserValidator : IResourceOwnerPasswordValidator
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityContext _context;
    private readonly ITokenService _tokenService;

    public UserValidator(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IdentityContext context,
        ITokenService tokenService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
        _tokenService = tokenService;
    }

    public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        var user = await _userManager.FindByNameAsync(context.UserName);
        if (user == null)
        {
            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidGrant, "Invalid username or password");
            return;
        }

        if (!user.IsActive)
        {
            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidGrant, "User account is inactive");
            return;
        }

        var signIn = await _signInManager.PasswordSignInAsync(
            user,
            context.Password,
            isPersistent: true,
            lockoutOnFailure: true);

        if (signIn.Succeeded)
        {
            var userId = user.Id.ToString();

            // Get the default tenant for this user (if any)
            var defaultUserTenantRole = await _context.UserTenantRoles
                .Include(utr => utr.Tenant)
                .Include(utr => utr.Role)
                .FirstOrDefaultAsync(utr => utr.UserId == user.Id && utr.IsDefault);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(IdentityConstant.ClaimTypes.UserId, userId)
            };

            // Add name claims if available
            if (!string.IsNullOrEmpty(user.FirstName))
                claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));

            if (!string.IsNullOrEmpty(user.LastName))
                claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

            // Add tenant context if available
            if (defaultUserTenantRole != null)
            {
                claims.Add(new Claim(IdentityConstant.ClaimTypes.TenantId, defaultUserTenantRole.TenantId.ToString()));
                claims.Add(new Claim(IdentityConstant.ClaimTypes.TenantType, defaultUserTenantRole.Tenant.TenantType));
                claims.Add(new Claim(ClaimTypes.Role, defaultUserTenantRole.Role.Name));

                // Get permissions for this role
                var permissions = await _tokenService.GetPermissionsForRoleAsync(defaultUserTenantRole.RoleId);
                foreach (var permission in permissions)
                {
                    claims.Add(new Claim(IdentityConstant.ClaimTypes.Permission, permission));
                }
            }

            // context set to success
            context.Result = new GrantValidationResult(
                subject: userId,
                authenticationMethod: "password",
                claims: claims
            );

            return;
        }

        // context set to Failure
        context.Result = new GrantValidationResult(
            TokenRequestErrors.InvalidGrant, "Invalid username or password");
    }
}
