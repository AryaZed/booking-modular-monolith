using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Constants;
using BuildingBlocks.Domain;
using Identity.Data;
using Identity.Identity.Events;
using Identity.Identity.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Identity.Features.Users.AssignRole;

public class AssignRoleCommandHandler : ICommandHandler<AssignRoleCommand, AssignRoleResponse>
{
    private readonly IdentityContext _context;
    private readonly IEventDispatcher _eventDispatcher;

    public AssignRoleCommandHandler(IdentityContext context, IEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<AssignRoleResponse> Handle(AssignRoleCommand command, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == command.RoleId, cancellationToken);
            
        if (role == null)
        {
            throw new NotFoundException($"Role with ID {command.RoleId} not found");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
            
        if (user == null)
        {
            throw new NotFoundException($"User with ID {command.UserId} not found");
        }

        // Create new user tenant role
        var userTenantRole = new UserTenantRole
        {
            UserId = command.UserId,
            RoleId = command.RoleId,
            TenantId = command.TenantId,
            TenantType = command.TenantType
        };

        await _context.UserTenantRoles.AddAsync(userTenantRole, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Publish UserRoleChangedEvent
        await _eventDispatcher.DispatchAsync(new UserRoleChangedEvent
        {
            UserId = command.UserId,
            Email = user.Email,
            RoleId = command.RoleId,
            RoleName = role.Name,
            TenantId = command.TenantId,
            TenantType = command.TenantType,
            IsRoleAdded = true,
            ChangedAt = DateTime.UtcNow
        });

        return new AssignRoleResponse
        {
            UserId = command.UserId,
            RoleId = command.RoleId,
            TenantId = command.TenantId,
            TenantType = command.TenantType,
            RoleName = role.Name
        };
    }
} 