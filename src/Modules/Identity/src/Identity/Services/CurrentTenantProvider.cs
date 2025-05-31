using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Constants;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Http;

namespace Identity.Services;

public class CurrentTenantProvider : ICurrentTenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IModuleService _moduleService;

    public CurrentTenantProvider(
        IHttpContextAccessor httpContextAccessor,
        IModuleService moduleService)
    {
        _httpContextAccessor = httpContextAccessor;
        _moduleService = moduleService;
    }

    public long? TenantId
    {
        get
        {
            var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirst(IdentityConstant.ClaimTypes.TenantId)?.Value;
            return !string.IsNullOrEmpty(claimValue) && long.TryParse(claimValue, out var id) ? id : null;
        }
    }

    public TenantType? TenantType
    {
        get
        {
            var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirst(IdentityConstant.ClaimTypes.TenantType)?.Value;

            if (string.IsNullOrEmpty(claimValue))
            {
                return null;
            }

            if (System.Enum.TryParse<TenantType>(claimValue, out var tenantType))
            {
                return tenantType;
            }

            return null;
        }
    }
    
    public async Task<bool> HasModuleAccessAsync(string moduleCode)
    {
        if (!TenantId.HasValue)
        {
            // If no tenant context, consider it as no access
            return false;
        }
        
        return await _moduleService.HasModuleAccessAsync(TenantId.Value, moduleCode);
    }
    
    public async Task<IReadOnlyList<string>> GetAccessibleModuleCodesAsync()
    {
        if (!TenantId.HasValue)
        {
            // If no tenant context, return empty list
            return new List<string>().AsReadOnly();
        }
        
        var modules = await _moduleService.GetTenantModuleCodesAsync(TenantId.Value);
        return modules.ToList().AsReadOnly();
    }
} 