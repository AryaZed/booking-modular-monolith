using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using FluentValidation;
using Identity.Identity.Exceptions;
using Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Identity.Identity.Features.Users.ResetPassword;

public record ResetPasswordCommand(string UserId, string Token, string NewPassword, string ConfirmPassword) : ICommand;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
            
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required");
            
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least 1 uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least 1 lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least 1 number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least 1 special character");
            
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }
}

public class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand>
{
    private readonly UserManager<Models.ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        UserManager<Models.ApplicationUser> userManager,
        ITokenService tokenService,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            _logger.LogWarning("Password reset attempted for non-existent user ID: {UserId}", request.UserId);
            throw new IdentityException("Invalid user ID or token");
        }

        // Decode the token
        var decodedToken = _tokenService.DecodeToken(request.Token);
        
        // Reset the password
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Password reset failed for user {UserId}: {Errors}", user.Id, errors);
            throw new IdentityException($"Password reset failed: {errors}");
        }

        _logger.LogInformation("Password reset successful for user {UserId}", user.Id);
        return Unit.Value;
    }
} 