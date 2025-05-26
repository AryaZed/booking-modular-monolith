using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Authorization;
using BuildingBlocks.Constants;
using BuildingBlocks.Identity;
using BuildingBlocks.Web;
using Identity.Identity.Features.RegisterNewUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Identity.Identity.Features.Users.CreateUser;

[Route(BaseApiPath + "/identity/users")]
public class CreateUserEndpoint : BaseController
{
    /// <summary>
    /// Administrative endpoint for creating users
    /// This endpoint is protected by authorization and requires the System.ManageUsers permission
    /// </summary>
    [HttpPost]
    [Authorize]
    [RequirePermission(PermissionsConstant.System.ManageRoles)] // Assuming this permission is appropriate for user management
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Create user (admin)", 
        Description = "Create a new user with optional tenant assignment (requires admin permission)",
        Tags = new[] { "Identity" })]
    public async Task<ActionResult> CreateUser(
        [FromBody] RegisterNewUserCommand command,
        CancellationToken cancellationToken)
    {
        // Get current user ID from claims
        var currentUserId = User.GetUserId();
        if (currentUserId == 0)
        {
            return Unauthorized();
        }
        
        // We reuse RegisterNewUserCommand but process it through the same handler
        var result = await Mediator.Send(command, cancellationToken);

        return Created($"/api/identity/users/{result.Id}", result);
    }
} 