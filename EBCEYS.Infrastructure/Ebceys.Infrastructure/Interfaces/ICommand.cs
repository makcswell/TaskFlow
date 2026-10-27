using JetBrains.Annotations;

namespace Ebceys.Infrastructure.Interfaces;

/// <summary>
///     Generic command interface implementing the Command pattern. Encapsulates a unit of work
///     that takes a context of type <typeparamref name="TContext" /> and returns a result of type
///     <typeparamref name="TResult" />. Commands are resolved from DI and executed via
///     <see cref="Helpers.ICommandExecutor" /> or <see cref="Helpers.IScopedCommandExecutor" />.
/// </summary>
/// <typeparam name="TContext">The input context (parameters/data) for the command.</typeparam>
/// <typeparam name="TResult">The result type returned after command execution.</typeparam>
[PublicAPI]
public interface ICommand<in TContext, TResult>
{
    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The <see cref="Task{TResult}" /> with specified result instance of <see cref="TResult" />.</returns>
    Task<TResult> ExecuteAsync(TContext context, CancellationToken token = default);
}