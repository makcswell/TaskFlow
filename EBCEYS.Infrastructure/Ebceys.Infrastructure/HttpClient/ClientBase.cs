using System.Diagnostics;
using Ebceys.Infrastructure.Helpers.Json;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Content;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Polly;

namespace Ebceys.Infrastructure.HttpClient;

/// <summary>
///     Abstract base class for HTTP clients built on Flurl. Provides standardized methods for
///     GET, POST, PUT, DELETE requests with JSON serialization/deserialization, automatic authorization
///     header injection, Polly retry policies, and structured logging of requests and responses.
/// </summary>
[PublicAPI]
public abstract class ClientBase
{
    /// <summary>
    ///     The authorization header key.
    /// </summary>
    public const string AuthorizationHeader = "Authorization";

    private readonly IFlurlClient _client;
    private readonly ILogger _logger;

    private readonly ClientBaseTokenResolver? _tokenResolver;

    /// <summary>
    ///     Initiates the new instance of <see cref="ClientBase" />.
    /// </summary>
    /// <param name="clientCache">The client cache.</param>
    /// <param name="baseUrlResolver">The base url resolver.</param>
    /// <param name="tokenResolver">The token resolver.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    protected ClientBase(
        IFlurlClientCache clientCache,
        ILoggerFactory loggerFactory,
        ClientBaseUrlResolver baseUrlResolver,
        ClientBaseTokenResolver? tokenResolver = null)
    {
        _tokenResolver = tokenResolver;
        Policy = GetExecutionPolicy();
        _client = clientCache.GetOrAdd(ClientName, baseUrlResolver.Invoke(), ConfigureClient);
        _logger = loggerFactory.CreateLogger(GetType());
    }

    /// <summary>
    ///     The client name to get it from <see cref="IFlurlClientCache" />.
    /// </summary>
    protected string ClientName => GetClientName(GetType());

    /// <summary>
    ///     The default response awaiting timeout.
    /// </summary>
    protected TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>
    ///     The default delay between retries.
    /// </summary>
    protected TimeSpan DelayBetweenRetries { get; init; } = TimeSpan.FromSeconds(3);

    /// <summary>
    ///     The default http request executing policy.
    /// </summary>
    protected IAsyncPolicy<IFlurlResponse> Policy { get; init; }

    /// <summary>
    ///     The default request and response serializer.
    /// </summary>
    public ISerializer DefaultSerializer { get; init; } = DefaultJsonSerializerOptions.DefaultJsonSerializer;

    private void ConfigureClient(IFlurlClientBuilder obj)
    {
        obj.AllowAnyHttpStatus();
        obj.WithTimeout(Timeout);
        obj.WithSettings(act => { act.JsonSerializer = DefaultSerializer; });
    }

    /// <summary>
    ///     Gets the client name. With this name the client will get from <see cref="IFlurlClientCache" />.
    /// </summary>
    /// <param name="implementationType">The client implementation type.</param>
    /// <returns></returns>
    public static string GetClientName(Type implementationType)
    {
        return $"{implementationType.Name}{nameof(ClientBase)}";
    }


    /// <summary>
    ///     Executes the flurl request with <see cref="Policy" />.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="method">The http method.</param>
    /// <param name="completionOption">The completion options.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The <see cref="Task" /> which result contains the new instance of <see cref="IFlurlResponse" />.</returns>
    protected internal async Task<IFlurlResponse> ExecuteWithPolicyAsync(
        IFlurlRequest request,
        HttpMethod method,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken token = default)
    {
        var requestId = Guid.CreateVersion7().ToString("N").ToUpper();
        LogRequest(method, request, requestId);
        var stopwatch = Stopwatch.StartNew();
        var result = await Policy.ExecuteAsync(async () =>
        {
            return await ExecuteWithoutPolicyAsync(
                (r, t) => r.SendAsync(method, request.Content, completionOption, t),
                request,
                token);
        });
        stopwatch.Stop();
        LogResponse(method, result, request, stopwatch.Elapsed, requestId);
        return result;
    }

    /// <summary>
    ///     Executes the request delegate without policy.
    /// </summary>
    /// <param name="func">The delegate to execute.</param>
    /// <param name="request">The request to be executed.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The <see cref="Task" /> which result contains the new instance of <see cref="IFlurlResponse" />.</returns>
    protected virtual Task<IFlurlResponse> ExecuteWithoutPolicyAsync(
        Func<IFlurlRequest, CancellationToken, Task<IFlurlResponse>> func,
        IFlurlRequest request,
        CancellationToken token = default)
    {
        return func.Invoke(request, token);
    }

    /// <summary>
    ///     Prepares the <see cref="IFlurlRequest" /> with specified <paramref name="url" /> and <paramref name="headers" />.
    /// </summary>
    /// <param name="url">The url configure action.</param>
    /// <param name="headers">The http headers.</param>
    /// <returns>The new instance of <see cref="IFlurlRequest" />.</returns>
    /// <remarks>To headers the <see cref="GetDefaultHeaders" /> method will applied.</remarks>
    protected internal async Task<IFlurlRequest> PrepareRequest(Action<Url> url, IDictionary<string, object>? headers)
    {
        var requestHeaders = await GetDefaultHeaders(headers);
        var request = _client.Request().WithHeaders(requestHeaders);
        url.Invoke(request.Url);
        return request;
    }

    /// <summary>
    ///     Gets the default <see cref="ClientBase" /> request headers.
    /// </summary>
    /// <param name="additionalHeaders">The additional headers.</param>
    /// <returns>The headers for <see cref="IFlurlRequest" />.</returns>
    /// <remarks>
    ///     If <paramref name="additionalHeaders" /> contains <see cref="AuthorizationHeader" /> the token resolver won't
    ///     call.
    /// </remarks>
    protected internal async Task<Dictionary<string, object>> GetDefaultHeaders(
        IDictionary<string, object>? additionalHeaders = null)
    {
        var headers = new Dictionary<string, object>();
        if (_tokenResolver is not null && !(additionalHeaders?.ContainsKey(AuthorizationHeader) ?? false))
        {
            var token = await _tokenResolver.Invoke();
            if (token is not null)
            {
                headers.Add(AuthorizationHeader, token);
            }
        }

        if (additionalHeaders == null)
        {
            return headers;
        }

        foreach (var header in additionalHeaders)
        {
            headers.Add(header.Key, header.Value);
        }

        return headers;
    }

    /// <summary>
    ///     Creates the <see cref="IAsyncPolicy{TResult}" /> that will be called on <see cref="ExecuteWithPolicyAsync" />
    ///     execution.
    /// </summary>
    /// <returns>The new instance of <see cref="IAsyncPolicy{TResult}" />.</returns>
    /// <remarks>By default the <see cref="DefaultClientPollyHelper.CreateDefaultHttpPolly" /> will creates.</remarks>
    protected virtual IAsyncPolicy<IFlurlResponse>? CreateExecutionPolicy()
    {
        return null;
    }

    private IAsyncPolicy<IFlurlResponse> GetExecutionPolicy()
    {
        return CreateExecutionPolicy() ?? DefaultClientPollyHelper.CreateDefaultHttpPolly<IFlurlResponse>(Timeout);
    }

    private void LogRequest(
        HttpMethod method,
        IFlurlRequest request,
        string requestId)
    {
        if (_logger.IsEnabled(LogLevel.Debug)
            && request.Content is CapturedJsonContent content)
        {
            _logger.LogDebug("[Send {id}] {method} {url} with body:\n{body}", requestId, method, request.Url,
                content.Content);
            return;
        }

        _logger.LogInformation("[Send {id}] {method} {url}", requestId, method, request.Url);
    }

    private void LogResponse(
        HttpMethod method,
        IFlurlResponse response,
        IFlurlRequest request,
        TimeSpan duration,
        string requestId)
    {
        if (_logger.IsEnabled(LogLevel.Debug)
            && response.ResponseMessage.Content is CapturedJsonContent content)
        {
            _logger.LogDebug("[Receive {id}] {method} {url} {statusCode} for {duration}ms with body:\n{body}",
                requestId,
                method,
                request.Url,
                response.StatusCode,
                duration.TotalMilliseconds,
                content.Content);
            return;
        }

        _logger.LogInformation("[Receive {id}] {method} {url} {statusCode} for {duration}ms",
            requestId,
            method,
            request.Url,
            response.StatusCode,
            duration.TotalMilliseconds);
    }

    /// <summary>
    ///     Gets the auth headers.
    /// </summary>
    /// <param name="token">The jwt.</param>
    /// <returns>The new instance of request headers.</returns>
    protected static Dictionary<string, object> GetAuthHeaders(string token)
    {
        return new Dictionary<string, object>
        {
            { AuthorizationHeader, token }
        };
    }
}