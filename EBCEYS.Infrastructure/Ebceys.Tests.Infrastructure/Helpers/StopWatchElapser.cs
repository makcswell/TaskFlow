using System.Diagnostics;
using JetBrains.Annotations;

namespace Ebceys.Tests.Infrastructure.Helpers;

/// <summary>
///     Disposable stopwatch wrapper that measures elapsed time between creation and disposal,
///     then invokes a callback with the elapsed <see cref="TimeSpan" />.
///     Useful for timing test operations in a <c>using</c> block.
/// </summary>
[PublicAPI]
public class StopWatchElapser : IDisposable
{
    private readonly Action<TimeSpan> _elapsedAction;
    private readonly Stopwatch _sw;

    private StopWatchElapser(Action<TimeSpan> elapsedAction)
    {
        _elapsedAction = elapsedAction;
        _sw = Stopwatch.StartNew();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _sw.Stop();
        _elapsedAction(_sw.Elapsed);
    }

    /// <summary>
    ///     Creates the new instance of <see cref="StopWatchElapser" />.
    /// </summary>
    /// <param name="elapsedAction">The on disposing action.</param>
    /// <returns>The new instance of <see cref="StopWatchElapser" />.</returns>
    public static IDisposable Create(Action<TimeSpan> elapsedAction)
    {
        return new StopWatchElapser(elapsedAction);
    }
}