using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.CQRS;

public static class Extensions
{
    /// <summary>
    /// Adds CQRS services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">The assemblies to scan for handlers</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddCQRS(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Add MediatR for handling commands and queries
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));
        
        return services;
    }
} 