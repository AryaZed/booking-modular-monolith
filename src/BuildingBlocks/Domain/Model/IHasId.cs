namespace BuildingBlocks.Domain.Model;

/// <summary>
/// Interface for entities with strongly typed IDs
/// </summary>
/// <typeparam name="TId">The type of the entity identifier</typeparam>
public interface IHasId<out TId>
{
    /// <summary>
    /// Gets the identifier of the entity
    /// </summary>
    TId Id { get; }
} 