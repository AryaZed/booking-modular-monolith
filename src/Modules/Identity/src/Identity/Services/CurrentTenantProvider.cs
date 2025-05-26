using System;
using System.Security.Claims;
using BuildingBlocks.Constants;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Http;

namespace Identity.Services;

public class CurrentTenantProvider : ICurrentTenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? TenantId
    {
        get
        {
            var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirst(IdentityConstant.ClaimTypes.TenantId)?.Value;
            if (string.IsNullOrEmpty(claimValue))
                return null;
                
            if (long.TryParse(claimValue, out long tenantId))
                return tenantId;
                
            return null;
        }
    }

    public TenantType? TenantType
    {
        get
        {
            var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirst(IdentityConstant.ClaimTypes.TenantType)?.Value;
            if (string.IsNullOrEmpty(claimValue))
                return null;
                
            if (Enum.TryParse<TenantType>(claimValue, out var tenantType))
                return tenantType;
                
            return null;
        }
    }
} 