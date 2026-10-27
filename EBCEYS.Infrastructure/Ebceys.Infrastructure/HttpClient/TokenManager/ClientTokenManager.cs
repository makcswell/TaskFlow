using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Ebceys.Infrastructure.HttpClient.TokenManager;

/// <summary>
///     Interface for managing authorization tokens for a specific HTTP client.
///     Implementations provide tokens that are automatically attached to outgoing requests.
/// </summary>
/// <typeparam name="TInterface">The client interface type, used to distinguish token managers for different clients.</typeparam>
[PublicAPI]
public interface IClientTokenManager<TInterface>
{
    /// <summary>
    ///     Gets the authorization token to be used in outgoing HTTP requests.
    /// </summary>
    /// <returns>The auth token string, or <c>null</c> if no token is available.</returns>
    Task<string?> GetTokenAsync();
}

/// <summary>
///     Default <see cref="IClientTokenManager{TInterface}" /> implementation that extracts the authorization token
///     from the current HTTP context's <c>Authorization</c> header, effectively forwarding the caller's token
///     to downstream service calls.
/// </summary>
/// <param name="contextAccessor">The HTTP context accessor to read the incoming request headers.</param>
/// <typeparam name="TInterface">The client interface type for token manager resolution.</typeparam>
[PublicAPI]
public class FromContextClientTokenManager<TInterface>(IHttpContextAccessor contextAccessor)
    : IClientTokenManager<TInterface>
{
    /// <inheritdoc />
    public Task<string?> GetTokenAsync()
    {
        if (contextAccessor.HttpContext?.Request.Headers.TryGetValue("Authorization", out var token) ??
            false)
        {
            return Task.FromResult((string?)token);
        }

        return Task.FromResult<string?>(null);
    }
}