using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Domain.Specification;

/// <summary>
/// Evaluates specifications and applies them to IQueryable
/// </summary>
public class SpecificationEvaluator<T> where T : class
{
    /// <summary>
    /// Gets the default instance of the specification evaluator
    /// </summary>
    public static SpecificationEvaluator<T> Default { get; } = new();

    /// <summary>
    /// Applies the specification to the queryable
    /// </summary>
    /// <param name="inputQuery">The input queryable</param>
    /// <param name="specification">The specification to apply</param>
    /// <returns>The resulting queryable after applying the specification</returns>
    public IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification)
    {
        var query = inputQuery;

        // Apply criteria
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes
        query = specification.Includes.Aggregate(query,
            (current, include) => current.Include(include));

        // Apply string includes
        query = specification.IncludeStrings.Aggregate(query,
            (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply grouping
        if (specification.GroupBy != null)
        {
            query = query.GroupBy(specification.GroupBy).SelectMany(x => x);
        }

        // Apply pagination
        if (specification.Pagination.HasValue)
        {
            var (pageNumber, pageSize) = specification.Pagination.Value;
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        return query;
    }
} 