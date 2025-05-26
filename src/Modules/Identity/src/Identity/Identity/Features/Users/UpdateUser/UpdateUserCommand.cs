using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain;
using BuildingBlocks.Identity;
using FluentValidation;
using Identity.Data;
using Identity.Identity.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Identity.Features.Users.UpdateUser;

public record UpdateUserCommand : ICommand<UpdateUserResponse>
{
    public long UserId { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public string PhoneNumber { get; init; }
    public bool IsActive { get; init; }
}

public record UpdateUserResponse
{
    public long Id { get; init; }
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string PhoneNumber { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator(IdentityContext dbContext)
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("Invalid user ID");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(async (cmd, email, ct) => 
                !await dbContext.Users.AnyAsync(u => u.Email == email && u.Id != cmd.UserId, ct))
            .WithMessage("Email is already in use by another user");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20);
    }
}

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly IdentityContext _context;

    public UpdateUserCommandHandler(IdentityContext context)
    {
        _context = context;
    }

    public async Task<UpdateUserResponse> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {command.UserId} not found");
        }

        // Update user properties
        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.Email = command.Email;
        user.PhoneNumber = command.PhoneNumber ?? user.PhoneNumber;
        user.IsActive = command.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive
        };
    }
} 