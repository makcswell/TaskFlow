using System.Diagnostics;
using System.Text;
using Ebceys.Infrastructure.Attributes;
using Ebceys.Infrastructure.Controllers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ebceys.Infrastructure.Middlewares;

/// <summary>
///     Configuration options for the <see cref="RequestLoggingMiddleware" />. Allows fine-grained control
///     over which request/response paths and content types are logged, and at which log level.
/// </summary>
[PublicAPI]
public class HttpLoggingOptions
{
    /// <summary>
    ///     The path start to exclude from http logging.
    /// </summary>
    /// <example>/some/path*</example>
    public string[] PathStartExcludeLogging { get; set; } = [];

    /// <summary>
    ///     The path contains to exclude from http logging.
    /// </summary>
    /// <example>*/some/path*</example>
    public string[] PathContainsExcludeLogging { get; set; } = [];

    /// <summary>
    ///     The path end to exclude from http logging.
    /// </summary>
    /// <example>*/some/path</example>
    public string[] PathEndExcludeLogging { get; set; } = [];

    /// <summary>
    ///     The log level to log bodies.
    /// </summary>
    public LogLevel LogLevelToLogBodies { get; set; } = LogLevel.Debug;

    /// <summary>
    ///     The logging content types.
    /// </summary>
    public string[] LoggingContentTypes { get; set; } = [];
}

/// <summary>
///     Middleware that logs incoming HTTP requests and outgoing responses including method, URL, status code,
///     duration, and optionally request/response bodies. Respects <see cref="NoRequestBodyLoggingAttribute" />,
///     <see cref="NoResponseBodyLoggingAttribute" />, and <see cref="HttpLoggingOptions" /> settings.
/// </summary>
/// <param name="next">The next middleware in the request pipeline.</param>
/// <param name="loggingOpts">The HTTP logging options.</param>
/// <param name="logger">The logger instance.</param>
[PublicAPI]
public class RequestLoggingMiddleware(
    RequestDelegate next,
    IOptions<HttpLoggingOptions> loggingOpts,
    ILogger<RequestLoggingMiddleware> logger)
{
    /// <summary>
    ///     The string that will write to log if body should not be logged.
    /// </summary>
    public const string NoBodyLoggingString = "NO-LOGGING";

    private static readonly string[] ExtraNoLoggingPaths =
    [
        "/swagger/",
        "/metrics",
        $"/{ServiceControllerRoutes.Controller}/{ServiceControllerRoutes.Methods.Healthz}",
        $"/{ServiceControllerRoutes.Controller}/{ServiceControllerRoutes.Methods.Ping}",
        $"/{ServiceControllerRoutes.Controller}/{ServiceControllerRoutes.Methods.HealthzStatus}",
        $"/{ServiceControllerRoutes.Controller}/{ServiceControllerRoutes.Methods.Metrics}"
    ];

    private readonly string[] _loggingContentTypes =
    [
        ..loggingOpts.Value.LoggingContentTypes, "text/",
        "application/json",
        "application/xml"
    ];

    private readonly LogLevel _logLevelToLogBodies = loggingOpts.Value.LogLevelToLogBodies;

    /// <summary>
    ///     Invokes the context.
    /// </summary>
    /// <param name="context">The context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var noRequestLogging = endpoint?.Metadata.GetMetadata<NoRequestBodyLoggingAttribute>();

        var stopwatch = Stopwatch.StartNew();
        await LogRequest(context, noRequestLogging);

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            endpoint ??= context.GetEndpoint();
            var noResponseLogging = endpoint?.Metadata.GetMetadata<NoResponseBodyLoggingAttribute>();

            await LogResponse(context, responseBody, originalBodyStream, stopwatch.Elapsed, noResponseLogging);
        }
    }

    private async Task LogRequest(HttpContext context, NoRequestBodyLoggingAttribute? bodyLoggingAttribute)
    {
        context.Request.EnableBuffering();

        var request = context.Request;
        var shouldLog = ShouldLogRequestBody(context.Request, bodyLoggingAttribute);
        var requestBody = shouldLog ? await ReadRequestBody(request) : NoBodyLoggingString;

        logger.LogInformation("REQUEST({trace}) ==> {Method} {Path} {QueryString} {RequestBody}",
            context.TraceIdentifier,
            request.Method,
            request.GetDisplayUrl(),
            request.QueryString,
            requestBody);

        request.Body.Position = 0;
    }

    private async Task LogResponse(HttpContext context, MemoryStream responseBody, Stream originalBodyStream,
        TimeSpan duration, NoResponseBodyLoggingAttribute? bodyLoggingAttribute)
    {
        var shouldLog = ShouldLogResponseBody(context.Response, bodyLoggingAttribute);
        responseBody.Seek(0, SeekOrigin.Begin);
        var responseText = shouldLog ? await new StreamReader(responseBody).ReadToEndAsync() : NoBodyLoggingString;
        responseBody.Seek(0, SeekOrigin.Begin);

        logger.LogInformation(
            "<== RESPONSE({trace}): {Method} {StatusCode} {Path} {ContentType} {ResponseBody} {duration} ms",
            context.TraceIdentifier,
            context.Request.Method,
            context.Response.StatusCode,
            context.Request.GetDisplayUrl(),
            context.Response.ContentType,
            responseText,
            duration.Milliseconds);

        await responseBody.CopyToAsync(originalBodyStream);
    }

    private bool ShouldLogResponseBody(HttpResponse response, NoResponseBodyLoggingAttribute? bodyLoggingAttribute)
    {
        var contentType = response.ContentType?.ToLower() ?? "";

        var isInNoLogsList = IsInNoLogsList(response.HttpContext.Request.Path);

        return !isInNoLogsList
               && bodyLoggingAttribute is null
               && IsCorrectLogLevel()
               && IsCorrectContentType(contentType);
    }

    private bool ShouldLogRequestBody(HttpRequest request, NoRequestBodyLoggingAttribute? bodyLoggingAttribute)
    {
        var contentType = request.ContentType?.ToLower() ?? "";

        var isInNoLogsList = IsInNoLogsList(request.Path);

        return !isInNoLogsList
               && bodyLoggingAttribute is null
               && IsCorrectLogLevel()
               && IsCorrectContentType(contentType);
    }

    private bool IsInNoLogsList(PathString path)
    {
        var pathString = path.HasValue ? path.Value : string.Empty;
        return ExtraNoLoggingPaths.Any(x => pathString.Contains(x))
               || loggingOpts.Value.PathStartExcludeLogging.Any(x => pathString.StartsWith(x))
               || loggingOpts.Value.PathContainsExcludeLogging.Any(x => pathString.Contains(x))
               || loggingOpts.Value.PathEndExcludeLogging.Any(x => pathString.EndsWith(x));
    }

    private bool IsCorrectContentType(string contentType)
    {
        return _loggingContentTypes.Aggregate(false,
            (current, contentTypes) => current | contentType.Contains(contentTypes));
    }

    private bool IsCorrectLogLevel()
    {
        return logger.IsEnabled(_logLevelToLogBodies);
    }

    private async Task<string> ReadRequestBody(HttpRequest request)
    {
        if (request.ContentLength is null or 0)
        {
            return string.Empty;
        }

        try
        {
            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                false,
                1024,
                true);

            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read request body");
            return "[Error reading request body]";
        }
    }
}