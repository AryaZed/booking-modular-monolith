using System;
using System.Threading.Tasks;
using Booking.Services;
using BuildingBlocks.Contracts.Identity;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;

namespace Booking.Identity.EventHandlers;

/// <summary>
/// Handles UserUpdatedEvent from the Identity module
/// </summary>
public class UserUpdatedEventHandler : ICapSubscribe
{
    private readonly ILogger<UserUpdatedEventHandler> _logger;
    private readonly IUserProfileService _userProfileService;

    public UserUpdatedEventHandler(
        ILogger<UserUpdatedEventHandler> logger,
        IUserProfileService userProfileService)
    {
        _logger = logger;
        _userProfileService = userProfileService;
    }

    /// <summary>
    /// Handles the UserUpdatedEvent from Identity module
    /// Using the contract DTO to avoid direct dependency on Identity module
    /// </summary>
    [CapSubscribe("UserUpdatedEvent")]
    public async Task HandleUserUpdatedEvent(UserUpdatedIntegrationEvent @event)
    {
        try
        {
            _logger.LogInformation("Received UserUpdatedEvent for user {UserId} with email {Email}", 
                @event.UserId, @event.Email);

            // Update corresponding user profile in the Booking module
            await _userProfileService.UpdateUserProfileAsync(
                @event.UserId,
                @event.Email,
                $"{@event.FirstName} {@event.LastName}",
                @event.IsActive);
                
            _logger.LogInformation("Successfully updated user profile for user {UserId}", @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling UserUpdatedEvent for user {UserId}", @event.UserId);
            throw; // Rethrowing will cause CAP to retry the message
        }
    }
} 