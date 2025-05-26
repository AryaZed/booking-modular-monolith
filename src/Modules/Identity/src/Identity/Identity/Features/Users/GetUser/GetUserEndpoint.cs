using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Identity;
using BuildingBlocks.Web;
using Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Identity.Identity.Features.Users.GetUser;

[Route(BaseApiPath + "/identity/users")]
public class GetUserEndpoint : BaseController
{
    private readonly IdentityContext _context;

    public GetUserEndpoint(IdentityContext context)
    {
        _context = context;
    }

    [HttpGet("{userId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Get user details",
        Description = "Get detailed information about a user including roles and tenants",
        Tags = new[] { "Identity" })]
    public async Task<ActionResult<UserDetailsResponse>> GetUser(
        [FromRoute] long userId,
        CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = User.GetUserId();
        if (currentUserId == 0)
        {
            return Unauthorized();
        }
        
        // Users can always see their own details
        bool canAccessDetails = userId == currentUserId;
        
        // Find the user
        var user = await _context.Users
            .Include(u => u.UserTenantRoles)
                .ThenInclude(utr => utr.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            
        if (user == null)
        {
            return NotFound("User not found");
        }
        
        // If not requesting own profile, check permissions
        if (!canAccessDetails)
        {
            // System admins can see any user
            if (User.IsSystemAdmin())
            {
                canAccessDetails = true;
            }
            else
            {
                var currentUserTenantId = User.GetTenantId();
                var currentUserTenantType = User.GetTenantType();
                
                if (currentUserTenantId.HasValue)
                {
                    // Check if the user belongs to a tenant managed by current user
                    foreach (var userTenantRole in user.UserTenantRoles)
                    {
                        // Brand admins can see users in their brand or branches under their brand
                        if (currentUserTenantType == IdentityConstant.TenantType.Brand && 
                            User.IsBrandAdmin())
                        {
                            if (userTenantRole.TenantId == currentUserTenantId && 
                                userTenantRole.TenantType == IdentityConstant.TenantType.Brand)
                            {
                                canAccessDetails = true;
                                break;
                            }
                            
                            if (userTenantRole.TenantType == IdentityConstant.TenantType.Branch)
                            {
                                var branch = await _context.Branches
                                    .FirstOrDefaultAsync(b => b.Id == userTenantRole.TenantId, cancellationToken);
                                    
                                if (branch != null && branch.BrandId == currentUserTenantId)
                                {
                                    canAccessDetails = true;
                                    break;
                                }
                            }
                        }
                        
                        // Branch admins can only see users in their branch
                        else if (currentUserTenantType == IdentityConstant.TenantType.Branch &&
                                User.IsBranchAdmin() &&
                                userTenantRole.TenantType == IdentityConstant.TenantType.Branch &&
                                userTenantRole.TenantId == currentUserTenantId)
                        {
                            canAccessDetails = true;
                            break;
                        }
                    }
                }
            }
        }
        
        if (!canAccessDetails)
        {
            return Forbid("You don't have permission to view this user's details");
        }
        
        // Build response
        var tenantRoles = user.UserTenantRoles.Select(utr => new TenantRoleDto
        {
            TenantId = utr.TenantId,
            TenantType = utr.TenantType,
            RoleId = utr.RoleId,
            RoleName = utr.Role.Name
        }).ToList();
        
        return Ok(new UserDetailsResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            TenantRoles = tenantRoles
        });
    }
}

public class UserDetailsResponse
{
    public long Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public System.DateTime CreatedAt { get; set; }
    public List<TenantRoleDto> TenantRoles { get; set; } = new();
}

public class TenantRoleDto
{
    public long TenantId { get; set; }
    public string TenantType { get; set; }
    public long RoleId { get; set; }
    public string RoleName { get; set; }
} 