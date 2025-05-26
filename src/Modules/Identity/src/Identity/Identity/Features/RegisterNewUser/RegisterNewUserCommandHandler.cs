using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain;
using BuildingBlocks.Identity;
using FluentValidation;
using Identity.Data;
using Identity.Identity.Events;
using Identity.Identity.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Identity.Features.RegisterNewUser;

public class RegisterNewUserCommandHandler : IRequestHandler<RegisterNewUserCommand, RegisterNewUserResponse>
{
    private readonly IdentityContext _context;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly IEventDispatcher _eventDispatcher;

    public RegisterNewUserCommandHandler(
        IdentityContext context, 
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEventDispatcher eventDispatcher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<RegisterNewUserResponse> Handle(RegisterNewUserCommand command, CancellationToken cancellationToken)
    {
        // Check if user with same email already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);
            
        if (existingUser != null)
        {
            throw new ValidationException("A user with this email already exists");
        }

        // Create new user
        var user = new ApplicationUser
        {
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            PhoneNumber = command.PhoneNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Hash password
        user.PasswordHash = _passwordHasher.HashPassword(user, command.Password);

        // Add user to database
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // If tenant information is provided, assign user to tenant with specified role
        if (command.TenantId.HasValue && !string.IsNullOrEmpty(command.TenantType) && command.RoleId.HasValue)
        {
            var role = await _context.Roles.FindAsync(new object[] { command.RoleId.Value }, cancellationToken);
            if (role != null)
            {
                var userTenantRole = new UserTenantRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    TenantId = command.TenantId.Value,
                    TenantType = command.TenantType
                };
                
                await _context.UserTenantRoles.AddAsync(userTenantRole, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        // Publish UserCreatedEvent
        await _eventDispatcher.DispatchAsync(new UserCreatedEvent
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            TenantId = command.TenantId,
            TenantType = command.TenantType,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });

        return new RegisterNewUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }
}
