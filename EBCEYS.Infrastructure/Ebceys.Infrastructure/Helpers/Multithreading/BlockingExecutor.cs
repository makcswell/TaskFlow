using System.Threading.Channels;
using JetBrains.Annotations;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

namespace Ebceys.Infrastructure.Helpers.Multithreading;

/// <summary>
///     The <see cref="BlockingExecutor{TContext,TResult}" /> abstract class.
/// </summary>
/// <param name="configuration">The configuration.</param>
/// <typeparam name="TContext">The function context.</typeparam>
/// <typeparam name="TResult">The function result.</typeparam>
[PublicAPI]
public abstract class BlockingExecutor<TContext, TResult>(BlockingLockConfiguration? configuration = null) : IDisposable
{
    /// <summary>
    ///     The channel.
    /// </summary>
    protected internal Channel<TResult> Channel { get; } =
        configuration?.ResultsCacheCapacity != null
            ? System.Threading.Channels.Channel.CreateBounded<TResult>(configuration.ResultsCacheCapacity.Value)
            : System.Threading.Channels.Channel.CreateUnbounded<TResult>();

    /// <summary>
    ///     The semaphore.
    /// </summary>
    protected internal SemaphoreSlim Semaphore = new(configuration?.MaxDegreeOfParallelism ?? 1,
        configuration?.MaxDegreeOfParallelism ?? 1);

    /// <inheritdoc />
    public void Dispose()
    {
        Channel.Writer.Complete();
        Semaphore.Dispose();
    }

    /// <summary>
    ///     Process the function with the specified context and cancellation token. The implementation of this method should
    ///     contain the logic for processing the function and returning the result. The result can be either a synchronous
    ///     value or an asynchronous task, depending on the specific implementation in the derived classes.
    /// </summary>
    /// <param name="context">The function context.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The new instance of <see cref="TResult" />.</returns>
    protected abstract TResult ProcessFunc(TContext context, CancellationToken token);

