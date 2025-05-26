using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Authorization;
using BuildingBlocks.Constants;
using BuildingBlocks.Identity;
using BuildingBlocks.Web;
using Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Identity.Identity.Features.Users.AssignRole;

[Route(BaseApiPath + "/identity/users")]
public class AssignRoleEndpoint : BaseController
{
    private readonly IdentityContext _context;

    public AssignRoleEndpoint(IdentityContext context)
    {
        _context = context;
    }

    [HttpPost("{userId}/roles")]
    [Authorize]
    [RequirePermission(PermissionsConstant.RoleManagement.AssignRole)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Assign role to user",
        Description = "Assign a role to a user within a specific tenant (requires RoleManagement.AssignRole permission)",
        Tags = new[] { "Identity" })]
    public async Task<ActionResult<AssignRoleResponse>> AssignRole(
        [FromRoute] long userId,
        [FromBody] AssignRoleCommand command,
        CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = User.GetUserId();
        if (currentUserId == 0)
        {
            return Unauthorized();
        }
        
        // Ensure UserId in route matches command
        command = command with { UserId = userId };
        
        // Check if the user exists
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
        if (!userExists)
        {
            return NotFound("User not found");
        }
        
        // Check if the role exists
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == command.RoleId, cancellationToken);
            
        if (role == null)
        {
            return NotFound("Role not found");
        }
        
        // Check if current user has permission to assign roles in this tenant
        bool hasPermission = false;
        
        // System admins can assign any role
        if (User.IsSystemAdmin())
        {
            hasPermission = true;
        }
        else
        {
            var currentUserTenantId = User.GetTenantId();
            var currentUserTenantType = User.GetTenantType();
            
            if (!currentUserTenantId.HasValue)
            {
                return Forbid("You don't have permission to assign roles");
            }
            
            // Brand admins can assign roles in their brand or in branches under their brand
            if (currentUserTenantType == IdentityConstant.TenantType.Brand)
            {
                if (command.TenantId == currentUserTenantId && 
                    command.TenantType == IdentityConstant.TenantType.Brand)
                {
                    // Can assign roles in own brand
                    hasPermission = true;
                }
                else if (command.TenantType == IdentityConstant.TenantType.Branch)
                {
                    // Check if branch belongs to this brand
                    var branch = await _context.Branches
                        .FirstOrDefaultAsync(b => b.Id == command.TenantId, cancellationToken);
                        
                    if (branch != null && branch.BrandId == currentUserTenantId)
                    {
                        hasPermission = true;
                    }
                }
            }
            
            // Branch admins can only assign roles in their branch
            else if (currentUserTenantType == IdentityConstant.TenantType.Branch &&
                    command.TenantType == IdentityConstant.TenantType.Branch &&
                    command.TenantId == currentUserTenantId)
            {
                hasPermission = true;
            }
        }
        
        if (!hasPermission)
        {
            return Forbid("You don't have permission to assign roles in this tenant");
        }
        
        // Verify role can be assigned at this tenant level
        bool roleAppropriateForTenant = true;
        
        // Check if any permission in the role is not allowed for this tenant type
        if (command.TenantType == IdentityConstant.TenantType.Branch)
        {
            // Check if role has permissions not allowed at branch level
            foreach (var permission in role.RolePermissions)
            {
                if (!PermissionsConstant.BranchLevelPermissions.Contains(permission.Permission))
                {
                    roleAppropriateForTenant = false;
                    break;
                }
            }
        }
        else if (command.TenantType == IdentityConstant.TenantType.Brand)
        {
            // Check if role has permissions not allowed at brand level
            foreach (var permission in role.RolePermissions)
            {
                if (!PermissionsConstant.BrandLevelPermissions.Contains(permission.Permission))
                {
                    roleAppropriateForTenant = false;
                    break;
                }
            }
        }
        
        if (!roleAppropriateForTenant)
        {
            return BadRequest("This role contains permissions that cannot be assigned at this tenant level");
        }
        
        var result = await Mediator.Send(command, cancellationToken);
        return Created($"/api/identity/users/{userId}/roles", result);
    }
} 