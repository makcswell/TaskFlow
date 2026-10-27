using System.Net;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ebceys.Infrastructure.Extensions;

/// <summary>
///     Server-related extension methods including <see cref="ProblemDetails" /> factory and async wait utilities.
/// </summary>
[PublicAPI]
public static class EbServerExtensions
{
    private static readonly Dictionary<int, string> StatusCodes = Enum.GetValues<HttpStatusCode>()
        .DistinctBy(x => (int)x).ToDictionary(x => (int)x, x => x.ToString());

    extension(ProblemDetails)
    {
        /// <summary>
        ///     Creates the new instance of <see cref="ProblemDetails" /> by <paramref name="ctx" />.
        /// </summary>
        /// <param name="ctx">The http response.</param>
        /// <returns>The new instance of <see cref="ProblemDetails" />.</returns>
        public static ProblemDetails CreateFromResponse(HttpResponse ctx)
        {
            return new ProblemDetails
            {
                Status = ctx.StatusCode,
                Instance = ctx.HttpContext.Request.Path,
                Title = StatusCodes.TryGetValue(ctx.HttpContext.Response.StatusCode, out var statusCode)
                    ? statusCode
                    : ctx.HttpContext.Response.StatusCode.ToString()
            };
        }
    }

    extension(Task)
    {
        /// <summary>
        ///     Awaits until <paramref name="predicate" /> wont return true or <paramref name="timeout" />.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="token">The cancellation token.</param>
        public static async Task WaitUntilAsync(Predicate<CancellationToken> predicate, TimeSpan timeout,
            CancellationToken token = default)
        {
            var cts = new CancellationTokenSource(timeout);
            token.Register(cts.Cancel);

            while (!predicate(cts.Token) && !cts.IsCancellationRequested)
            {
                await Task.Delay(50, cts.Token);
            }
        }
    }
}