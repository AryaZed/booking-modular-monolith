using BuildingBlocks.Authorization;
using BuildingBlocks.Constants;
using BuildingBlocks.Identity;
using BuildingBlocks.Web;
using Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Identity.Features.Users.AssignUserToTenant;

[Route(BaseApiPath + "/identity/tenants/{tenantId}/users")]
public class AssignUserToTenantEndpoint : BaseController
{
    private readonly IUserTenantService _userTenantService;
    
    public AssignUserToTenantEndpoint(IUserTenantService userTenantService)
    {
        _userTenantService = userTenantService;
    }
    
    [HttpPost("{userId}/roles")]
    [Authorize]
    [RequirePermission(PermissionsConstant.RoleManagement.AssignRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Assign user to tenant with role", Description = "Assigns a user to a tenant with a specific role (requires RoleManagement.AssignRole permission)")]
    public async Task<ActionResult> AssignUserToTenant(
        [FromRoute] long tenantId,
        [FromRoute] long userId,
        [FromBody] AssignUserToTenantCommand command,
        CancellationToken cancellationToken)
    {
        // Get current user ID using our extension method
        var currentUserId = User.GetUserId();
        if (currentUserId == 0)
        {
            return Unauthorized();
        }
        
        // Add parameters from route
        command.TenantId = tenantId;
        command.UserId = userId;
        
        var userTenantRole = await _userTenantService.AssignUserToTenantAsync(
            userId,
            tenantId,
            command.TenantType,
            command.RoleId,
            currentUserId);
            
        return Ok(new AssignUserToTenantResponse
        {
            Id = userTenantRole.Id,
            TenantId = userTenantRole.TenantId,
            TenantType = userTenantRole.TenantType,
            RoleId = userTenantRole.RoleId
        });
    }
} 