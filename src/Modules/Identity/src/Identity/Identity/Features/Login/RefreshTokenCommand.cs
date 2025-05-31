using FluentValidation;
using MediatR;

namespace Identity.Identity.Features.Login;

public record RefreshTokenCommand : IRequest<LoginResponse>
{
    public string RefreshToken { get; init; }
}

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
} 