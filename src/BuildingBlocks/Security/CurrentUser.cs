using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using BuildingBlocks.Constants;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Security;

/// <summary>
/// Implementation of ICurrentUser that gets user information from ClaimsPrincipal
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ClaimsPrincipal _user => _httpContextAccessor.HttpContext?.User;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? Id => IsAuthenticated ? Convert.ToInt64(GetClaimValue(ClaimTypes.NameIdentifier)) : null;

    public string Username => GetClaimValue(ClaimTypes.Name);

    public string Email => GetClaimValue(ClaimTypes.Email);

    public string FirstName => GetClaimValue(ClaimTypes.GivenName);

    public string LastName => GetClaimValue(ClaimTypes.Surname);

    public IReadOnlyList<string> Roles => GetClaimValues(ClaimTypes.Role);

    public IReadOnlyList<string> Permissions => GetClaimValues(IdentityConstant.ClaimTypes.Permission);

    public long? TenantId => string.IsNullOrEmpty(GetClaimValue(IdentityConstant.ClaimTypes.TenantId))
        ? null
        : Convert.ToInt64(GetClaimValue(IdentityConstant.ClaimTypes.TenantId));

    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated ?? false;

    public bool HasPermission(string permission)
    {
        return Permissions.Contains(permission);
    }

    public bool IsInRole(string role)
    {
        return Roles.Contains(role);
    }

    private string GetClaimValue(string claimType)
    {
        return _user?.FindFirst(claimType)?.Value;
    }

    private List<string> GetClaimValues(string claimType)
    {
        return _user?.Claims
            .Where(c => c.Type == claimType)
            .Select(c => c.Value)
            .ToList() ?? new List<string>();
    }
}
