using MediatR;

namespace BuildingBlocks.CQRS;

/// <summary>
/// Handler for commands that don't return a value
/// </summary>
/// <typeparam name="TCommand">The type of command to handle</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand> 
    where TCommand : ICommand
{
}

/// <summary>
/// Handler for commands that return a value
/// </summary>
/// <typeparam name="TCommand">The type of command to handle</typeparam>
/// <typeparam name="TResult">The type of result returned by the command</typeparam>
public interface ICommandHandler<in TCommand, TResult> : IRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
} 