using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Authorization;
using BuildingBlocks.Constants;
using BuildingBlocks.Identity;
using BuildingBlocks.Web;
using Identity.Identity.Features.RegisterNewUser;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Identity.Identity.Features.Users.CreateBrandUser;

[Route(BaseApiPath + "/identity/brands/{brandId}/users")]
public class CreateBrandUserEndpoint : BaseController
{
    [HttpPost]
    [Authorize]
    [RequirePermission(PermissionsConstant.Brands.ManageBrandUsers)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Create brand user",
        Description = "Create a new user for a specific brand (requires Brand.ManageBrandUsers permission)",
        Tags = new[] { "Identity" })]
    public async Task<ActionResult> CreateBrandUser(
        [FromRoute] long brandId,
        [FromBody] RegisterNewUserCommand command,
        CancellationToken cancellationToken)
    {
        // Get current user ID
        var currentUserId = User.GetUserId();
        if (currentUserId == 0)
        {
            return Unauthorized();
        }
        
        // Verify the user has access to this brand
        if (!User.IsSystemAdmin() && !User.BelongsToTenant(brandId))
        {
            return Forbid("You don't have permission to create users for this brand");
        }

        // Validate the user can manage brand users
        if (!User.CanManageBrandUsers())
        {
            return Forbid("You don't have permission to manage brand users");
        }

        // Set brand-specific properties
        command.TenantId = brandId;
        command.TenantType = IdentityConstant.TenantType.Brand;
        
        // We reuse RegisterNewUserCommand but process it through the same handler
        var result = await Mediator.Send(command, cancellationToken);

        return Created($"/api/identity/users/{result.Id}", result);
    }
} 