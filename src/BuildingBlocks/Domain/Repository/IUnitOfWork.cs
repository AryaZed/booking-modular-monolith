using System.Data;
using BuildingBlocks.Domain.Model;

namespace BuildingBlocks.Domain.Repository;

/// <summary>
/// Unit of Work pattern interface for coordinating multiple repositories
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets a repository for a specific entity type
    /// </summary>
    /// <typeparam name="TEntity">The type of entity</typeparam>
    /// <typeparam name="TId">The type of entity identifier</typeparam>
    /// <returns>The repository for the specified entity type</returns>
    IRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : class, IAggregate<TId>;
    
    /// <summary>
    /// Begins a transaction with the specified isolation level
    /// </summary>
    /// <param name="isolationLevel">The isolation level of the transaction</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>Completion task</returns>
    Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Commits the current transaction
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>Completion task</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>Completion task</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Saves all changes made in this unit of work to the database
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes the specified action within a transaction
    /// </summary>
    /// <param name="action">The action to execute</param>
    /// <param name="isolationLevel">The isolation level of the transaction</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>Completion task</returns>
    Task ExecuteTransactionAsync(Func<Task> action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes the specified function within a transaction
    /// </summary>
    /// <typeparam name="TResult">The type of the result</typeparam>
    /// <param name="func">The function to execute</param>
    /// <param name="isolationLevel">The isolation level of the transaction</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The result of the function</returns>
    Task<TResult> ExecuteTransactionAsync<TResult>(Func<Task<TResult>> func, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);
} 