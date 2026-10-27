using Flurl;
using Flurl.Http.Content;
using JetBrains.Annotations;

namespace Ebceys.Infrastructure.HttpClient;

/// <summary>
///     Extension class for DELETE HTTP requests on <see cref="ClientBase" />.
/// </summary>
[PublicAPI]
public static class ClientBaseDeleteExtensions
{
    /// <param name="client">The client instance.</param>
    extension(ClientBase client)
    {
        /// <summary>
        ///     Sends DELETE request without body and without response.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The cancellation token.</param>
        /// <typeparam name="TError">The error response type.</typeparam>
        /// <returns>The new instance of <see cref="OperationResult{TError}" />.</returns>
        public async Task<OperationResult<TError>> DeleteAsync<TError>(Action<Url> url,
            IDictionary<string, object>? headers = null,
            CancellationToken token = default) where TError : class
        {
            var request = await client.PrepareRequest(url, headers);

            var response = await client.ExecuteWithPolicyAsync(request, HttpMethod.Delete, token: token);

            return await OperationResult<TError>.CreateFromResponseAsync(response);
        }

        /// <summary>
        ///     Sends DELETE request without body and with awaiting <typeparamref name="TResponse" /> deserialized response.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The cancellation token.</param>
        /// <typeparam name="TResponse">The success response type.</typeparam>
        /// <typeparam name="TError">The error response type.</typeparam>
        /// <returns>The new instance of <see cref="OperationResult{TResponse,TError}" />.</returns>
        public async Task<OperationResult<TResponse, TError>> DeleteJsonAsync<TResponse, TError>(Action<Url> url,
            IDictionary<string, object>? headers = null,
            CancellationToken token = default) where TResponse : class where TError : class
        {
            var request = await client.PrepareRequest(url, headers);

            var response = await client.ExecuteWithPolicyAsync(request, HttpMethod.Delete, token: token);

            return await OperationResult<TResponse, TError>.CreateFromResponseAsync(response);
        }

        /// <summary>
        ///     Sends DELETE request with specified serialized body and without response.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="body">The body.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The cancellation token.</param>
        /// <typeparam name="TRequest">The request object type.</typeparam>
        /// <typeparam name="TError">The error response type.</typeparam>
        /// <returns>The new instance of <see cref="OperationResult{TError}" />.</returns>
        public async Task<OperationResult<TError>> DeleteJsonAsync<TRequest, TError>(Action<Url> url,
            TRequest body,
            IDictionary<string, object>? headers = null,
            CancellationToken token = default) where TError : class
        {
            var request = await client.PrepareRequest(url, headers);

            var serializedBody = client.DefaultSerializer.Serialize(body);
            request.Content = new CapturedJsonContent(serializedBody);

            var response = await client.ExecuteWithPolicyAsync(request, HttpMethod.Delete, token: token);

            return await OperationResult<TError>.CreateFromResponseAsync(response);
        }

        /// <summary>
        ///     Sends DELETE request with specified serialized body and with awaiting <typeparamref name="TResponse" />
        ///     deserialized response.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="body">The body.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The cancellation token.</param>
        /// <typeparam name="TRequest">The request object type.</typeparam>
        /// <typeparam name="TResponse">The success response type.</typeparam>
        /// <typeparam name="TError">The error response type.</typeparam>
        /// <returns>The new instance of <see cref="OperationResult{TResponse,TError}" />.</returns>
        public async Task<OperationResult<TResponse, TError>> DeleteJsonAsync<TRequest, TResponse, TError>(
            Action<Url> url,
            TRequest body,
            IDictionary<string, object>? headers = null,
            CancellationToken token = default) where TError : class where TResponse : class
        {
            var request = await client.PrepareRequest(url, headers);

            var serializedBody = client.DefaultSerializer.Serialize(body);
            request.Content = new CapturedJsonContent(serializedBody);

            var response = await client.ExecuteWithPolicyAsync(request, HttpMethod.Delete, token: token);

            return await OperationResult<TResponse, TError>.CreateFromResponseAsync(response);
        }

        /// <summary>
        ///     Sends DELETE request with specified stream body and without response.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="body">The body stream.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The cancellation token.</param>
        /// <typeparam name="TError">The error response type.</typeparam>
        /// <returns>The new instance of <see cref="OperationResult{TError}" />.</returns>
        public async Task<OperationResult<TError>> DeleteStreamAsync<TError>(Action<Url> url,
            Stream body,
            IDictionary<string, object>? headers = null,
            CancellationToken token = default) where TError : class
        {
            var request = await client.PrepareRequest(url, headers);

            request.Content = new StreamContent(body);

            var response = await client.ExecuteWithPolicyAsync(request, HttpMethod.Delete, token: token);

            return await OperationResult<TError>.CreateFromResponseAsync(response);
        }

        /// <summary>
        ///     Sends DELETE request with specified stream body and with awaiting <typeparamref name="TResponse" /> deserialized
        ///     response.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="body">The body stream.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The cancellation token.</param>
        /// <typeparam name="TResponse">The success response type.</typeparam>
        /// <typeparam name="TError">The error response type.</typeparam>
        /// <returns>The new instance of <see cref="OperationResult{TResponse,TError}" />.</returns>
        public async Task<OperationResult<TResponse, TError>> DeleteStreamAsync<TResponse, TError>(Action<Url> url,
            Stream body,
            IDictionary<string, object>? headers = null,
            CancellationToken token = default) where TError : class where TResponse : class
        {
            var request = await client.PrepareRequest(url, headers);

            request.Content = new StreamContent(body);

            var response = await client.ExecuteWithPolicyAsync(request, HttpMethod.Delete, token: token);

            return await OperationResult<TResponse, TError>.CreateFromResponseAsync(response);
        }
    }
}