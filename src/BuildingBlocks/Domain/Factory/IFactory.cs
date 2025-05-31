namespace BuildingBlocks.Domain.Factory;

/// <summary>
/// Generic factory interface for creating domain entities
/// </summary>
/// <typeparam name="TEntity">The type of entity to create</typeparam>
/// <typeparam name="TParams">The type of parameters used for creation</typeparam>
public interface IFactory<out TEntity, in TParams>
{
    /// <summary>
    /// Creates a new entity with the specified parameters
    /// </summary>
    /// <param name="parameters">The parameters for entity creation</param>
    /// <returns>The created entity</returns>
    TEntity Create(TParams parameters);
} 