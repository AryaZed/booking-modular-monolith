using BuildingBlocks.Authorization;
using BuildingBlocks.Constants;
using BuildingBlocks.Web;
using Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Identity.Features.Roles.GetRolePermissions;

[Route(BaseApiPath + "/identity/roles")]
public class GetRolePermissionsEndpoint : BaseController
{
    private readonly ITenantRoleService _tenantRoleService;
    
    public GetRolePermissionsEndpoint(ITenantRoleService tenantRoleService)
    {
        _tenantRoleService = tenantRoleService;
    }
    
    [HttpGet("{roleId}/permissions")]
    [Authorize]
    [RequirePermission(PermissionsConstant.RoleManagement.EditRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Get role permissions", Description = "Returns the permissions assigned to a role (requires RoleManagement.EditRole permission)")]
    public async Task<ActionResult> GetRolePermissions(
        [FromRoute] long roleId,
        CancellationToken cancellationToken)
    {
        var permissions = await _tenantRoleService.GetPermissionsForRoleAsync(roleId);
        return Ok(new GetRolePermissionsResponse { Permissions = permissions });
    }
} 