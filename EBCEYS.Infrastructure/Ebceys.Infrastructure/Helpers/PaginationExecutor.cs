using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Ebceys.Infrastructure.Helpers;

/// <summary>
///     Utility class for executing paginated queries. Automatically manages skip/take offsets
///     and yields results in batches until all data is consumed or max iterations are reached.
/// </summary>
[PublicAPI]
public static class PaginationExecutor
{
    /// <summary>
    ///     Executes the <paramref name="execution" /> with auto increments <see cref="PaginationData" />.
    /// </summary>
    /// <param name="execution">The execution func.</param>
    /// <param name="batchSize">The num of elements to take per iteration.</param>
    /// <param name="maxIterations">The max num of iterations.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TData"></typeparam>
    /// <exception cref="InvalidOperationException">Throws when <paramref name="maxIterations" /> is reached.</exception>
    /// <returns>Enumerates the <typeparamref name="TData" /> elements.</returns>
    public static async IAsyncEnumerable<TData> PaginationExecuteAsync<TData>(
        Func<PaginationData, CancellationToken, Task<ICollection<TData>>> execution,
        int batchSize,
        int maxIterations = 100,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        var paginationData = new PaginationData(batchSize);
        do
        {
            token.ThrowIfCancellationRequested();
            var collection = await execution(paginationData, token);

            foreach (var data in collection)
            {
                yield return data;
            }

            if (collection.Count < batchSize)
            {
                yield break;
            }

            if (paginationData.Iterations >= maxIterations)
            {
                throw new InvalidOperationException("Maximum number of iterations exceeded");
            }

            paginationData.NextIteration();
        } while (!token.IsCancellationRequested);
    }

    /// <summary>
    ///     Executes the <paramref name="execution" /> with auto increments <see cref="PaginationData" />.
    /// </summary>
    /// <param name="execution">The execution func.</param>
    /// <param name="batchSize">The num of elements to take per iteration.</param>
    /// <param name="maxIterations">The max num of iterations.</param>
    /// <typeparam name="TData"></typeparam>
    /// <exception cref="InvalidOperationException">Throws when <paramref name="maxIterations" /> is reached.</exception>
    /// <returns>Enumerates the <typeparamref name="TData" /> elements.</returns>
    public static IEnumerable<TData> PaginationExecute<TData>(
        Func<PaginationData, ICollection<TData>> execution,
        int batchSize,
        int maxIterations = 100)
    {
        var paginationData = new PaginationData(batchSize);
        do
        {
            var collection = execution(paginationData);
            foreach (var data in collection)
            {
                yield return data;
            }

            if (collection.Count < batchSize)
            {
                yield break;
            }

            if (paginationData.Iterations >= maxIterations)
            {
                throw new InvalidOperationException("Maximum number of iterations exceeded");
            }

            paginationData.NextIteration();
        } while (paginationData.Iterations < maxIterations);
    }
}

/// <summary>
///     Mutable struct that tracks pagination state (batch size, skip offset, and iteration count)
///     used by <see cref="PaginationExecutor" /> to manage paginated data retrieval.
/// </summary>
[PublicAPI]
public struct PaginationData
{
    /// <summary>
    ///     The num elements to take.
    /// </summary>
    public int BatchSize { get; } = 100;

    /// <summary>
    ///     The num elements to skip.
    /// </summary>
    public int Skip { get; private set; } = 0;

    /// <summary>
    ///     The num of iterations.
    /// </summary>
    public int Iterations { get; private set; } = 0;

    /// <summary>
    ///     Initiates the default instance of <see cref="PaginationData" />.
    /// </summary>
    public PaginationData()
    {
    }

    /// <summary>
    ///     Initiates the new instance of <see cref="PaginationData" /> with specified batch size.
    /// </summary>
    /// <param name="batchSize">The batch size.</param>
    /// <exception cref="ArgumentOutOfRangeException">If batch size negative or zero.</exception>
    public PaginationData(int batchSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);
        BatchSize = batchSize;
    }

    /// <summary>
    ///     Switch pagination data to next iteration.
    /// </summary>
    public void NextIteration()
    {
        Iterations++;
        Skip += BatchSize;
    }
}