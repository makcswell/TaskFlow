using Ebceys.Infrastructure.Exceptions;
using Ebceys.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Ebceys.Infrastructure.ControllerFilters;

/// <summary>
///     Exception filter that handles exceptions thrown during controller action execution.
///     Converts <see cref="ApiException" /> into a proper <see cref="ProblemDetailsResult" /> response,
///     and wraps all other exceptions into a generic 500 Internal Server Error <see cref="ProblemDetails" /> response.
/// </summary>
/// <param name="logger">The logger for error diagnostics.</param>
public class ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) : ExceptionFilterAttribute
{
    /// <inheritdoc />
    public override void OnException(ExceptionContext context)
    {
        logger.LogError(context.Exception, "Error on request processing!");
        if (context.Exception is ApiException apiException)
        {
            context.Result = new ProblemDetailsResult(apiException.ProblemDetails);
            return;
        }

        ProblemDetails details = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Detail = context.Exception.Message,
            Instance = FormatDetail(context.HttpContext.Request),
            Title = "Internal error!"
        };
        context.Result = new ProblemDetailsResult(details);
    }

    private static string FormatDetail(HttpRequest request)
    {
        return $"{request.Method}: {request.Path}";
    }
}