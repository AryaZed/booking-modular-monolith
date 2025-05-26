using System.Reflection;
using Booking.Identity.EventHandlers;
using Booking.Services;
using BuildingBlocks.Caching;
using BuildingBlocks.CAP;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.EventStoreDB;
using BuildingBlocks.Exception;
using BuildingBlocks.IdsGenerator;
using BuildingBlocks.Mapster;
using Booking.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.OpenApi;

namespace Booking;

public static class BookingModule
{
    public static IServiceCollection AddBookingModules(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCustomDbContext<BookingContext>(nameof(Booking), configuration);
        
        // Register booking services
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IBookingPermissionService, BookingPermissionService>();
        
        // Register CAP event handlers
        services.AddTransient<UserCreatedEventHandler>();
        services.AddTransient<UserUpdatedEventHandler>();
        services.AddTransient<UserDeletedEventHandler>();
        services.AddTransient<UserRoleChangedEventHandler>();
        
        services.AddValidatorsFromAssembly(typeof(BookingRoot).Assembly);
        services.AddCustomMapster(typeof(BookingRoot).Assembly);
        
        services.AddCachingRequest(new[] {typeof(BookingRoot).Assembly});
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(EfTxBookingBehavior<,>));
        
        services.AddCustomCap();
        services.AddTransient<IBusPublisher, BusPublisher>();
        services.AddAspnetOpenApi();
        
        return services;
    }

    public static IApplicationBuilder UseBookingModules(this IApplicationBuilder app)
    {
        app.UseMigration<BookingContext>();
        return app;
    }
}
