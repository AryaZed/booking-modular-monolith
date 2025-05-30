using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using BuildingBlocks.Constants;
using BuildingBlocks.Domain;
using FluentValidation;
using Identity.Data;
using Identity.Identity.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Exception;

namespace Identity.Identity.Features.Users.AssignRole;

public class AssignRoleCommand : ICommand<bool>
{
    public long UserId { get; set; }
    public string RoleName { get; set; }
    public long? TenantId { get; set; }

    // Validator
    public class Validator : AbstractValidator<AssignRoleCommand>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required");
            RuleFor(x => x.RoleName).NotEmpty().WithMessage("Role name is required");
        }
    }
}

public class AssignRoleCommandHandler : ICommandHandler<AssignRoleCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IdentityContext _context;

    public AssignRoleCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IdentityContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<bool> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new NotFoundException("User not found");

        var roleExists = await _roleManager.RoleExistsAsync(request.RoleName);
        if (!roleExists)
            throw new NotFoundException($"Role '{request.RoleName}' not found");

        var result = await _userManager.AddToRoleAsync(user, request.RoleName);

        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to assign role: {string.Join(", ", result.Errors)}");

        // If this is a tenant-specific role assignment, create the user-tenant-role record
        if (request.TenantId.HasValue)
        {
            var tenant = await _context.Tenants.FindAsync(request.TenantId.Value);
            if (tenant == null)
                throw new NotFoundException($"Tenant with ID {request.TenantId} not found");

            var role = await _roleManager.FindByNameAsync(request.RoleName);

            var userTenantRole = UserTenantRole.Create(
                user.Id,
                tenant.Id,
                role.Id,
                1); // TODO: Get actual current user ID

            _context.UserTenantRoles.Add(userTenantRole);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
