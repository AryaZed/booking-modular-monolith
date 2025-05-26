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

namespace Identity.Identity.Features.Roles.CreateRole;

[Route(BaseApiPath + "/identity/tenants/{tenantId}/roles")]
public class CreateRoleEndpoint : BaseController
{
    private readonly ITenantRoleService _tenantRoleService;
    
    public CreateRoleEndpoint(ITenantRoleService tenantRoleService)
    {
        _tenantRoleService = tenantRoleService;
    }
    
    [HttpPost]
    [Authorize]
    [RequirePermission(PermissionsConstant.RoleManagement.CreateRole)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Create a new role", Description = "Creates a new role for a tenant (requires RoleManagement.CreateRole permission)")]
    public async Task<ActionResult> CreateRole(
        [FromRoute] long tenantId,
        [FromBody] CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        // Get current user ID using our extension method
        var userId = User.GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }
        
        // Add tenantId to command
        command.TenantId = tenantId;
        
        var role = await _tenantRoleService.CreateRoleAsync(
            tenantId,
            command.Name,
            command.Description,
            command.Permissions,
            userId);
            
        return Created($"/api/identity/roles/{role.Id}", new CreateRoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description
        });
    }
} 