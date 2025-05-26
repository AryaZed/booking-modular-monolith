using System;
using System.Threading.Tasks;
using Booking.Services;
using BuildingBlocks.Contracts.Identity;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;

namespace Booking.Identity.EventHandlers;

/// <summary>
/// Handles UserRoleChangedEvent from the Identity module
/// </summary>
public class UserRoleChangedEventHandler : ICapSubscribe
{
    private readonly ILogger<UserRoleChangedEventHandler> _logger;
    private readonly IBookingPermissionService _bookingPermissionService;

    public UserRoleChangedEventHandler(
        ILogger<UserRoleChangedEventHandler> logger,
        IBookingPermissionService bookingPermissionService)
    {
        _logger = logger;
        _bookingPermissionService = bookingPermissionService;
    }

    /// <summary>
    /// Handles the UserRoleChangedEvent from Identity module
    /// Using the contract DTO to avoid direct dependency on Identity module
    /// </summary>
    [CapSubscribe("UserRoleChangedEvent")]
    public async Task HandleUserRoleChangedEvent(UserRoleChangedIntegrationEvent @event)
    {
        try
        {
            _logger.LogInformation(
                "Received UserRoleChangedEvent for user {UserId} with role {RoleName} in tenant {TenantId} ({TenantType}). Action: {Action}", 
                @event.UserId, 
                @event.RoleName,
                @event.TenantId,
                @event.TenantType,
                @event.IsRoleAdded ? "added" : "removed");

            if (@event.IsRoleAdded)
            {
                // Role was added
                await _bookingPermissionService.SyncUserPermissionsAsync(
                    @event.UserId,
                    @event.RoleId,
                    @event.TenantId,
                    @event.TenantType);
            }
            else
            {
                // Role was removed
                await _bookingPermissionService.RemoveUserPermissionsAsync(
                    @event.UserId,
                    @event.RoleId,
                    @event.TenantId);
            }
                
            _logger.LogInformation(
                "Successfully processed role change for user {UserId}",
                @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling UserRoleChangedEvent for user {UserId}", @event.UserId);
            throw; // Rethrowing will cause CAP to retry the message
        }
    }
} 