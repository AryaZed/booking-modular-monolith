using System.Reflection;
using BuildingBlocks.Caching;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.Mapster;
using FluentValidation;
using Identity.Data;
using Identity.Extensions;
using MediatR;
using BuildingBlocks.Authorization;
using BuildingBlocks.CAP;
using Microsoft.AspNetCore.RateLimiting;

namespace Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModules(this IServiceCollection services, IConfiguration configuration,
        IWebHostEnvironment env = null)
    {
        services.AddCustomDbContext<IdentityContext>(nameof(Identity), configuration);
        services.AddScoped<IDataSeeder, IdentityDataSeeder>();

        services.AddTransient<IEventMapper, EventMapper>();
        services.AddIdentityServer(env);

        services.AddValidatorsFromAssembly(typeof(IdentityRoot).Assembly);
        services.AddCustomMapster(typeof(IdentityRoot).Assembly);

        services.AddCachingRequest(new List<Assembly> { typeof(IdentityRoot).Assembly });
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(EfTxIdentityBehavior<,>));

        services.AddPermissionAuthorization();

        // Register event dispatching services
        services.AddScoped<IEventDispatcher, EventDispatcher>();

        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("registration", opt =>
            {
                opt.PermitLimit = 5;
                opt.Window = TimeSpan.FromMinutes(10);
                opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 2;
            });
        });

        // Add module authorization
        services.AddModuleAuthorization();

        // Register domain event handlers
        services.AddScoped<IDomainEventHandler<Identity.Events.BranchReassignedEvent>,
            Identity.EventHandlers.BranchReassignedEventHandler>();

        return services;
    }

    public static IApplicationBuilder UseIdentityModules(this IApplicationBuilder app)
    {
        app.UseIdentityServer();
        app.UseMigration<IdentityContext>();
        return app;
    }
}
