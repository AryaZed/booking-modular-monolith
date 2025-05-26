using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Authorization;
using BuildingBlocks.Constants;
using BuildingBlocks.Identity;
using BuildingBlocks.Web;
using Identity.Data;
using Identity.Identity.Features.RegisterNewUser;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Identity.Identity.Features.Users.CreateBranchUser;

[Route(BaseApiPath + "/identity/branches/{branchId}/users")]
public class CreateBranchUserEndpoint : BaseController
{
    private readonly IdentityContext _context;

    public CreateBranchUserEndpoint(IdentityContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Authorize]
    [RequirePermission(PermissionsConstant.Branches.ManageBranchUsers)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Create branch user",
        Description = "Create a new user for a specific branch (requires Branch.ManageBranchUsers permission)",
        Tags = new[] { "Identity" })]
    public async Task<ActionResult> CreateBranchUser(
        [FromRoute] long branchId,
        [FromBody] RegisterNewUserCommand command,
        CancellationToken cancellationToken)
    {
        // Get current user ID
        var currentUserId = User.GetUserId();
        if (currentUserId == 0)
        {
            return Unauthorized();
        }
        
        // Validate the user can manage branch users
        if (!User.CanManageBranchUsers())
        {
            return Forbid("You don't have permission to manage branch users");
        }
        
        // Verify the user has access to this branch
        var userTenantId = User.GetTenantId();
        var userTenantType = User.GetTenantType();
        bool hasAccess = false;
        
        // System admins have access to all branches
        if (User.IsSystemAdmin())
        {
            hasAccess = true;
        }
        // Branch managers can only access their own branch
        else if (userTenantType == IdentityConstant.TenantType.Branch && userTenantId == branchId)
        {
            hasAccess = true;
        }
        // Brand managers can access branches under their brand
        else if (userTenantType == IdentityConstant.TenantType.Brand)
        {
            // Check if branch belongs to the user's brand
            var branch = await _context.Branches
                .FirstOrDefaultAsync(b => b.Id == branchId, cancellationToken);
                
            if (branch != null && branch.BrandId == userTenantId)
            {
                hasAccess = true;
            }
        }
        
        if (!hasAccess)
        {
            return Forbid("You don't have permission to create users for this branch");
        }

        // Set branch-specific properties
        command.TenantId = branchId;
        command.TenantType = IdentityConstant.TenantType.Branch;
        
        // We reuse RegisterNewUserCommand but process it through the same handler
        var result = await Mediator.Send(command, cancellationToken);

        return Created($"/api/identity/users/{result.Id}", result);
    }
} 