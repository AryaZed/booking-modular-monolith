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

namespace Identity.Identity.Features.Users.DeleteUser;

[Route(BaseApiPath + "/identity/users")]
public class DeleteUserEndpoint : BaseController
{
    private readonly IdentityContext _context;

    public DeleteUserEndpoint(IdentityContext context)
    {
        _context = context;
    }

    [HttpDelete("{userId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Delete user",
        Description = "Delete a user (requires appropriate tenant permissions)",
        Tags = new[] { "Identity" })]
    public async Task<ActionResult> DeleteUser(
        [FromRoute] long userId,
        CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = User.GetUserId();
        if (currentUserId == 0)
        {
            return Unauthorized();
        }
        
        // Prevent self-deletion
        if (userId == currentUserId)
        {
            return BadRequest("You cannot delete your own account");
        }
        
        // Find the user to delete
        var userToDelete = await _context.Users
            .Include(u => u.UserTenantRoles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            
        if (userToDelete == null)
        {
            return NotFound("User not found");
        }
        
        // Check if current user has permission to delete this user
        bool hasPermission = false;
        
        // System admins can delete any user
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
                return Forbid("You don't have permission to delete users");
            }
            
            // Check if the user to delete belongs to a tenant managed by current user
            foreach (var userTenantRole in userToDelete.UserTenantRoles)
            {
                // Brand admins can delete users in their brand or branches under their brand
                if (currentUserTenantType == IdentityConstant.TenantType.Brand && 
                    User.HasPermission(PermissionsConstant.Brands.ManageBrandUsers))
                {
                    if (userTenantRole.TenantId == currentUserTenantId) // Same brand
                    {
                        hasPermission = true;
                        break;
                    }
                    
                    // Check if user belongs to a branch under current user's brand
                    if (userTenantRole.TenantType == IdentityConstant.TenantType.Branch)
                    {
                        var branch = await _context.Branches
                            .FirstOrDefaultAsync(b => b.Id == userTenantRole.TenantId, cancellationToken);
                            
                        if (branch != null && branch.BrandId == currentUserTenantId)
                        {
                            hasPermission = true;
                            break;
                        }
                    }
                }
                
                // Branch admins can only delete users in their branch
                else if (currentUserTenantType == IdentityConstant.TenantType.Branch &&
                        User.HasPermission(PermissionsConstant.Branches.ManageBranchUsers) &&
                        userTenantRole.TenantType == IdentityConstant.TenantType.Branch &&
                        userTenantRole.TenantId == currentUserTenantId)
                {
                    hasPermission = true;
                    break;
                }
            }
        }
        
        if (!hasPermission)
        {
            return Forbid("You don't have permission to delete this user");
        }
        
        // Send delete command
        await Mediator.Send(new DeleteUserCommand { UserId = userId }, cancellationToken);
        
        return NoContent();
    }
} 