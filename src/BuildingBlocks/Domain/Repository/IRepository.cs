using System.Linq.Expressions;
using BuildingBlocks.Domain.Model;
using BuildingBlocks.Domain.Specification;

namespace BuildingBlocks.Domain.Repository;

/// <summary>
/// Generic repository interface for domain entities
/// </summary>
/// <typeparam name="TEntity">The type of entity</typeparam>
/// <typeparam name="TId">The type of entity identifier</typeparam>
public interface IRepository<TEntity, in TId> where TEntity : class, IAggregate<TId>
{
    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>List of all entities</returns>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities by specification
    /// </summary>
    /// <param name="specification">The specification to apply</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>List of entities matching the specification</returns>
    Task<IReadOnlyList<TEntity>> GetAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single entity by specification
    /// </summary>
    /// <param name="specification">The specification to apply</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The entity matching the specification, or null if not found</returns>
    Task<TEntity?> GetSingleAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single entity by ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The entity with the specified ID, or null if not found</returns>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The added entity</returns>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities
    /// </summary>
    /// <param name="entities">The entities to add</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>Completion task</returns>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>Completion task</returns>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>Completion task</returns>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching a specification
    /// </summary>
    /// <param name="specification">The specification to apply</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The count of matching entities</returns>
    Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the specification
    /// </summary>
    /// <param name="specification">The specification to apply</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if any entity matches the specification, false otherwise</returns>
    Task<bool> AnyAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes made to the repository
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>Completion task</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
} 