using System;
using System.Security.Claims;
using Identity.Data;
using Identity.Identity.Models;
using Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Identity.Testing;

/// <summary>
/// Test fixture for Identity module integration tests
/// </summary>
public class IdentityTestFixture : IDisposable
{
    public IdentityContext DbContext { get; }
    public UserManager<ApplicationUser> UserManager { get; }
    public RoleManager<ApplicationRole> RoleManager { get; }
    public ITokenService TokenService { get; }
    public IPermissionValidator PermissionValidator { get; }
    public ICurrentTenantProvider CurrentTenantProvider { get; }
    
    private readonly ServiceProvider _serviceProvider;
    
    public IdentityTestFixture()
    {
        var services = new ServiceCollection();
        
        // Setup in-memory database
        services.AddDbContext<IdentityContext>(options =>
        {
            options.UseInMemoryDatabase("TestIdentityDb");
        });
        
        // Setup Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();
        
        // Setup HTTP context accessor with mock
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        services.AddSingleton(httpContextAccessor.Object);
        
        // Register services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ITenantPermissionService, TenantPermissionService>();
        services.AddScoped<IPermissionValidator, PermissionValidatorAdapter>();
        services.AddScoped<ICurrentTenantProvider, CurrentTenantProvider>();
        
        // Build service provider
        _serviceProvider = services.BuildServiceProvider();
        
        // Initialize services
        DbContext = _serviceProvider.GetRequiredService<IdentityContext>();
        UserManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        RoleManager = _serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        TokenService = _serviceProvider.GetRequiredService<ITokenService>();
        PermissionValidator = _serviceProvider.GetRequiredService<IPermissionValidator>();
        CurrentTenantProvider = _serviceProvider.GetRequiredService<ICurrentTenantProvider>();
    }
    
    /// <summary>
    /// Sets the current user for testing
    /// </summary>
    public void SetCurrentUser(ClaimsPrincipal user)
    {
        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(x => x.User).Returns(user);
        
        var httpContextAccessor = _serviceProvider.GetRequiredService<IHttpContextAccessor>();
        var mockAccessor = Mock.Get(httpContextAccessor);
        mockAccessor.Setup(x => x.HttpContext).Returns(httpContext.Object);
    }
    
    /// <summary>
    /// Sets the current tenant for testing
    /// </summary>
    public void SetCurrentTenant(long tenantId, TenantType tenantType)
    {
        var currentTenantProvider = _serviceProvider.GetRequiredService<ICurrentTenantProvider>();
        var mockProvider = Mock.Get(currentTenantProvider);
        mockProvider.Setup(x => x.TenantId).Returns(tenantId);
        mockProvider.Setup(x => x.TenantType).Returns(tenantType);
    }
    
    public void Dispose()
    {
        _serviceProvider.Dispose();
    }
} 