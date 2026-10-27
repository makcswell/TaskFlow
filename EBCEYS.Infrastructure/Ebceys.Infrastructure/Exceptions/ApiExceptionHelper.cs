using System.Runtime.CompilerServices;
using Ebceys.Infrastructure.Extensions;
using Ebceys.Infrastructure.HttpClient;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ebceys.Infrastructure.Exceptions;

/// <summary>
///     Static helper class providing convenient methods to throw <see cref="ApiException" /> instances
///     for common HTTP error scenarios (NotFound, Conflict, Validation, InternalServerError, etc.).
/// </summary>
[PublicAPI]
public static class ApiExceptionHelper
{
    /// <summary>
    ///     Throws not found exception.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static TResult ThrowNotFound<TResult>(string message, [CallerMemberName] string memberName = "")
    {
        throw new ApiException(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "NotFound",
            Instance = memberName,
            Detail = message
        }, message);
    }

    /// <summary>
    ///     Throws not found exception.
    /// </summary>
    /// <param name="exception">The inner exception.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TException"></typeparam>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static TResult ThrowNotFound<TResult, TException>(TException exception,
        [CallerMemberName] string memberName = "")
        where TException : Exception
    {
        throw new ApiException(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "NotFound",
            Instance = memberName,
            Detail = exception.Message
        }, exception);
    }

    /// <summary>
    ///     Throws the conflict exception.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static TResult ThrowConflict<TResult>(string message, [CallerMemberName] string memberName = "")
    {
        throw new ApiException(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict",
            Instance = memberName,
            Detail = message
        }, message);
    }

    /// <summary>
    ///     Throws conflict exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TException"></typeparam>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static TResult ThrowConflict<TResult, TException>(TException exception,
        [CallerMemberName] string memberName = "")
        where TException : Exception
    {
        throw new ApiException(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict",
            Instance = memberName,
            Detail = exception.Message
        }, exception);
    }

    /// <summary>
    ///     Throws the validation exception.
    /// </summary>
    /// <param name="problemDetails">The problem details.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static TResult ThrowValidation<TResult>(ValidationProblemDetails problemDetails,
        [CallerMemberName] string memberName = "")
    {
        throw new ApiException(problemDetails, problemDetails.Title ?? "Validation");
    }

    /// <summary>
    ///     Throws the exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TException"></typeparam>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static TResult ThrowException<TResult, TException>(TException exception,
        int statusCode = StatusCodes.Status500InternalServerError, [CallerMemberName] string memberName = "")
        where TException : Exception
    {
        throw new ApiException(statusCode, exception, memberName);
    }

    /// <summary>
    ///     Throws the exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TException"></typeparam>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static void ThrowException<TException>(TException exception,
        int statusCode = StatusCodes.Status500InternalServerError, [CallerMemberName] string memberName = "")
        where TException : Exception
    {
        throw new ApiException(statusCode, exception, memberName);
    }

    /// <summary>
    ///     Throws the api exception.
    /// </summary>
    /// <param name="problemDetails">The problem details.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static TResult ThrowApiException<TResult>(ProblemDetails? problemDetails, int? statusCode = null,
        [CallerMemberName] string memberName = "")
    {
        ThrowApiException(problemDetails, statusCode, memberName);
        return default!;
    }

    /// <summary>
    ///     Throws the api exception.
    /// </summary>
    /// <param name="problemDetails">The problem details.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="memberName">The member name.</param>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static void ThrowApiException(ProblemDetails? problemDetails, int? statusCode = null,
        [CallerMemberName] string memberName = "")
    {
        statusCode ??= problemDetails?.Status ?? StatusCodes.Status500InternalServerError;
        if (problemDetails is null)
        {
            problemDetails ??= new ProblemDetails
            {
                Status = statusCode,
                Title = "API Error",
                Detail = "Null problem details",
                Instance = memberName
            };
            throw new ApiException(problemDetails, "Null problem details");
        }

        problemDetails.Instance = memberName;

        throw new ApiException(problemDetails, problemDetails.Title ?? "Problem details");
    }

    /// <summary>
    ///     Throws the api exception created by <paramref name="operationResult" />.
    /// </summary>
    /// <param name="operationResult">The operation result.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TError">The error.</typeparam>
    /// <exception cref="ApiException">The api exception.</exception>
    public static void ThrowApiException<TError>(OperationResult<TError> operationResult,
        [CallerMemberName] string memberName = "") where TError : class
    {
        if (operationResult.Error is ProblemDetails problemDetails)
        {
            ThrowApiException(problemDetails, operationResult.StatusCode, memberName);
        }

        throw new ApiException(operationResult.StatusCode, operationResult.Error?.ToDiagnosticJson() ?? string.Empty,
            memberName);
    }

    /// <summary>
    ///     Throws the api exception created by <paramref name="operationResult" />.
    /// </summary>
    /// <param name="operationResult">The operation result.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TError">The error.</typeparam>
    /// <typeparam name="TResult">The result.</typeparam>
    /// <exception cref="ApiException">The api exception.</exception>
    public static TResult ThrowApiException<TResult, TError>(OperationResult<TError> operationResult,
        [CallerMemberName] string memberName = "") where TError : class
    {
        if (operationResult.Error is ProblemDetails problemDetails)
        {
            ThrowApiException(problemDetails, operationResult.StatusCode, memberName);
        }

        throw new ApiException(operationResult.StatusCode, operationResult.Error?.ToDiagnosticJson() ?? string.Empty,
            memberName);
    }

    /// <summary>
    ///     Throws the api exception created by <paramref name="operationResult" />.
    /// </summary>
    /// <param name="operationResult">The operation result.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TError">The error.</typeparam>
    /// <typeparam name="TResult">The result.</typeparam>
    /// <exception cref="ApiException">The api exception.</exception>
    public static TResult ThrowApiException<TResult, TError>(OperationResult<TResult, TError> operationResult,
        [CallerMemberName] string memberName = "") where TError : class where TResult : class
    {
        if (operationResult.Error is ProblemDetails problemDetails)
        {
            ThrowApiException(problemDetails, operationResult.StatusCode, memberName);
        }

        throw new ApiException(operationResult.StatusCode, operationResult.Error?.ToDiagnosticJson() ?? string.Empty,
            memberName);
    }
}