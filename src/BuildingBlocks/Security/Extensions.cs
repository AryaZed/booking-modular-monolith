using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Security;

public static class Extensions
{
    /// <summary>
    /// Add security services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        
        return services;
    }
} 