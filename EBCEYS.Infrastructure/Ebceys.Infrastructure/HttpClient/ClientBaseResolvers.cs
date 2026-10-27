using JetBrains.Annotations;

namespace Ebceys.Infrastructure.HttpClient;

/// <summary>
///     Resolver that provides the base URL for <see cref="ClientBase" /> instances.
///     Supports implicit conversion from <see cref="Func{String}" /> for convenient inline usage.
/// </summary>
/// <param name="resolver">The function that returns the base URL string.</param>
[PublicAPI]
public sealed class ClientBaseUrlResolver(Func<string> resolver) : IClientBaseResolver<string>
{
    /// <inheritdoc />
    public string Invoke()
    {
        return Invoker.Invoke();
    }

    /// <inheritdoc />
    public Func<string> Invoker { get; } = resolver;

    /// <summary>
    ///     Casts the <paramref name="resolver" /> to <see cref="ClientBaseUrlResolver" />.
    /// </summary>
    /// <param name="resolver">The resolver.</param>
    /// <returns>The new instance of <see cref="ClientBaseUrlResolver" />.</returns>
    public static implicit operator ClientBaseUrlResolver(Func<string> resolver)
    {
        return new ClientBaseUrlResolver(resolver);
    }

    /// <summary>
    ///     Creates the new instance of <see cref="ClientBaseUrlResolver" />.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <returns>The new instance of <see cref="ClientBaseUrlResolver" />.</returns>
    public static ClientBaseUrlResolver Create(string url)
    {
        return new ClientBaseUrlResolver(() => url);
    }
}

/// <summary>
///     Resolver that provides the authorization token for <see cref="ClientBase" /> instances.
///     The resolved token is automatically added to the <c>Authorization</c> request header.
///     Supports implicit conversion from <see cref="Func{Task}" /> for convenient inline usage.
/// </summary>
/// <param name="resolver">The asynchronous function that returns the auth token, or <c>null</c> if no token is available.</param>
[PublicAPI]
public sealed class ClientBaseTokenResolver(Func<Task<string?>>? resolver) : IClientBaseResolver<Task<string?>>
{
    /// <inheritdoc />
    public Task<string?> Invoke()
    {
        return Invoker?.Invoke() ?? Task.FromResult<string?>(null);
    }

    /// <inheritdoc />
    public Func<Task<string?>>? Invoker { get; } = resolver;

    /// <summary>
    ///     Casts the <paramref name="resolver" /> to <see cref="ClientBaseTokenResolver" />.
    /// </summary>
    /// <param name="resolver">The resolver.</param>
    /// <returns>The new instance of <see cref="ClientBaseTokenResolver" />.</returns>
    public static implicit operator ClientBaseTokenResolver(Func<Task<string?>>? resolver)
    {
        return new ClientBaseTokenResolver(resolver);
    }

    /// <summary>
    ///     Creates the new instance of <see cref="ClientBaseTokenResolver" />.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>The new instance of <see cref="ClientBaseTokenResolver" />.</returns>
    public static ClientBaseTokenResolver Create(string? token)
    {
        return new ClientBaseTokenResolver(() => Task.FromResult(token));
    }
}

/// <summary>
///     Generic interface for value resolvers used by <see cref="ClientBase" />.
/// </summary>
/// <typeparam name="TResolve">The type of the resolved value.</typeparam>
[PublicAPI]
public interface IClientBaseResolver<out TResolve>
{
    /// <summary>
    ///     The underlying delegate function that produces the resolved value.
    /// </summary>
    public Func<TResolve>? Invoker { get; }

    /// <summary>
    ///     Invokes the resolver and returns the resolved value.
    /// </summary>
    /// <returns>The resolved value of type <typeparamref name="TResolve" />.</returns>
    public TResolve Invoke();
}