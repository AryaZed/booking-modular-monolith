using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain;
using Identity.Data;
using Identity.Identity.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Identity.Features.Users.RemoveRole;

public record RemoveRoleCommand : ICommand<bool>
{
    public long UserId { get; init; }
    public long RoleId { get; init; }
    public long TenantId { get; init; }
}

public class RemoveRoleCommandHandler : ICommandHandler<RemoveRoleCommand, bool>
{
    private readonly IdentityContext _context;
    private readonly IEventDispatcher _eventDispatcher;

    public RemoveRoleCommandHandler(IdentityContext context, IEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<bool> Handle(RemoveRoleCommand command, CancellationToken cancellationToken)
    {
        var userTenantRole = await _context.UserTenantRoles
            .Include(utr => utr.Role)
            .Include(utr => utr.User)
            .FirstOrDefaultAsync(utr => 
                utr.UserId == command.UserId && 
                utr.RoleId == command.RoleId && 
                utr.TenantId == command.TenantId, 
                cancellationToken);
                
        if (userTenantRole == null)
        {
            throw new NotFoundException("Role assignment not found");
        }

        // Check if this is the user's last role
        var userRoleCount = await _context.UserTenantRoles.CountAsync(
            utr => utr.UserId == command.UserId, cancellationToken);
            
        if (userRoleCount <= 1)
        {
            throw new ValidationException("Cannot remove the last role from a user");
        }

        // Remove the role
        _context.UserTenantRoles.Remove(userTenantRole);
        await _context.SaveChangesAsync(cancellationToken);

        // Publish UserRoleChangedEvent
        await _eventDispatcher.DispatchAsync(new UserRoleChangedEvent
        {
            UserId = command.UserId,
            Email = userTenantRole.User.Email,
            RoleId = command.RoleId,
            RoleName = userTenantRole.Role.Name,
            TenantId = command.TenantId,
            TenantType = userTenantRole.TenantType,
            IsRoleAdded = false,
            ChangedAt = DateTime.UtcNow
        });

        return true;
    }
} 