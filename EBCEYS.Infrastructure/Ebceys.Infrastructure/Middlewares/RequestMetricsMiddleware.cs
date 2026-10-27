using Microsoft.AspNetCore.Http;
using Prometheus;

namespace Ebceys.Infrastructure.Middlewares;

/// <summary>
///     Middleware that collects Prometheus metrics for HTTP requests.
///     Tracks total request count with labels for path, method, and HTTP status code.
/// </summary>
/// <param name="next">The next middleware in the request pipeline.</param>
internal sealed class RequestMetricsMiddleware(RequestDelegate next)
{
    /// <summary>
    ///     The metrics endpoint path that is excluded from metrics collection to avoid recursion.
    /// </summary>
    public const string MetricsPath = "/metrics";

    /// <summary>
    ///     Invokes the next middleware and records Prometheus request metrics.
    /// </summary>
    /// <param name="httpContext">The HTTP context for the current request.</param>
    public async Task Invoke(HttpContext httpContext)
    {
        var path = httpContext.Request.Path;
        var method = httpContext.Request.Method;

        var counter = Metrics.CreateCounter("prometheus_demo_request_total", "HTTP Requests Total",
            new CounterConfiguration
            {
                LabelNames = ["path", "method", "status"]
            });

        var statusCode = 200;

        try
        {
            await next.Invoke(httpContext);
        }
        catch (Exception)
        {
            statusCode = 500;
            counter.Labels(path, method, statusCode.ToString()).Inc();

            throw;
        }

        if (!path.Value?.EndsWith(MetricsPath) == true)
        {
            statusCode = httpContext.Response.StatusCode;
            counter.Labels(path, method, statusCode.ToString()).Inc();
        }
    }
}