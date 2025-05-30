using System;
using BuildingBlocks.Domain.Event;

namespace Identity.Identity.Events;

/// <summary>
/// Event published when a user is updated
/// </summary>
public class UserUpdatedEvent : IDomainEvent
{
    public long UserId { get; }
    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public DateTime UpdatedAt { get; }
    
    public UserUpdatedEvent(long userId, string email, string firstName, string lastName)
    {
        UserId = userId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        UpdatedAt = DateTime.UtcNow;
    }
} 