using BuildingBlocks.Validation;
using FluentValidation;
using MediatR;

namespace Identity.Identity.Features.Login;

public record LoginCommand : IRequest<LoginResponse>
{
    public string Email { get; init; }
    public string Password { get; init; }
    public bool RememberMe { get; init; }
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email address is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
} 