using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Authorization;
using BuildingBlocks.Constants;
using BuildingBlocks.Identity;
using BuildingBlocks.Web;
using Identity.Data;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Identity.Identity.Features.Users.UpdateUser;

[Route(BaseApiPath + "/identity/users")]
public class UpdateUserEndpoint : BaseController
{
    private readonly IdentityContext _context;

    public UpdateUserEndpoint(IdentityContext context)
    {
        _context = context;
    }

    [HttpPut("{userId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Update user",
        Description = "Update user information (requires appropriate tenant permissions)",
        Tags = new[] { "Identity" })]
    public async Task<ActionResult<UpdateUserResponse>> UpdateUser(
        [FromRoute] long userId,
        [FromBody] UpdateUserCommand command,
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

        // Find the user to update
        var userToUpdate = await _context.Users
            .Include(u => u.TenantRoles)
            .ThenInclude(tr => tr.Tenant)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (userToUpdate == null)
        {
            return NotFound("User not found");
        }

        // Check if current user has permission to update this user
        bool hasPermission = false;

        // Users can update their own basic information
        if (userId == currentUserId)
        {
            // Don't allow self-deactivation
            if (!command.IsActive)
            {
                return BadRequest("You cannot deactivate your own account");
            }

            hasPermission = true;
        }
        // System admins can update any user
        else if (User.IsSystemAdmin())
        {
            hasPermission = true;
        }
        else
        {
            var currentUserTenantId = User.GetTenantId();
            var currentUserTenantType = User.GetTenantType();

            if (!currentUserTenantId.HasValue)
            {
                return Forbid("You don't have permission to update users");
            }

            // Check if the user to update belongs to a tenant managed by current user
            foreach (var userTenantRole in userToUpdate.TenantRoles)
            {
                // Brand admins can update users in their brand or branches under their brand
                if (currentUserTenantType == IdentityConstant.TenantType.Brand &&
                    BuildingBlocks.Identity.ClaimsPrincipalExtensions.HasPermission(User, PermissionsConstant.Brands.ManageBrandUsers))
                {
                    if (userTenantRole.TenantId == currentUserTenantId) // Same brand
                    {
                        hasPermission = true;
                        break;
                    }

                    // Check if user belongs to a branch under current user's brand
                    if (userTenantRole.Tenant != null && userTenantRole.Tenant.Type == TenantType.Branch)
                    {
                        // Since we don't have direct access to Branches DbSet, 
                        // we would need to implement a service or repository to get this information.
                        // For now, we'll skip this branch check

                        // TODO: Implement proper branch-to-brand relationship check using ParentTenantId
                        if (userTenantRole.Tenant.ParentTenantId == currentUserTenantId)
                        {
                            hasPermission = true;
                            break;
                        }
                    }
                }

                // Branch admins can only update users in their branch
                else if (currentUserTenantType == IdentityConstant.TenantType.Branch &&
                        BuildingBlocks.Identity.ClaimsPrincipalExtensions.HasPermission(User, PermissionsConstant.Branches.ManageBranchUsers) &&
                        userTenantRole.Tenant != null &&
                        userTenantRole.Tenant.Type == TenantType.Branch &&
                        userTenantRole.TenantId == currentUserTenantId)
                {
                    hasPermission = true;
                    break;
                }
            }
        }

        if (!hasPermission)
        {
            return Forbid("You don't have permission to update this user");
        }

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