    /// <summary>
    ///     Enques the <paramref name="context" /> to be processed by the <see cref="ProcessFunc" /> method. This method
    ///     ensures that the number of concurrently processed contexts does not exceed the specified maximum degree of
    ///     parallelism in the configuration. If the maximum degree of parallelism is set to zero, an exception is thrown. The
    ///     method waits for an available slot in the semaphore, processes the context using the <see cref="ProcessFunc" />
    ///     method, and writes the result to the channel. Finally, it releases the semaphore slot to allow other contexts to be
    ///     processed.
    /// </summary>
    /// <param name="context">The function context.</param>
    /// <param name="token">The cancellation token.</param>
    public async Task EnqueJobAsync(TContext context, CancellationToken token = default)
    {
        try
        {
            await Semaphore.WaitAsync(token);
            var result = ProcessFunc(context, token);
            if (result is Task task)
            {
                await task;
            }

            await Channel.Writer.WriteAsync(result, token);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    /// <summary>
    ///     Gets the results queue from already processed functions.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The new instance of <see cref="IAsyncEnumerable{T}" /> with <typeparamref name="TResult" /> elements.</returns>
    public virtual IAsyncEnumerable<TResult> GetResultsAsync(CancellationToken token = default)
    {
        return Channel.Reader.ReadAllAsync(token);
    }
}

/// <summary>
///     The <see cref="BlockingAsyncExecutor{TContext,TResult}" /> abstract class.
/// </summary>
/// <param name="configuration">The configuration.</param>
/// <typeparam name="TContext">The function context.</typeparam>
/// <typeparam name="TResult">The function result.</typeparam>
[PublicAPI]
public abstract class BlockingAsyncExecutor<TContext, TResult>(BlockingLockConfiguration? configuration = null)
    : BlockingExecutor<TContext, TResult>(configuration)
{
    /// <summary>
    ///     Process the function with the specified context and cancellation token. The implementation of this method should
    ///     contain the logic for processing the function and returning the result. The result can be either a synchronous
    ///     value or an asynchronous task, depending on the specific implementation in the derived classes.
    /// </summary>
    /// <param name="context">The function context.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The new instance of <see cref="TResult" />.</returns>
    protected abstract Task<TResult> ProcessFuncAsync(TContext context, CancellationToken token);

    /// <inheritdoc />
    [Obsolete("Use GetResultsAsync instead", true)]
    protected override TResult ProcessFunc(TContext context, CancellationToken token)
    {
        throw new NotSupportedException("The method or operation is not supported.");
    }

    /// <summary>
    ///     Enques the <paramref name="context" /> to be processed by the <see cref="ProcessFunc" /> method. This method
    ///     ensures that the number of concurrently processed contexts does not exceed the specified maximum degree of
    ///     parallelism in the configuration. If the maximum degree of parallelism is set to zero, an exception is thrown. The
    ///     method waits for an available slot in the semaphore, processes the context using the <see cref="ProcessFunc" />
    ///     method, and writes the result to the channel. Finally, it releases the semaphore slot to allow other contexts to be
    ///     processed.
    /// </summary>
    /// <param name="context">The function context.</param>
    /// <param name="token">The cancellation token.</param>
    public new async Task EnqueJobAsync(TContext context, CancellationToken token = default)
    {
        try
        {
            await Semaphore.WaitAsync(token);
            var result = await ProcessFuncAsync(context, token);

            await Channel.Writer.WriteAsync(result, token);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    /// <summary>
    ///     Gets the results queue from already processed functions.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The new instance of <see cref="IAsyncEnumerable{T}" /> with <typeparamref name="TResult" /> elements.</returns>
    public new virtual IAsyncEnumerable<TResult> GetResultsAsync(CancellationToken token = default)
    {
        return Channel.Reader.ReadAllAsync(token);
    }
}

/// <summary>
///     The blocking synchronous functions executor.
/// </summary>
/// <param name="func">The function to execute.</param>
/// <param name="configuration">The lock executor configuration.</param>
/// <typeparam name="TContext">The context.</typeparam>
/// <typeparam name="TResult">The result.</typeparam>
[PublicAPI]
public sealed class BlockingLockExecutor<TContext, TResult>(
    Func<TContext, TResult> func,
    BlockingLockConfiguration? configuration = null) : BlockingExecutor<TContext, TResult>(configuration)
{
    /// <inheritdoc />
    protected override TResult ProcessFunc(TContext context, CancellationToken token)
    {
        return func(context);
    }
}

/// <summary>
///     The blocking asynchronouse functions executor.
/// </summary>
/// <param name="asyncFunc">The asynchronouse function.</param>
/// <param name="configuration">The lock executor configuration.</param>
/// <typeparam name="TContext">The context.</typeparam>
/// <typeparam name="TResult">The result.</typeparam>
[PublicAPI]
public sealed class BlockingLockAsyncExecutor<TContext, TResult>(
    Func<TContext, Task<TResult>> asyncFunc,
    BlockingLockConfiguration? configuration = null) : BlockingAsyncExecutor<TContext, TResult>(configuration)
{
    /// <inheritdoc />
    protected override async Task<TResult> ProcessFuncAsync(TContext context, CancellationToken token)
    {
        return await asyncFunc(context);
    }
}

/// <summary>
///     The blocking synchronouse actions processor.
/// </summary>
/// <param name="action">The synchronous action.</param>
/// <param name="configuration">The configuration.</param>
/// <typeparam name="TContext">The context.</typeparam>
[PublicAPI]
public sealed class BlockingLockProcessor<TContext>(
    Action<TContext> action,
    BlockingLockConfiguration? configuration = null) : BlockingExecutor<TContext, object?>(configuration)
{
    /// <inheritdoc />
    protected override object? ProcessFunc(TContext context, CancellationToken token)
    {
        action(context);
        return null;
    }

    /// <summary>
    ///     Gets the results queue from already processed functions. Since <see cref="BlockingLockProcessor{TContext}" /> is
    ///     designed for processing actions that do not produce any results, this method is not supported and will throw a
    ///     <see cref="NotSupportedException" /> if called.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    [Obsolete(
        $"{nameof(GetResultsAsync)} is not supported for {nameof(BlockingLockProcessor<>)} since it doesn't produce any results.",
        true)]
    public override IAsyncEnumerable<object?> GetResultsAsync(CancellationToken token = default)
    {
        throw new NotSupportedException(
            $"{nameof(GetResultsAsync)} is not supported for {nameof(BlockingLockProcessor<>)} since it doesn't produce any results.");
    }
}

/// <summary>
///     The blocking asynchronouse action processor.
/// </summary>
/// <param name="asyncAction">The asyncronouse action.</param>
/// <param name="configuration">The lock configuration.</param>
/// <typeparam name="TContext">The context.</typeparam>
[PublicAPI]
public sealed class BlockingLockAsyncProcessor<TContext>(
    Func<TContext, Task> asyncAction,
    BlockingLockConfiguration? configuration = null) : BlockingExecutor<TContext, Task>(configuration)
{
    /// <inheritdoc />
    protected override async Task ProcessFunc(TContext context, CancellationToken token)
    {
        await asyncAction(context);
    }

    /// <summary>
    ///     Gets the results queue from already processed functions. Since <see cref="BlockingLockAsyncProcessor{TContext}" />
    ///     is designed for processing actions that do not produce any results, this method is not supported and will throw a
    ///     <see cref="NotSupportedException" /> if called.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    [Obsolete(
        $"{nameof(GetResultsAsync)} is not supported for {nameof(BlockingLockAsyncProcessor<>)} since it doesn't produce any results.",
        true)]
    public override IAsyncEnumerable<Task> GetResultsAsync(CancellationToken token = default)
    {
        throw new NotSupportedException(
            $"{nameof(GetResultsAsync)} is not supported for {nameof(BlockingLockAsyncProcessor<>)} since it doesn't produce any results.");
    }
}

/// <summary>
///     The blocking lock configuration.
/// </summary>
[PublicAPI]
public record BlockingLockConfiguration
{
    /// <summary>
    ///     Initiates the new instance of <see cref="BlockingLockConfiguration" />.
    /// </summary>
    /// <param name="maxDegreeOfParallelism">The max degree of parallelism. Should be greater than 0.</param>
    /// <param name="resultsCacheCapacity">The results cache capacity. Should be greater than 0 if specified.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <remarks>If <paramref name="resultsCacheCapacity" /> not set, the cache will have infinite capacity.</remarks>
    public BlockingLockConfiguration(ushort maxDegreeOfParallelism, ushort? resultsCacheCapacity = null)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(maxDegreeOfParallelism, 0);
        if (resultsCacheCapacity.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfEqual(resultsCacheCapacity.Value, 0);
        }

        MaxDegreeOfParallelism = maxDegreeOfParallelism;
        ResultsCacheCapacity = resultsCacheCapacity;
    }

    /// <summary>
    ///     The max degree of parallelism.
    /// </summary>
    public ushort MaxDegreeOfParallelism { get; }

    /// <summary>
    ///     The results cache capacity. If the capacity is reached, the
    ///     <see cref="BlockingExecutor{TContext, TResult}.EnqueJobAsync" /> method will wait until there is a free slot in the
    ///     channel to write the result. If the capacity is set to zero, an exception is thrown.
    /// </summary>
    public ushort? ResultsCacheCapacity { get; }
}