using Ebceys.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Ebceys.Infrastructure.Middlewares;

/// <summary>
///     Middleware that catches all unhandled exceptions (except <see cref="ApiException" />)
///     and wraps them into <see cref="ApiException" /> with a 500 status code.
///     This ensures a consistent error response format for all unhandled errors.
/// </summary>
/// <param name="next">The next middleware in the request pipeline.</param>
internal sealed class ExceptionCatcherMiddleware(RequestDelegate next)
{
    /// <summary>
    ///     Invokes the next middleware and catches any non-<see cref="ApiException" /> exceptions.
    /// </summary>
    /// <param name="httpContext">The HTTP context for the current request.</param>
    /// <exception cref="ApiException">Always thrown when a non-<see cref="ApiException" /> exception is caught.</exception>
    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await next.Invoke(httpContext);
        }
        catch (Exception ex) when (ex is not ApiException)
        {
            ApiExceptionHelper.ThrowException(ex);
        }
    }
}