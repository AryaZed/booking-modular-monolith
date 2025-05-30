using MediatR;

namespace BuildingBlocks.CQRS;

/// <summary>
/// Marker interface for queries
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query</typeparam>
public interface IQuery<out TResult> : IRequest<TResult>
{
} 