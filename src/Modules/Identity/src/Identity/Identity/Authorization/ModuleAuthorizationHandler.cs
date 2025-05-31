using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Identity.Services;

namespace Identity.Identity.Authorization;

/// <summary>
/// Authorization handler that checks if the user has access to a specific module
/// </summary>
public class ModuleAuthorizationHandler : AuthorizationHandler<ModuleRequirement>
{
    private readonly ICurrentTenantProvider _tenantProvider;
    
    public ModuleAuthorizationHandler(ICurrentTenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }
    
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        ModuleRequirement requirement)
    {
        if (!context.User.Identity.IsAuthenticated)
        {
            return; // Not authenticated, so can't succeed
        }
        
        // First check claim-based approach (from token)
        var moduleClaims = context.User.Claims.Where(c => c.Type == "module").Select(c => c.Value.ToLowerInvariant());
        if (moduleClaims.Contains(requirement.ModuleCode.ToLowerInvariant()))
        {
            context.Succeed(requirement);
            return;
        }
        
        // If claim-based check fails, use the current tenant provider for a real-time check
        // This is useful when subscriptions have changed since token was issued
        if (await _tenantProvider.HasModuleAccessAsync(requirement.ModuleCode))
        {
            context.Succeed(requirement);
            return;
        }
    }
} 