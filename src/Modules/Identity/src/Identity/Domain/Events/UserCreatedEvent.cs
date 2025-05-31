using BuildingBlocks.Domain.Event;

namespace Identity.Domain.Events;

public record UserCreatedEvent : IDomainEvent
{
    public long UserId { get; }
    public string UserName { get; }
    public string Email { get; }
    public long? TenantId { get; }
    public DateTime CreatedAt { get; }

    public UserCreatedEvent(long userId, string userName, string email, long? tenantId)
    {
        UserId = userId;
        UserName = userName;
        Email = email;
        TenantId = tenantId;
        CreatedAt = DateTime.UtcNow;
    }
} 