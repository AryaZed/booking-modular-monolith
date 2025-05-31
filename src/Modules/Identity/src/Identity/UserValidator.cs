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

        var signIn = await _signInManager.PasswordSignInAsync(
            user,
            context.Password,
            isPersistent: true,
            lockoutOnFailure: true);

        if (signIn.Succeeded)
        {
            var userId = user!.Id.ToString();

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
