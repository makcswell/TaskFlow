using System.Runtime.CompilerServices;
using Ebceys.Infrastructure.Extensions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Ebceys.Infrastructure.Exceptions;

/// <summary>
///     Custom exception that wraps <see cref="ProblemDetails" /> for consistent API error responses.
///     Caught by <see cref="ControllerFilters.ApiExceptionFilter" /> and converted to a proper HTTP response.
/// </summary>
[PublicAPI]
public class ApiException : Exception
{
    /// <summary>
    ///     Initiates the new instance of <see cref="ApiException" />.
    /// </summary>
    /// <param name="problemDetails">The problem details.</param>
    /// <param name="message">The message.</param>
    public ApiException(ProblemDetails problemDetails, string message) : base(
        $"{message}{Environment.NewLine}{problemDetails.ToDiagnosticJson()}")
    {
        ProblemDetails = problemDetails;
    }

    /// <summary>
    ///     Initiates the new instance of <see cref="ApiException" />.
    /// </summary>
    /// <param name="problemDetails">The problem details.</param>
    /// <param name="innerException">The inner exception.</param>
    public ApiException(ProblemDetails problemDetails, Exception innerException) : base(innerException.Message,
        innerException)
    {
        ProblemDetails = problemDetails;
    }

    /// <summary>
    ///     Initiates the new instance of <see cref="ApiException" />.
    /// </summary>
    /// <param name="statusCode">The status code.</param>
    /// <param name="message">The message.</param>
    /// <param name="memberName">The member name.</param>
    public ApiException(int statusCode, string message, [CallerMemberName] string memberName = "") : base(
        $"Status code: {statusCode} with message: {message}")
    {
        ProblemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = "Exception",
            Instance = memberName,
            Detail = message
        };
    }

    /// <summary>
    ///     Initiates the new instance of <see cref="ApiException" />.
    /// </summary>
    /// <param name="statusCode">The status code.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <param name="memberName">The member name.</param>
    public ApiException(int statusCode, Exception innerException, string memberName = "") : base(innerException.Message,
        innerException)
    {
        ProblemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = "Exception",
            Instance = memberName,
            Detail = innerException.Message
        };
    }

    /// <summary>
    ///     The problem details.
    /// </summary>
    public ProblemDetails ProblemDetails { get; init; }
}