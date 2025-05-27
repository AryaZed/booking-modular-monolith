using BookingMonolith.Identity.Configurations;
using Identity.Data;
using Identity.Identity;
using Identity.Identity.Models;
using Identity.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BuildingBlocks.Constants;
using System.Threading.Tasks;

namespace Identity.Extensions;

public static class IdentityServerExtensions
{
    public static IServiceCollection AddIdentityServer(
        this IServiceCollection services,
        IWebHostEnvironment env
    )
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>(config =>
            {
                config.Password.RequiredLength = 6;
                config.Password.RequireDigit = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

        var identityServerBuilder = services.AddIdentityServer(options =>
                                                               {
                                                                   options.Events.RaiseErrorEvents =
                                                                       true;

                                                                   options.Events
                                                                           .RaiseInformationEvents =
                                                                       true;

                                                                   options.Events
                                                                       .RaiseFailureEvents = true;

                                                                   options.Events
                                                                       .RaiseSuccessEvents = true;
                                                               })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiResources(Config.ApiResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<ApplicationUser>()
            .AddResourceOwnerValidator<UserValidator>();

        if (env.IsDevelopment())
        {
            identityServerBuilder.AddDeveloperSigningCredential();
        }

        services.ConfigureApplicationCookie(options =>
                                            {
                                                options.Events.OnRedirectToLogin = context =>
                                                {
                                                    context.Response.StatusCode =
                                                        StatusCodes.Status401Unauthorized;

                                                    return Task.CompletedTask;
                                                };

                                                options.Events.OnRedirectToAccessDenied = context =>
                                                {
                                                    context.Response.StatusCode =
                                                        StatusCodes.Status403Forbidden;

                                                    return Task.CompletedTask;
                                                };
                                            });

        // Register multi-tenant authorization services
        services.AddScoped<IPermissionValidator, PermissionValidator>();
        services.AddScoped<ITenantRoleService, TenantRoleService>();
        services.AddScoped<IUserTenantService, UserTenantService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentTenantProvider, CurrentTenantProvider>();
        
        // Register OTP services
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IOtpNotificationService, DefaultOtpNotificationService>();

        return services;
    }
}
