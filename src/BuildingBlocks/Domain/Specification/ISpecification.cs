using System.Linq.Expressions;

namespace BuildingBlocks.Domain.Specification;

/// <summary>
/// Base interface for specifications in the domain model
/// </summary>
/// <typeparam name="T">The type of entity that the specification applies to</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Gets the expression that defines the specification
    /// </summary>
    Expression<Func<T, bool>> Criteria { get; }
    
    /// <summary>
    /// Gets the list of include expressions for eager loading related entities
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }
    
    /// <summary>
    /// Gets the list of string paths for eager loading related entities
    /// </summary>
    List<string> IncludeStrings { get; }
    
    /// <summary>
    /// Gets the ordering expression for the entity
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }
    
    /// <summary>
    /// Gets the descending ordering expression for the entity
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }
    
    /// <summary>
    /// Gets the grouping expression for the entity
    /// </summary>
    Expression<Func<T, object>>? GroupBy { get; }
    
    /// <summary>
    /// Gets the pagination information for the query
    /// </summary>
    (int pageNumber, int pageSize)? Pagination { get; }
} 