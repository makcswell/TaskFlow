namespace Ebceys.Infrastructure.Helpers;

/// <summary>
///     Interface for executing commands resolved from the DI container.
///     Registered as a singleton service. Use <see cref="IScopedCommandExecutor" /> when scoped resolution is needed.
/// </summary>
public interface ICommandExecutor
{
    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TContext">The context.</typeparam>
    /// <typeparam name="TResult">The result.</typeparam>
    /// <returns>The new instance of <typeparamref name="TResult" />.</returns>
    Task<TResult> ExecuteCommandAsync<TContext, TResult>(TContext context, CancellationToken token = default);
}