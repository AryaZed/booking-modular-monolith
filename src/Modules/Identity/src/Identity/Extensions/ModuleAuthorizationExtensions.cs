using Identity.Identity.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Extensions;

public static class ModuleAuthorizationExtensions
{
    /// <summary>
    /// Adds module-based authorization to the application
    /// </summary>
    public static IServiceCollection AddModuleAuthorization(this IServiceCollection services)
    {
        // Register authorization handlers
        services.AddScoped<IAuthorizationHandler, ModuleAuthorizationHandler>();
        
        // Register policy provider
        services.AddSingleton<IAuthorizationPolicyProvider, ModuleAuthorizationPolicyProvider>();
        
        return services;
    }
    
    /// <summary>
    /// Creates a policy name for a module
    /// </summary>
    public static string ForModule(this AuthorizationPolicyBuilder builder, string moduleCode)
    {
        return $"{ModuleAuthorizationPolicyProvider.MODULE_POLICY_PREFIX}{moduleCode}";
    }
    
    /// <summary>
    /// Requires access to a specific module
    /// </summary>
    public static AuthorizationPolicyBuilder RequireModule(this AuthorizationPolicyBuilder builder, string moduleCode)
    {
        return builder.AddRequirements(new ModuleRequirement(moduleCode));
    }
} 