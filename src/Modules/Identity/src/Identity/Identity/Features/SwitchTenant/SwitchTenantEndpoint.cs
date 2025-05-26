using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Web;
using Identity.Data;
using Identity.Identity.Dtos;
using Identity.Identity.Models;
using Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Identity.Identity.Features.SwitchTenant;

[Route(BaseApiPath + "/identity/tenants")]
public class SwitchTenantEndpoint : BaseController
{
    private readonly IdentityContext _context;
    private readonly ITokenService _tokenService;
    
    public SwitchTenantEndpoint(IdentityContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }
    
    [HttpPost("switch")]
    [Authorize]
    [ProducesResponseType(typeof(SwitchTenantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Switch tenant context", 
        Description = "Changes the user's active tenant context and returns a new access token",
        Tags = new[] { "Tenant Management" })]
    public async Task<ActionResult<SwitchTenantResponse>> SwitchTenant(
        [FromBody] SwitchTenantCommand command,
        CancellationToken cancellationToken)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        
        // Verify the user has access to the requested tenant
        var userTenantRole = await _context.UserTenantRoles
            .Where(utr => utr.UserId == userId && 
                   utr.TenantId == command.TenantId && 
                   utr.TenantType == command.TenantType &&
                   utr.IsActive)
            .Include(utr => utr.Role)
            .FirstOrDefaultAsync();
            
        if (userTenantRole == null)
        {
            return Forbid();
        }
        
        // Get permissions for this role
        var permissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == userTenantRole.RoleId)
            .Select(rp => rp.Permission)
            .ToListAsync();
            
        // Generate a new token with the different tenant context
        var user = await _context.Users.FindAsync(userId);
        var token = _tokenService.GenerateToken(
            user.Id.ToString(),
            user.UserName,
            user.Email,
            userTenantRole.TenantId.ToString(),
            userTenantRole.TenantType.ToString(),
            userTenantRole.Role.Name,
            permissions);
            
        return Ok(new SwitchTenantResponse
        {
            AccessToken = token,
            CurrentTenant = new TenantContext
            {
                TenantId = userTenantRole.TenantId,
                TenantType = userTenantRole.TenantType,
                RoleName = userTenantRole.Role.Name
            }
        });
    }
} 