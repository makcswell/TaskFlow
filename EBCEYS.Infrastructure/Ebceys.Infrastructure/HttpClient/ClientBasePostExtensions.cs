using Flurl;
using Flurl.Http.Content;
using JetBrains.Annotations;

namespace Ebceys.Infrastructure.HttpClient;

/// <summary>
///     Extension class for POST HTTP requests on <see cref="ClientBase" />.
/// </summary>
[PublicAPI]
public static class ClientBasePostExtensions
{
    /// <param name="client">The client instance.</param>
    extension(ClientBase client)
    {
        /// <summary>
        ///     Sends POST request without body and without response.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The cancellation token.</param>
        /// <typeparam name="TError">The error response type.</typeparam>
        /// <returns>The new instance of <see cref="OperationResult{TError}" />.</returns>
        public async Task<OperationResult<TError>> PostAsync<TError>(Action<Url> url,
            IDictionary<string, object>? headers = null,
            CancellationToken token = default) where TError : class
        {
            var request = await client.PrepareRequest(url, headers);

            var response = await client.ExecuteWithPolicyAsync(request, HttpMethod.Post, token: token);

            return await OperationResult<TError>.CreateFromResponseAsync(response);
        }

        /// <summary>
        ///     Sends POST request with specified serialized body and without response.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="body">The body.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The cancellation token.</param>
        /// <typeparam name="TRequest">The request object type.</typeparam>
        /// <typeparam name="TError">The error response type.</typeparam>
        /// <returns>The new instance of <see cref="OperationResult{TError}" />.</returns>
        public async Task<OperationResult<TError>> PostJsonAsync<TRequest, TError>(Action<Url> url,
            TRequest body,
            IDictionary<string, object>? headers = null,
            CancellationToken token = default) where TError : class
        {
            var request = await client.PrepareRequest(url, headers);

            var serializedBody = client.DefaultSerializer.Serialize(body);
            request.Content = new CapturedJsonContent(serializedBody);

            var response = await client.ExecuteWithPolicyAsync(request, HttpMethod.Post, token: token);

            return await OperationResult<TError>.CreateFromResponseAsync(response);
        }

        /// <summary>
        ///     Sends POST request with specified serialized body and with awaiting <typeparamref name="TResponse" /> deserialized
        ///     response.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="body">The body.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The cancellation token.</param>
        /// <typeparam name="TRequest">The request object type.</typeparam>
        /// <typeparam name="TResponse">The success response type.</typeparam>
        /// <typeparam name="TError">The error response type.</typeparam>
        /// <returns>The new instance of <see cref="OperationResult{TResponse,TError}" />.</returns>
        public async Task<OperationResult<TResponse, TError>> PostJsonAsync<TRequest, TResponse, TError>(
            Action<Url> url,
            TRequest body,
            IDictionary<string, object>? headers = null,
            CancellationToken token = default) where TError : class where TResponse : class
        {
            var request = await client.PrepareRequest(url, headers);

            var serializedBody = client.DefaultSerializer.Serialize(body);
            request.Content = new CapturedJsonContent(serializedBody);

            var response = await client.ExecuteWithPolicyAsync(request, HttpMethod.Post, token: token);

            return await OperationResult<TResponse, TError>.CreateFromResponseAsync(response);
        }

        /// <summary>
        ///     Sends POST request without body and with awaiting <typeparamref name="TResponse" /> deserialized response.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The cancellation token.</param>
        /// <typeparam name="TResponse">The success response type.</typeparam>
        /// <typeparam name="TError">The error response type.</typeparam>
        /// <returns>The new instance of <see cref="OperationResult{TResponse,TError}" />.</returns>
        public async Task<OperationResult<TResponse, TError>> PostJsonAsync<TResponse, TError>(Action<Url> url,
            IDictionary<string, object>? headers = null,
            CancellationToken token = default) where TError : class where TResponse : class
        {
            var request = await client.PrepareRequest(url, headers);

            var response = await client.ExecuteWithPolicyAsync(request, HttpMethod.Post, token: token);

            return await OperationResult<TResponse, TError>.CreateFromResponseAsync(response);
        }

        /// <summary>
        ///     Sends POST request without body and without response.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The cancellation token.</param>
        /// <typeparam name="TError">The error response type.</typeparam>
        /// <returns>The new instance of <see cref="OperationResult{TError}" />.</returns>
        public async Task<OperationResult<TError>> PostJsonAsync<TError>(Action<Url> url,
            IDictionary<string, object>? headers = null,
            CancellationToken token = default) where TError : class
        {
            var request = await client.PrepareRequest(url, headers);

            var response = await client.ExecuteWithPolicyAsync(request, HttpMethod.Post, token: token);

            return await OperationResult<TError>.CreateFromResponseAsync(response);
        }

        /// <summary>
        ///     Sends POST request with specified stream body and without response.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="body">The body stream.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The cancellation token.</param>
        /// <typeparam name="TError">The error response type.</typeparam>
        /// <returns>The new instance of <see cref="OperationResult{TError}" />.</returns>
        public async Task<OperationResult<TError>> PostStreamAsync<TError>(Action<Url> url,
            Stream body,
            IDictionary<string, object>? headers = null,
            CancellationToken token = default) where TError : class
        {
            var request = await client.PrepareRequest(url, headers);

            request.Content = new StreamContent(body);

            var response = await client.ExecuteWithPolicyAsync(request, HttpMethod.Post, token: token);

            return await OperationResult<TError>.CreateFromResponseAsync(response);
        }

        /// <summary>
        ///     Sends POST request with specified stream body and with awaiting <typeparamref name="TResponse" /> deserialized
        ///     response.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="body">The body stream.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The cancellation token.</param>
        /// <typeparam name="TResponse">The success response type.</typeparam>
        /// <typeparam name="TError">The error response type.</typeparam>
        /// <returns>The new instance of <see cref="OperationResult{TResponse,TError}" />.</returns>
        public async Task<OperationResult<TResponse, TError>> PostStreamAsync<TResponse, TError>(Action<Url> url,
            Stream body,
            IDictionary<string, object>? headers = null,
            CancellationToken token = default) where TError : class where TResponse : class
        {
            var request = await client.PrepareRequest(url, headers);

            request.Content = new StreamContent(body);

            var response = await client.ExecuteWithPolicyAsync(request, HttpMethod.Post, token: token);

            return await OperationResult<TResponse, TError>.CreateFromResponseAsync(response);
        }
    }
}