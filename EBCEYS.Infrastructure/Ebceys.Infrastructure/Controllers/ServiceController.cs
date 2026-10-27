using Ebceys.Infrastructure.Exceptions;
using Ebceys.Infrastructure.Extensions;
using HealthChecks.UI.Core;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;

namespace Ebceys.Infrastructure.Controllers;

/// <summary>
///     Built-in API controller that provides service infrastructure endpoints including
///     ping, health checks, health status reports, and Prometheus metrics.
/// </summary>
/// <param name="health">The health check service used to verify service health.</param>
[ApiController]
[Route(ServiceControllerRoutes.Controller)]
[ProducesErrorResponseType(typeof(ProblemDetails))]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public class ServiceController(HealthCheckService health) : ControllerBase
{
    /// <summary>
    ///     Simple ping endpoint that returns "pong" to verify the service is reachable.
    /// </summary>
    /// <response code="200">Pong</response>
    /// <returns>An <see cref="IActionResult" /> containing the "pong" string.</returns>
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [HttpGet(ServiceControllerRoutes.Methods.Ping)]
    public IActionResult Ping()
    {
        return Ok("pong");
    }

    /// <summary>
    ///     Health checks the service without body payload.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">Service is healthy.</response>
    /// <response code="500">Service is unhealthy.</response>
    [HttpGet(ServiceControllerRoutes.Methods.Healthz)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HealthCheckAsync(CancellationToken cancellationToken)
    {
        var healths = await health.CheckHealthAsync(cancellationToken);
        return healths.Status == HealthStatus.Healthy &&
               healths.Entries.All(x => x.Value.Status == HealthStatus.Healthy)
            ? Ok()
            : Unhealthy(healths);
    }

    /// <summary>
    ///     Health checks the service with response body payload.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">Service health info.</response>
    [HttpGet(ServiceControllerRoutes.Methods.HealthzStatus)]
    [ProducesResponseType<UIHealthReport>(StatusCodes.Status200OK)]
    public async Task<IActionResult> HealthCheckStatusAsync(CancellationToken cancellationToken)
    {
        var healths = await health.CheckHealthAsync(cancellationToken);

        return Ok(healths);
    }

    /// <summary>
    ///     Gets the service metrics.
    /// </summary>
    /// <response code="200">The service metrics.</response>
    [HttpGet(ServiceControllerRoutes.Methods.Metrics)]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    public async Task GetMetricsAsync(CancellationToken token)
    {
        Response.ContentType = "text/plain; version=0.0.4; charset=utf-8";

        await Metrics.DefaultRegistry.CollectAndExportAsTextAsync(Response.Body, token);
    }

    [HttpGet(ServiceControllerRoutes.Methods.Healthz)]
    private static IActionResult Unhealthy(HealthReport healths)
    {
        return ApiExceptionHelper.ThrowApiException<IActionResult>(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = healths.Status.ToString(),
            Detail = healths.Entries.Select(x => $"{x.Key} - {x.Value.Status}").Join(Environment.NewLine),
            Instance = nameof(HealthCheckService)
        });
    }
}

/// <summary>
///     Contains route constants for the <see cref="ServiceController" />.
/// </summary>
[PublicAPI]
public static class ServiceControllerRoutes
{
    /// <summary>
    ///     The controller path.
    /// </summary>
    public const string Controller = "service";

    /// <summary>
    ///     The <see cref="ServiceController" /> methods.
    /// </summary>
    public static class Methods
    {
        /// <summary>
        ///     Ping.
        /// </summary>
        public const string Ping = "ping";

        /// <summary>
        ///     The health check without response body payload.
        /// </summary>
        public const string Healthz = "healthz";

        /// <summary>
        ///     The health check with response body payload.
        /// </summary>
        public const string HealthzStatus = "healthz/status";

        /// <summary>
        ///     The service metrics.
        /// </summary>
        public const string Metrics = "metrics";
    }
}