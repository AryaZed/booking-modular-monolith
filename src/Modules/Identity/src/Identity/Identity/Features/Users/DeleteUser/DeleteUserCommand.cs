using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using BuildingBlocks.Domain;
using FluentValidation;
using Identity.Data;
using Identity.Identity.Events;
using Identity.Identity.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Identity.Features.Users.DeleteUser;

public class DeleteUserCommand : ICommand<bool>
{
    public long UserId { get; set; }
    
    public class Validator : AbstractValidator<DeleteUserCommand>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required");
        }
    }
}

public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityContext _context;
    private readonly IEventDispatcher _eventDispatcher;

    public DeleteUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        IdentityContext context,
        IEventDispatcher eventDispatcher)
    {
        _userManager = userManager;
        _context = context;
        _eventDispatcher = eventDispatcher;
    }
    
    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new NotFoundException("User not found");
            
        // Mark user as deleted rather than actually deleting
        user.SoftDelete(1); // TODO: Get actual current user ID
        
        // Remove from all roles
        var userRoles = await _userManager.GetRolesAsync(user);
        if (userRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, userRoles);
        }
        
        // Remove from all tenants
        var tenantRoles = await _context.UserTenantRoles
            .Where(x => x.UserId == user.Id)
            .ToListAsync(cancellationToken);
            
        if (tenantRoles.Any())
        {
            _context.UserTenantRoles.RemoveRange(tenantRoles);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        
        // Publish UserDeletedEvent
        await _eventDispatcher.DispatchAsync(new UserDeletedEvent
        {
            UserId = request.UserId,
            DeletedAt = DateTime.UtcNow
        });

        return true;
    }
} 