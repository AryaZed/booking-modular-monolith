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

namespace Identity.Identity.Features.Users.RemoveRole;

public class RemoveRoleCommand : ICommand<bool>
{
    public long UserId { get; set; }
    public string RoleName { get; set; }
    public long? TenantId { get; set; }
    
    public class Validator : AbstractValidator<RemoveRoleCommand>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required");
            RuleFor(x => x.RoleName).NotEmpty().WithMessage("Role name is required");
        }
    }
}

public class RemoveRoleCommandHandler : ICommandHandler<RemoveRoleCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IdentityContext _context;
    private readonly IEventDispatcher _eventDispatcher;

    public RemoveRoleCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IdentityContext context,
        IEventDispatcher eventDispatcher)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _eventDispatcher = eventDispatcher;
    }
    
    public async Task<bool> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new NotFoundException("User not found");
            
        var roleExists = await _roleManager.RoleExistsAsync(request.RoleName);
        if (!roleExists)
            throw new NotFoundException($"Role '{request.RoleName}' not found");
            
        // Check if user is in the role
        var isInRole = await _userManager.IsInRoleAsync(user, request.RoleName);
        if (!isInRole)
            return true; // Already not in role, consider it a success
            
        // Remove from role
        var result = await _userManager.RemoveFromRoleAsync(user, request.RoleName);
        
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to remove role: {string.Join(", ", result.Errors)}");
            
        // If this is a tenant-specific role assignment, remove the user-tenant-role record
        if (request.TenantId.HasValue)
        {
            var role = await _roleManager.FindByNameAsync(request.RoleName);
            
            var userTenantRole = await _context.UserTenantRoles
                .FirstOrDefaultAsync(x => 
                    x.UserId == user.Id && 
                    x.RoleId == role.Id && 
                    x.TenantId == request.TenantId.Value,
                    cancellationToken);
                    
            if (userTenantRole != null)
            {
                _context.UserTenantRoles.Remove(userTenantRole);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        
        // Publish UserRoleChangedEvent
        await _eventDispatcher.DispatchAsync(new UserRoleChangedEvent
        {
            UserId = request.UserId,
            Email = user.Email,
            RoleId = role.Id,
            RoleName = role.Name,
            TenantId = request.TenantId,
            TenantType = userTenantRole?.TenantType,
            IsRoleAdded = false,
            ChangedAt = DateTime.UtcNow
        });

        return true;
    }
} 