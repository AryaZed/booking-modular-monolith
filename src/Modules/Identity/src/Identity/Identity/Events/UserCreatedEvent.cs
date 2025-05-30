using System;
using BuildingBlocks.Domain.Event;

namespace Identity.Identity.Events;

/// <summary>
/// Event published when a new user is created
/// </summary>
public class UserCreatedEvent : IDomainEvent
{
    public long UserId { get; }
    public string Email { get; }
    public string Username { get; }
    public DateTime CreatedAt { get; }
    
    public UserCreatedEvent(long userId, string email, string username)
    {
        UserId = userId;
        Email = email;
        Username = username;
        CreatedAt = DateTime.UtcNow;
    }
} 