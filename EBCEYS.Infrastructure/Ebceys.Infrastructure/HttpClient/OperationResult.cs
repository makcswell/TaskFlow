using Flurl.Http;
using JetBrains.Annotations;

namespace Ebceys.Infrastructure.HttpClient;

/// <summary>
///     Represents the result of an HTTP operation that can either succeed with a <typeparamref name="TResponse" />
///     or fail with a <typeparamref name="TError" />. Includes the HTTP status code and success indicator.
/// </summary>
/// <typeparam name="TResponse">The type of the success response body.</typeparam>
/// <typeparam name="TError">The type of the error response body.</typeparam>
[PublicAPI]
public struct OperationResult<TResponse, TError>
    where TError : class
    where TResponse : class
{
    /// <summary>
    ///     The success result.
    /// </summary>
    public TResponse? Result { get; }

    /// <summary>
    ///     The error result.
    /// </summary>
    public TError? Error { get; }

    /// <summary>
    ///     Indicates that response is success.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    ///     The status code. Default is '-1'.
    /// </summary>
    public int StatusCode { get; }

    private OperationResult(TResponse? result, TError? error, bool isSuccess, int statusCode)
    {
        Result = result;
        Error = error;
        IsSuccess = isSuccess;
        StatusCode = statusCode;
    }

    /// <summary>
    ///     Creates the success result with specified result.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="statusCode">The status code.</param>
    /// <returns>The new instance of <see cref="OperationResult{TResponse,TError}" />.</returns>
    public static OperationResult<TResponse, TError> Success(TResponse result, int statusCode = -1)
    {
        return new OperationResult<TResponse, TError>(result, null, true, statusCode);
    }

    /// <summary>
    ///     Creates the failure result with specified error.
    /// </summary>
    /// <param name="result">The error result.</param>
    /// <param name="statusCode">The status code.</param>
    /// <returns>The new instance of <see cref="OperationResult{TResponse,TError}" />.</returns>
    public static OperationResult<TResponse, TError> Failure(TError result, int statusCode = -1)
    {
        return new OperationResult<TResponse, TError>(null, result, false, statusCode);
    }

    internal static async Task<OperationResult<TResponse, TError>> CreateFromResponseAsync(
        IFlurlResponse response)
    {
        var isSuccess = response.ResponseMessage.IsSuccessStatusCode;
        if (isSuccess)
        {
            var result = await response.GetJsonAsync<TResponse>();
            return Success(result, response.StatusCode);
        }

        var error = await response.GetJsonAsync<TError>();
        return Failure(error, response.StatusCode);
    }

    internal static async Task<OperationResult<Stream, TError>> CreateFromStreamResponseAsync(
        IFlurlResponse response)
    {
        var isSuccess = response.ResponseMessage.IsSuccessStatusCode;
        if (isSuccess)
        {
            var result = await response.GetStreamAsync();
            return OperationResult<Stream, TError>.Success(result, response.StatusCode);
        }

        var error = await response.GetJsonAsync<TError>();
        return OperationResult<Stream, TError>.Failure(error, response.StatusCode);
    }

    internal static async Task<OperationResult<byte[], TError>> CreateFromRawResponseAsync(
        IFlurlResponse response)
    {
        var isSuccess = response.ResponseMessage.IsSuccessStatusCode;
        if (isSuccess)
        {
            var result = await response.GetBytesAsync();
            return OperationResult<byte[], TError>.Success(result, response.StatusCode);
        }

        var error = await response.GetJsonAsync<TError>();
        return OperationResult<byte[], TError>.Failure(error, response.StatusCode);
    }
}

/// <summary>
///     Represents the result of an HTTP operation without a success response body.
///     Can either succeed (no body) or fail with a <typeparamref name="TError" />.
///     Includes the HTTP status code and success indicator.
/// </summary>
/// <typeparam name="TError">The type of the error response body.</typeparam>
[PublicAPI]
public struct OperationResult<TError>
    where TError : class
{
    /// <summary>
    ///     The error result.
    /// </summary>
    public TError? Error { get; }

    /// <summary>
    ///     Indicates that response is success.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    ///     The status code. Default is '-1'.
    /// </summary>
    public int StatusCode { get; }

    private OperationResult(TError? error, bool isSuccess, int statusCode)
    {
        Error = error;
        IsSuccess = isSuccess;
        StatusCode = statusCode;
    }

    /// <summary>
    ///     Creates the success result with specified result.
    /// </summary>
    /// <param name="statusCode">The status code.</param>
    /// <returns>The new instance of <see cref="OperationResult{TResponse,TError}" />.</returns>
    public static OperationResult<TError> Success(int statusCode = -1)
    {
        return new OperationResult<TError>(null, true, statusCode);
    }

    /// <summary>
    ///     Creates the failure result with specified error.
    /// </summary>
    /// <param name="result">The error result.</param>
    /// <param name="statusCode">The status code.</param>
    /// <returns>The new instance of <see cref="OperationResult{TResponse,TError}" />.</returns>
    public static OperationResult<TError> Failure(TError result, int statusCode = -1)
    {
        return new OperationResult<TError>(result, false, statusCode);
    }

    internal static async Task<OperationResult<TError>> CreateFromResponseAsync(
        IFlurlResponse response)
    {
        var isSuccess = response.ResponseMessage.IsSuccessStatusCode;
        if (isSuccess)
        {
            return Success(response.StatusCode);
        }

        var error = await response.GetJsonAsync<TError>();
        return Failure(error, response.StatusCode);
    }
}