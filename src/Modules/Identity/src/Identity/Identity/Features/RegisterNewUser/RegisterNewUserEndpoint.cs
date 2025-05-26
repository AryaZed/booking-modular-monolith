using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace Identity.Identity.Features.RegisterNewUser;

[Route(BaseApiPath + "/identity/register")]
public class RegisterNewUserEndpoint : BaseController
{
    /// <summary>
    /// Public endpoint for user self-registration
    /// This endpoint is intentionally not protected by [Authorize] attribute
    /// since it's used by unauthenticated users to create accounts
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [EnableRateLimiting("registration")] // Apply rate limiting to prevent abuse
    [SwaggerOperation(
        Summary = "Register new user", 
        Description = "Register a new user in the system (public endpoint)",
        Tags = new[] { "Identity" })]
    public async Task<ActionResult> RegisterNewUser(
        [FromBody] RegisterNewUserCommand command,
        CancellationToken cancellationToken)
    {
        // Ensure tenant-related properties are null for public registration
        command.TenantId = null;
        command.TenantType = null;
        command.RoleId = null;
        
        var result = await Mediator.Send(command, cancellationToken);

        return Created($"/api/identity/users/{result.Id}", result);
    }
}
