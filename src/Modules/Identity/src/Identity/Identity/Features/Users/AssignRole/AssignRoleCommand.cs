using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Constants;
using BuildingBlocks.Domain;
using FluentValidation;
using Identity.Data;
using Identity.Identity.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Identity.Features.Users.AssignRole;

public record AssignRoleCommand : ICommand<AssignRoleResponse>
{
    public long UserId { get; init; }
    public long RoleId { get; init; }
    public long TenantId { get; init; }
    public string TenantType { get; init; }
}

public record AssignRoleResponse
{
    public long UserId { get; init; }
    public long RoleId { get; init; }
    public long TenantId { get; init; }
    public string TenantType { get; init; }
    public string RoleName { get; init; }
}

public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator(IdentityContext dbContext)
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .MustAsync(async (id, ct) => await dbContext.Users.AnyAsync(u => u.Id == id, ct))
            .WithMessage("User not found");

        RuleFor(x => x.RoleId)
            .GreaterThan(0)
            .MustAsync(async (id, ct) => await dbContext.Roles.AnyAsync(r => r.Id == id, ct))
            .WithMessage("Role not found");

        RuleFor(x => x.TenantId)
            .GreaterThan(0)
            .WithMessage("Invalid tenant ID");

        RuleFor(x => x.TenantType)
            .NotEmpty()
            .Must(type => 
                type == IdentityConstant.TenantType.System || 
                type == IdentityConstant.TenantType.Brand || 
                type == IdentityConstant.TenantType.Branch)
            .WithMessage("Invalid tenant type. Must be System, Brand, or Branch");

        // Validate that the combination does not already exist
        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => !await dbContext.UserTenantRoles.AnyAsync(
                u => u.UserId == cmd.UserId && 
                     u.RoleId == cmd.RoleId && 
                     u.TenantId == cmd.TenantId && 
                     u.TenantType == cmd.TenantType, ct))
            .WithMessage("User already has this role in the specified tenant");
    }
}

public class AssignRoleCommandHandler : ICommandHandler<AssignRoleCommand, AssignRoleResponse>
{
    private readonly IdentityContext _context;

    public AssignRoleCommandHandler(IdentityContext context)
    {
        _context = context;
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