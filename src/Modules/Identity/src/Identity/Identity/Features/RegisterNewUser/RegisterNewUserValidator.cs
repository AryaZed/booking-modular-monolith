using FluentValidation;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Identity.Features.RegisterNewUser;

public class RegisterNewUserValidator : AbstractValidator<RegisterNewUserCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterNewUserValidator(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
        
        CascadeMode = CascadeMode.Stop;

        // Password validation
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
            
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.Password).WithMessage("Passwords do not match");
        
        // Basic user info validation
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters")
            .MustAsync(BeUniqueUsername).WithMessage("Username is already taken");
            
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email address is required")
            .MustAsync(BeUniqueEmail).WithMessage("Email is already registered");
            
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");
            
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");
            
        // Optional tenant assignment validation
        When(x => x.TenantId.HasValue, () => {
            RuleFor(x => x.TenantType)
                .NotNull().WithMessage("Tenant type is required when tenant ID is provided");
            
            RuleFor(x => x.RoleId)
                .NotNull().WithMessage("Role ID is required when tenant ID is provided");
        });
    }
    
    private async Task<bool> BeUniqueUsername(string username, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(username);
        return user == null;
    }
    
    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user == null;
    }
}
