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

namespace Identity.Identity.Features.Users.ListUsers;

[Route(BaseApiPath + "/identity/users")]
public class ListUsersEndpoint : BaseController
{
    private readonly IdentityContext _context;

    public ListUsersEndpoint(IdentityContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "List users",
        Description = "List users with pagination and filtering options",
        Tags = new[] { "Identity" })]
    public async Task<ActionResult<PagedResult<UserListItemResponse>>> ListUsers(
        [FromQuery] UserListRequest request,
        CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = User.GetUserId();
        if (currentUserId == 0)
        {
            return Unauthorized();
        }
        
        // Start with base query
        var query = _context.Users
            .Include(u => u.UserTenantRoles)
                .ThenInclude(utr => utr.Role)
            .AsQueryable();
            
        // Filter by tenant if specified
        if (request.TenantId.HasValue)
        {
            query = query.Where(u => u.UserTenantRoles.Any(utr => 
                utr.TenantId == request.TenantId && 
                (string.IsNullOrEmpty(request.TenantType) || utr.TenantType == request.TenantType)));
        }
        
        // Filter by active status if specified
        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive);
        }
        
        // Filter by search term if provided
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(u => 
                u.Email.ToLower().Contains(searchTerm) ||
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm));
        }
        
        // Apply tenant-based restrictions unless user is system admin
        if (!User.IsSystemAdmin())
        {
            var currentUserTenantId = User.GetTenantId();
            var currentUserTenantType = User.GetTenantType();
            
            if (!currentUserTenantId.HasValue)
            {
                return Forbid("You don't have permission to list users");
            }
            
            // Brand admins can see users in their brand and branches under their brand
            if (currentUserTenantType == IdentityConstant.TenantType.Brand && User.IsBrandAdmin())
            {
                var branchesUnderBrand = await _context.Branches
                    .Where(b => b.BrandId == currentUserTenantId)
                    .Select(b => b.Id)
                    .ToListAsync(cancellationToken);
                    
                query = query.Where(u => 
                    u.UserTenantRoles.Any(utr => 
                        (utr.TenantType == IdentityConstant.TenantType.Brand && utr.TenantId == currentUserTenantId) ||
                        (utr.TenantType == IdentityConstant.TenantType.Branch && branchesUnderBrand.Contains(utr.TenantId))));
            }
            // Branch admins can only see users in their branch
            else if (currentUserTenantType == IdentityConstant.TenantType.Branch && User.IsBranchAdmin())
            {
                query = query.Where(u => 
                    u.UserTenantRoles.Any(utr => 
                        utr.TenantType == IdentityConstant.TenantType.Branch && 
                        utr.TenantId == currentUserTenantId));
            }
            else
            {
                // Regular users can only see themselves
                query = query.Where(u => u.Id == currentUserId);
            }
        }
        
        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);
        
        // Apply pagination
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
        var skip = (pageNumber - 1) * pageSize;
        
        // Apply sorting
        query = request.SortDirection?.ToLower() == "desc" 
            ? query.OrderByDescending(u => u.CreatedAt) 
            : query.OrderBy(u => u.CreatedAt);
            
        // Execute query with pagination
        var users = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        // Map to response
        var items = users.Select(u => new UserListItemResponse
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            Roles = u.UserTenantRoles.Select(utr => new UserRoleDto
            {
                RoleName = utr.Role.Name,
                TenantId = utr.TenantId,
                TenantType = utr.TenantType
            }).ToList()
        }).ToList();
        
        return Ok(new PagedResult<UserListItemResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize)
        });
    }
}

public class UserListRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SearchTerm { get; set; }
    public long? TenantId { get; set; }
    public string TenantType { get; set; }
    public bool? IsActive { get; set; }
    public string SortDirection { get; set; } = "asc";
}

public class UserListItemResponse
{
    public long Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsActive { get; set; }
    public System.DateTime CreatedAt { get; set; }
    public List<UserRoleDto> Roles { get; set; } = new();
}

public class UserRoleDto
{
    public string RoleName { get; set; }
    public long TenantId { get; set; }
    public string TenantType { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
} 