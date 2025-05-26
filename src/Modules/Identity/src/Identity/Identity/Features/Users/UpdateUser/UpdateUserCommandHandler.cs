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
using Microsoft.EntityFrameworkCore;

namespace Identity.Identity.Features.Users.UpdateUser;

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly IdentityContext _context;
    private readonly IEventDispatcher _eventDispatcher;

    public UpdateUserCommandHandler(IdentityContext context, IEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
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
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Publish UserUpdatedEvent
        await _eventDispatcher.DispatchAsync(new UserUpdatedEvent
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            UpdatedAt = user.UpdatedAt ?? DateTime.UtcNow
        });

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