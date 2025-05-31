using System.Data;
using BuildingBlocks.Domain.Model;
using BuildingBlocks.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingBlocks.Domain.Repository;

/// <summary>
/// Entity Framework Core implementation of the Unit of Work pattern
/// </summary>
public class EfCoreUnitOfWork : IUnitOfWork
{
    private readonly IDbContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfCoreUnitOfWork"/> class
    /// </summary>
    /// <param name="dbContext">The database context</param>
    public EfCoreUnitOfWork(IDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc/>
    public IRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : class, IAggregate<TId>
    {
        var entityType = typeof(TEntity);
        
        if (!_repositories.ContainsKey(entityType))
        {
            var repositoryType = typeof(EfCoreRepository<,>).MakeGenericType(typeof(TEntity), typeof(TId));
            var repository = Activator.CreateInstance(repositoryType, _dbContext);
            
            if (repository == null)
                throw new InvalidOperationException($"Could not create repository for {entityType.Name}");
                
            _repositories.Add(entityType, repository);
        }
        
        return (IRepository<TEntity, TId>)_repositories[entityType];
    }

    /// <inheritdoc/>
    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
            return;
            
        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            if (_currentTransaction != null)
                await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    /// <inheritdoc/>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
            return;
            
        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task ExecuteTransactionAsync(Func<Task> action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
            try
            {
                await action();
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    /// <inheritdoc/>
    public async Task<TResult> ExecuteTransactionAsync<TResult>(Func<Task<TResult>> func, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
            try
            {
                var result = await func();
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the resources used by this unit of work
    /// </summary>
    /// <param name="disposing">Whether the method is being called from Dispose</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
            
            _disposed = true;
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_currentTransaction != null)
                await _currentTransaction.DisposeAsync();
                
            _currentTransaction = null;
            _disposed = true;
        }
        
        GC.SuppressFinalize(this);
    }
} 