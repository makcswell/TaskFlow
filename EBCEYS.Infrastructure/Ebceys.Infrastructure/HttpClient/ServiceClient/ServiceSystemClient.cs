using System.Text;
using Ebceys.Infrastructure.Controllers;
using Ebceys.Infrastructure.Exceptions;
using Ebceys.Infrastructure.HttpClient.Extensions;
using Ebceys.Infrastructure.Models;
using Flurl.Http.Configuration;
using HealthChecks.UI.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ebceys.Infrastructure.HttpClient.ServiceClient;

/// <summary>
///     Interface for a system-level HTTP client that interacts with <see cref="ServiceController" /> endpoints
///     of another service (ping, health check, health status, metrics).
/// </summary>
public interface IServiceSystemClient
{
    /// <summary>
    ///     Pings the service.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>Completed task if pinged successfully.</returns>
    /// <exception cref="ApiException">Api exception if catched error while requesting.</exception>
    Task PingAsync(CancellationToken token = default);

    /// <summary>
    ///     Health checks the service.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>Completed task if service is healthy.</returns>
    /// <exception cref="ApiException">Api exception if catched error while requesting or service is unhealthy.</exception>
    Task HealthCheckAsync(CancellationToken token = default);

    /// <summary>
    ///     Health checks the service with information.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The health check result.</returns>
    /// <exception cref="ApiException"></exception>
    Task<UIHealthReport> HealthStatusCheckAsync(CancellationToken token = default);

    /// <summary>
    ///     Gets the service metrics.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The string with service metrics.</returns>
    Task<string> GetMetricsAsync(CancellationToken token = default);
}

/// <summary>
///     Default implementation of <see cref="IServiceSystemClient" /> that communicates with
///     <see cref="ServiceController" /> endpoints of a remote service via HTTP.
/// </summary>
/// <param name="apiInfo">The service API info containing base address and service metadata.</param>
/// <param name="clientCache">The Flurl client cache.</param>
/// <param name="loggerFactory">The logger factory for request/response logging.</param>
/// <param name="baseUrlResolver">The resolver providing the target service base URL.</param>
/// <param name="tokenResolver">The optional resolver providing the authorization token.</param>
public class ServiceSystemClient(
    IOptions<ServiceApiInfo> apiInfo,
    IFlurlClientCache clientCache,
    ILoggerFactory loggerFactory,
    ClientBaseUrlResolver baseUrlResolver,
    ClientBaseTokenResolver? tokenResolver = null)
    : ClientBase(clientCache, loggerFactory, baseUrlResolver, tokenResolver), IServiceSystemClient
{
    /// <inheritdoc />
    public async Task PingAsync(CancellationToken token = default)
    {
        var response = await this.GetAsync<ProblemDetails>(
            url => url.AppendPathSegments(apiInfo.Value.BaseAddress, ServiceControllerRoutes.Controller,
                ServiceControllerRoutes.Methods.Ping),
            token: token);

        response.ThrowIfUnsuccess();
    }

    /// <inheritdoc />
    public async Task HealthCheckAsync(CancellationToken token = default)
    {
        var response = await this.GetAsync<ProblemDetails>(
            url => url.AppendPathSegments(apiInfo.Value.BaseAddress, ServiceControllerRoutes.Controller,
                ServiceControllerRoutes.Methods.Healthz),
            token: token);

        response.ThrowIfUnsuccess();
    }

    /// <inheritdoc />
    public async Task<UIHealthReport> HealthStatusCheckAsync(CancellationToken token = default)
    {
        var response = await this.GetJsonAsync<UIHealthReport, ProblemDetails>(
            url => url.AppendPathSegments(apiInfo.Value.BaseAddress, ServiceControllerRoutes.Controller,
                ServiceControllerRoutes.Methods.HealthzStatus),
            token: token);

        return response.GetResponseOrThrow();
    }

    /// <inheritdoc />
    public async Task<string> GetMetricsAsync(CancellationToken token = default)
    {
        var response = await this.GetRawAsync<ProblemDetails>(
            url => url.AppendPathSegments(apiInfo.Value.BaseAddress, ServiceControllerRoutes.Controller,
                ServiceControllerRoutes.Methods.Metrics)
            , token: token);

        if (!response.IsSuccess || response.Result is null)
        {
            return ApiExceptionHelper.ThrowApiException<string>(response.Error, response.StatusCode);
        }

        return Encoding.UTF8.GetString(response.Result);
    }
}