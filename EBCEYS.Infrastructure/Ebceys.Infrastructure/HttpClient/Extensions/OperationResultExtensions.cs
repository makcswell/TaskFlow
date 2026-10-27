using System.Runtime.CompilerServices;
using Ebceys.Infrastructure.Exceptions;
using JetBrains.Annotations;

namespace Ebceys.Infrastructure.HttpClient.Extensions;

/// <summary>
///     The <see cref="OperationResult{TResponse,TError}" /> and <see cref="OperationResult{TError}" /> extensions.
/// </summary>
[PublicAPI]
public static class OperationResultExtensions
{
    /// <summary>
    ///     Throws the <see cref="ApiException" /> if is not <see cref="OperationResult{TError}.IsSuccess" />.
    /// </summary>
    /// <exception cref="ApiException">Throw if not <see cref="OperationResult{TError}.IsSuccess" />.</exception>
    public static void ThrowIfUnsuccess<TError>(this OperationResult<TError> result,
        [CallerMemberName] string memberName = "") where TError : class
    {
        if (!result.IsSuccess)
        {
            ApiExceptionHelper.ThrowApiException(result, memberName);
        }
    }

    extension<TResponse, TError>(OperationResult<TResponse, TError> result) where TResponse : class where TError : class
    {
        /// <summary>
        ///     Gets the response if <see cref="OperationResult{TResponse, TError}.IsSuccess" /> and
        ///     <see cref="OperationResult{TResponse, TError}.Result" /> is not null.<br />
        ///     Otherwise throws the <see cref="ApiException" />.
        /// </summary>
        /// <returns>The <see cref="OperationResult{TResponse, TError}.Result" />.</returns>
        /// <exception cref="ApiException">Throw if not <see cref="OperationResult{TResponse, TError}.IsSuccess" />.</exception>
        public TResponse GetResponseOrThrow([CallerMemberName] string memberName = "")
        {
            if (result is { IsSuccess: true, Result: not null })
            {
                return result.Result;
            }

            return ApiExceptionHelper.ThrowApiException(result, memberName);
        }

        /// <summary>
        ///     Gets the response if <see cref="OperationResult{TResponse, TError}.IsSuccess" /> without check
        ///     <see cref="OperationResult{TResponse, TError}.Result" /> is null.<br />
        ///     Otherwise throws the <see cref="ApiException" />.
        /// </summary>
        /// <returns>The <see cref="OperationResult{TResponse, TError}.Result" /> if exists; otherwise null.</returns>
        /// <exception cref="ApiException">Throw if not <see cref="OperationResult{TResponse, TError}.IsSuccess" />.</exception>
        public TResponse? GetResponseOrThrowWithoutNullCheck([CallerMemberName] string memberName = "")
        {
            if (result.IsSuccess)
            {
                return result.Result;
            }

            return ApiExceptionHelper.ThrowApiException(result, memberName);
        }
    }
}