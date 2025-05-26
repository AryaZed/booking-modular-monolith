using System;
using System.Threading.Tasks;
using Booking.Services;
using BuildingBlocks.Contracts.Identity;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;

namespace Booking.Identity.EventHandlers;

/// <summary>
/// Handles UserDeletedEvent from the Identity module
/// </summary>
public class UserDeletedEventHandler : ICapSubscribe
{
    private readonly ILogger<UserDeletedEventHandler> _logger;
    private readonly IUserProfileService _userProfileService;

    public UserDeletedEventHandler(
        ILogger<UserDeletedEventHandler> logger,
        IUserProfileService userProfileService)
    {
        _logger = logger;
        _userProfileService = userProfileService;
    }

    /// <summary>
    /// Handles the UserDeletedEvent from Identity module
    /// Using the contract DTO to avoid direct dependency on Identity module
    /// </summary>
    [CapSubscribe("UserDeletedEvent")]
    public async Task HandleUserDeletedEvent(UserDeletedIntegrationEvent @event)
    {
        try
        {
            _logger.LogInformation("Received UserDeletedEvent for user {UserId}", @event.UserId);

            // Mark the user as deleted in the Booking module
            await _userProfileService.DeleteUserProfileAsync(@event.UserId);
                
            _logger.LogInformation("Successfully marked user {UserId} as deleted in Booking module", @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling UserDeletedEvent for user {UserId}", @event.UserId);
            throw; // Rethrowing will cause CAP to retry the message
        }
    }
} 