using Ebceys.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Ebceys.Infrastructure.Extensions;

/// <summary>
///     The <see cref="IApplicationBuilder" /> extensions.
/// </summary>
internal static class EbIApplicationBuilderExtensions
{
    /// <param name="app">The application builder.</param>
    extension(IApplicationBuilder app)
    {
        /// <summary>
        ///     Adds the <see cref="RequestLoggingMiddleware" /> to <paramref name="app" />.
        /// </summary>
        public void UseRequestLogging()
        {
            app.UseMiddleware<RequestLoggingMiddleware>();
        }

        /// <summary>
        ///     Adds the <see cref="ExceptionCatcherMiddleware" /> to <paramref name="app" />.
        /// </summary>
        public void UseExceptionCatcher()
        {
            app.UseMiddleware<ExceptionCatcherMiddleware>();
        }

        /// <summary>
        ///     Adds the <see cref="RequestMetricsMiddleware" /> to <paramref name="app" />.
        /// </summary>
        public void UseRequestMetrics()
        {
            app.UseMiddleware<RequestMetricsMiddleware>();
        }
    }
}