using System;
using BuildingBlocks.Domain.Event;

namespace Identity.Identity.Events;

public class UserRegisteredEvent : IDomainEvent
{
    public long UserId { get; }
    public string Email { get; }
    public string Username { get; }
    public DateTime RegisteredAt { get; }
    
    public UserRegisteredEvent(long userId, string email, string username)
    {
        UserId = userId;
        Email = email;
        Username = username;
        RegisteredAt = DateTime.UtcNow;
    }
} 