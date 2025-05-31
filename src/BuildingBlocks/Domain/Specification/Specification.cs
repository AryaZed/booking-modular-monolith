using System.Linq.Expressions;

namespace BuildingBlocks.Domain.Specification;

/// <summary>
/// Base implementation of the Specification pattern
/// </summary>
/// <typeparam name="T">The type of entity that the specification applies to</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Specification{T}"/> class
    /// </summary>
    /// <param name="criteria">The criteria expression</param>
    protected Specification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    /// <inheritdoc/>
    public Expression<Func<T, bool>> Criteria { get; }
    
    /// <inheritdoc/>
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    
    /// <inheritdoc/>
    public List<string> IncludeStrings { get; } = new();
    
    /// <inheritdoc/>
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    
    /// <inheritdoc/>
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    
    /// <inheritdoc/>
    public Expression<Func<T, object>>? GroupBy { get; private set; }
    
    /// <inheritdoc/>
    public (int pageNumber, int pageSize)? Pagination { get; private set; }

    /// <summary>
    /// Adds an include expression to the specification
    /// </summary>
    /// <param name="includeExpression">The include expression</param>
    /// <returns>The current specification</returns>
    protected Specification<T> AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
        return this;
    }

    /// <summary>
    /// Adds an include string to the specification
    /// </summary>
    /// <param name="includeString">The include string</param>
    /// <returns>The current specification</returns>
    protected Specification<T> AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
        return this;
    }

    /// <summary>
    /// Adds an order by expression to the specification
    /// </summary>
    /// <param name="orderByExpression">The order by expression</param>
    /// <returns>The current specification</returns>
    protected Specification<T> ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
        return this;
    }

    /// <summary>
    /// Adds an order by descending expression to the specification
    /// </summary>
    /// <param name="orderByDescendingExpression">The order by descending expression</param>
    /// <returns>The current specification</returns>
    protected Specification<T> ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
        return this;
    }

    /// <summary>
    /// Adds a group by expression to the specification
    /// </summary>
    /// <param name="groupByExpression">The group by expression</param>
    /// <returns>The current specification</returns>
    protected Specification<T> ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
    {
        GroupBy = groupByExpression;
        return this;
    }

    /// <summary>
    /// Adds pagination to the specification
    /// </summary>
    /// <param name="pageNumber">The page number</param>
    /// <param name="pageSize">The page size</param>
    /// <returns>The current specification</returns>
    protected Specification<T> ApplyPaging(int pageNumber, int pageSize)
    {
        Pagination = (pageNumber, pageSize);
        return this;
    }
} 