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

namespace Identity.Identity.Features.Users.RemoveRole;

[Route(BaseApiPath + "/identity/users")]
public class RemoveRoleEndpoint : BaseController
{
    private readonly IdentityContext _context;

    public RemoveRoleEndpoint(IdentityContext context)
    {
        _context = context;
    }

    [HttpDelete("{userId}/roles/{roleId}")]
    [Authorize]
    [RequirePermission(PermissionsConstant.RoleManagement.AssignRole)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Remove role from user",
        Description = "Remove a role from a user within a specific tenant (requires RoleManagement.AssignRole permission)",
        Tags = new[] { "Identity" })]
    public async Task<ActionResult> RemoveRole(
        [FromRoute] long userId,
        [FromRoute] long roleId,
        [FromQuery] long tenantId,
        CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = User.GetUserId();
        if (currentUserId == 0)
        {
            return Unauthorized();
        }
        
        // Find the user role assignment
        var userTenantRole = await _context.UserTenantRoles
            .FirstOrDefaultAsync(utr => 
                utr.UserId == userId && 
                utr.RoleId == roleId && 
                utr.TenantId == tenantId, 
                cancellationToken);
                
        if (userTenantRole == null)
        {
            return NotFound("Role assignment not found");
        }
        
        // Check if current user has permission to remove roles in this tenant
        bool hasPermission = false;
        
        // System admins can remove any role
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
                return Forbid("You don't have permission to remove roles");
            }
            
            // Brand admins can remove roles in their brand or in branches under their brand
            if (currentUserTenantType == IdentityConstant.TenantType.Brand)
            {
                if (userTenantRole.TenantId == currentUserTenantId && 
                    userTenantRole.TenantType == IdentityConstant.TenantType.Brand)
                {
                    // Can remove roles in own brand
                    hasPermission = true;
                }
                else if (userTenantRole.TenantType == IdentityConstant.TenantType.Branch)
                {
                    // Check if branch belongs to this brand
                    var branch = await _context.Branches
                        .FirstOrDefaultAsync(b => b.Id == userTenantRole.TenantId, cancellationToken);
                        
                    if (branch != null && branch.BrandId == currentUserTenantId)
                    {
                        hasPermission = true;
                    }
                }
            }
            
            // Branch admins can only remove roles in their branch
            else if (currentUserTenantType == IdentityConstant.TenantType.Branch &&
                    userTenantRole.TenantType == IdentityConstant.TenantType.Branch &&
                    userTenantRole.TenantId == currentUserTenantId)
            {
                hasPermission = true;
            }
        }
        
        if (!hasPermission)
        {
            return Forbid("You don't have permission to remove roles in this tenant");
        }
        
        try
        {
            // Send the command to remove the role
            await Mediator.Send(new RemoveRoleCommand
            {
                UserId = userId,
                RoleId = roleId,
                TenantId = tenantId
            }, cancellationToken);
            
            return NoContent();
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
} 