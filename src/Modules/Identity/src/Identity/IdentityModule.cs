using System.Collections.Generic;
using System.Reflection;
using BuildingBlocks.Caching;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.Mapster;
using FluentValidation;
using Identity.Data;
using Identity.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Authorization;
using BuildingBlocks.CAP;
using Identity.Data.Seed;
using Identity.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System;
using BuildingBlocks.Security;
using BuildingBlocks.Email;
using BuildingBlocks.CQRS;
using Identity.Identity.Repositories;
using Identity.Identity.Services;

namespace Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModules(this IServiceCollection services, IConfiguration configuration,
        IWebHostEnvironment env = null)
    {
        services.AddCustomDbContext<IdentityContext>(nameof(Identity), configuration);
        services.AddScoped<IDataSeeder, IdentityDataSeeder>();

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        // Register services
        services.AddScoped<UserService>();

        services.AddTransient<IEventMapper, EventMapper>();
        services.AddIdentityServer(env);

        services.AddValidatorsFromAssembly(typeof(IdentityRoot).Assembly);
        services.AddCustomMapster(typeof(IdentityRoot).Assembly);

        services.AddCachingRequest(new List<Assembly> {typeof(IdentityRoot).Assembly});
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(EfTxIdentityBehavior<,>));

        services.AddPermissionAuthorization();
        
        // Register event dispatching services
        services.AddScoped<IEventDispatcher, EventDispatcher>();
        
        // Register CQRS
        services.AddCQRS(typeof(IdentityRoot).Assembly);
        
        // Register Security services
        services.AddSecurity();
        
        // Register Email services
        services.AddEmail(configuration);

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

        return services;
    }

    public static IApplicationBuilder UseIdentityModules(this IApplicationBuilder app)
    {
        app.UseIdentityServer();
        app.UseMigration<IdentityContext>();
        return app;
    }
}
