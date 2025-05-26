using System;
using System.Threading.Tasks;
using Booking.Services;
using BuildingBlocks.Contracts.Identity;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;

namespace Booking.Identity.EventHandlers;

/// <summary>
/// Handles UserCreatedEvent from the Identity module
/// </summary>
public class UserCreatedEventHandler : ICapSubscribe
{
    private readonly ILogger<UserCreatedEventHandler> _logger;
    private readonly IUserProfileService _userProfileService;

    public UserCreatedEventHandler(
        ILogger<UserCreatedEventHandler> logger,
        IUserProfileService userProfileService)
    {
        _logger = logger;
        _userProfileService = userProfileService;
    }

    /// <summary>
    /// Handles the UserCreatedEvent from Identity module
    /// Using the contract DTO to avoid direct dependency on Identity module
    /// </summary>
    [CapSubscribe("UserCreatedEvent")]
    public async Task HandleUserCreatedEvent(UserCreatedIntegrationEvent @event)
    {
        try
        {
            _logger.LogInformation("Received UserCreatedEvent for user {UserId} with email {Email}",
                @event.UserId, @event.Email);

            // Create a corresponding user profile in the Booking module
            await _userProfileService.CreateUserProfileAsync(
                @event.UserId,
                @event.Email,
                $"{@event.FirstName} {@event.LastName}",
                @event.TenantId,
                @event.TenantType);

            _logger.LogInformation("Successfully created user profile for user {UserId}", @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling UserCreatedEvent for user {UserId}", @event.UserId);
            throw; // Rethrowing will cause CAP to retry the message
        }
    }
}
