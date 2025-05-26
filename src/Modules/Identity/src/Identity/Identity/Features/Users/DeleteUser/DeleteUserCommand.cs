using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain;
using Identity.Data;
using Identity.Identity.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Identity.Features.Users.DeleteUser;

public record DeleteUserCommand : ICommand<bool>
{
    public long UserId { get; init; }
}

public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand, bool>
{
    private readonly IdentityContext _context;
    private readonly IEventDispatcher _eventDispatcher;

    public DeleteUserCommandHandler(IdentityContext context, IEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<bool> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserTenantRoles)
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
            
        if (user == null)
        {
            throw new NotFoundException($"User with ID {command.UserId} not found");
        }

        // Delete user tenant roles first
        _context.UserTenantRoles.RemoveRange(user.UserTenantRoles);
        
        // Delete user
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Publish UserDeletedEvent
        await _eventDispatcher.DispatchAsync(new UserDeletedEvent
        {
            UserId = command.UserId,
            DeletedAt = DateTime.UtcNow
        });

        return true;
    }
} 