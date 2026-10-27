using Flurl;
using JetBrains.Annotations;

namespace Ebceys.Infrastructure.HttpClient;

/// <summary>
///     Extension class for GET HTTP requests on <see cref="ClientBase" />.
/// </summary>
[PublicAPI]
public static class ClientBaseGetExtensions
{
    /// <param name="client">The client instance.</param>
    extension(ClientBase client)
    {
        /// <summary>
        ///     Sends GET request with awaiting serialized body response.
        /// </summary>
        /// <param name="url">The url transform.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The cancellation token.</param>
        /// <typeparam name="TResponse">The success response type.</typeparam>
        /// <typeparam name="TError">The error response type.</typeparam>
        /// <returns>The new instance of <see cref="OperationResult{TResponse,TError}" />.</returns>
        public async Task<OperationResult<TResponse, TError>> GetJsonAsync<TResponse, TError>(Action<Url> url,
            IDictionary<string, object>? headers = null,
            CancellationToken token = default) where TResponse : class where TError : class
        {
            var request = await client.PrepareRequest(url, headers);

            var response = await client.ExecuteWithPolicyAsync(request, HttpMethod.Get, token: token);

            return await OperationResult<TResponse, TError>.CreateFromResponseAsync(response);
        }

        /// <summary>
        ///     Sends GET request.
        /// </summary>
        /// <param name="url">The url transform.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The cancellation token.</param>
        /// <typeparam name="TError">The error response type.</typeparam>
        /// <returns>The new instance of <see cref="OperationResult{TError}" />.</returns>
        public async Task<OperationResult<TError>> GetAsync<TError>(Action<Url> url,
            IDictionary<string, object>? headers = null,
            CancellationToken token = default) where TError : class
        {
            var request = await client.PrepareRequest(url, headers);

            var response = await client.ExecuteWithPolicyAsync(request, HttpMethod.Get, token: token);

            return await OperationResult<TError>.CreateFromResponseAsync(response);
        }

        /// <summary>
        ///     Sends GET request with awaiting <see cref="Stream" /> response.
        /// </summary>
        /// <param name="url">The url transform.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The cancellation token.</param>
        /// <typeparam name="TError">The error response type.</typeparam>
        /// <returns>The new instance of <see cref="OperationResult{Stream,TError}" />.</returns>
        public async Task<OperationResult<Stream, TError>> GetStreamAsync<TError>(Action<Url> url,
            IDictionary<string, object>? headers = null,
            CancellationToken token = default) where TError : class
        {
            var request = await client.PrepareRequest(url, headers);

            var response =
                await client.ExecuteWithPolicyAsync(request, HttpMethod.Get, HttpCompletionOption.ResponseHeadersRead,
                    token);

            return await OperationResult<Stream, TError>.CreateFromStreamResponseAsync(response);
        }

        /// <summary>
        ///     Sends GET request with awaiting <see cref="byte" /> array response.
        /// </summary>
        /// <param name="url">The url transform.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The cancellation token.</param>
        /// <typeparam name="TError">The error response type.</typeparam>
        /// <returns>The new instance of <see cref="OperationResult{T,TError}" /> where T is byte array.</returns>
        public async Task<OperationResult<byte[], TError>> GetRawAsync<TError>(Action<Url> url,
            IDictionary<string, object>? headers = null,
            CancellationToken token = default) where TError : class
        {
            var request = await client.PrepareRequest(url, headers);

            var response = await client.ExecuteWithPolicyAsync(request, HttpMethod.Get, token: token);

            return await OperationResult<byte[], TError>.CreateFromRawResponseAsync(response);
        }
    }
}