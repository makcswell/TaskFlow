using System.Collections.Concurrent;

namespace Ebceys.Infrastructure.Tests.Helpers;

internal class LogCatcherMiddleware<T> : ILogger<T>
{
    public ConcurrentBag<LogInformation> Logs { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Logs.Add(new LogInformation(logLevel, formatter(state, exception)));
    }
}

internal record LogInformation(LogLevel LogLevel, string Message);