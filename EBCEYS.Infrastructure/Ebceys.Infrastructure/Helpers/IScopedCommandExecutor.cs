namespace Ebceys.Infrastructure.Helpers;

/// <summary>
///     Scoped command executor interface. Resolves <see cref="ICommand{TContext,TResult}" /> from the current DI scope,
///     ensuring proper scoped service lifetime management (e.g., DbContext, scoped repositories).
/// </summary>
public interface IScopedCommandExecutor
{
    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TContext">The command context type.</typeparam>
    /// <typeparam name="TResult">The command result type.</typeparam>
    /// <returns>The new <see cref="Task" /> which result contains <typeparamref name="TResult" />.</returns>
    Task<TResult> ExecuteAsync<TContext, TResult>(TContext context, CancellationToken token = default);
}