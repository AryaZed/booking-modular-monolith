using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.Security;
using FluentValidation;
using Identity.Data;
using Identity.Identity.Exceptions;
using Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Identity.Features.Users.RequestPasswordReset;

public record RequestPasswordResetCommand(string Email) : ICommand;

public class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email address is required");
    }
}

public class RequestPasswordResetCommandHandler : ICommandHandler<RequestPasswordResetCommand>
{
    private readonly UserManager<Models.ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RequestPasswordResetCommandHandler> _logger;

    public RequestPasswordResetCommandHandler(
        UserManager<Models.ApplicationUser> userManager,
        IEmailService emailService,
        ITokenService tokenService,
        ILogger<RequestPasswordResetCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Unit> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        // We don't want to reveal if a user exists or not for security reasons
        if (user == null)
        {
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
            return Unit.Value;
        }

        // Generate password reset token
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        // Create a secure token for email
        var encodedToken = _tokenService.EncodeToken(token);

        // Send email with reset link
        await _emailService.SendPasswordResetEmailAsync(
            user.Email, 
            user.UserName, 
            encodedToken, 
            user.Id);

        return Unit.Value;
    }
} 