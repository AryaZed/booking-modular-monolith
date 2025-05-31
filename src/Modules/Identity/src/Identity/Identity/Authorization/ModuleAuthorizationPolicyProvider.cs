using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Identity.Identity.Authorization;

/// <summary>
/// Authorization policy provider that creates policies for module access
/// </summary>
public class ModuleAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    public const string MODULE_POLICY_PREFIX = "Module:";
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    
    public ModuleAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }
    
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();
    
    public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();
    
    public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(MODULE_POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            var moduleCode = policyName.Substring(MODULE_POLICY_PREFIX.Length);
            
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new ModuleRequirement(moduleCode))
                .Build();
                
            return Task.FromResult(policy);
        }
        
        // If not a module policy, use the fallback provider
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
} 