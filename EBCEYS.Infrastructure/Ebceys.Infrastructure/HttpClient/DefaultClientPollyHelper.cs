using Flurl.Http;
using JetBrains.Annotations;
using Polly;

namespace Ebceys.Infrastructure.HttpClient;

/// <summary>
///     Helper class that creates default Polly retry policies for HTTP requests
///     made via Flurl. Handles <see cref="FlurlHttpException" /> with a single retry after the specified delay.
/// </summary>
[PublicAPI]
public static class DefaultClientPollyHelper
{
    /// <summary>
    ///     Creates the default http executing policy.
    /// </summary>
    /// <param name="delay">The delay between retries.</param>
    /// <param name="onRetryAction">The action that could be called on retry.</param>
    /// <typeparam name="TResult">The policy execution result.</typeparam>
    /// <returns>The new instance of configured policy.</returns>
    public static IAsyncPolicy<TResult> CreateDefaultHttpPolly<TResult>(
        TimeSpan delay,
        Action<Exception, TimeSpan, int, Context>? onRetryAction = null)
    {
        var policy = Policy.Handle<FlurlHttpException>()
            .WaitAndRetryAsync([delay], (exception, span, retry, ctx)
                => onRetryAction?.Invoke(exception, span, retry, ctx))
            .AsAsyncPolicy<TResult>();
        return policy;
    }
}