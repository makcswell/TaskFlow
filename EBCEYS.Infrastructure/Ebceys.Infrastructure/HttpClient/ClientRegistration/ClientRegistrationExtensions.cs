using Ebceys.Infrastructure.HttpClient.TokenManager;
using Ebceys.Infrastructure.Validation;
using FluentValidation;
using Flurl.Http.Configuration;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ebceys.Infrastructure.HttpClient.ClientRegistration;

/// <summary>
///     Extension methods for fluent registration of <see cref="ClientBase" /> implementations in the DI container.
/// </summary>
[PublicAPI]
public static class ClientRegistrationExtensions
{
    /// <summary>
    ///     Starts the registration process of <typeparamref name="TImplementation" /> in <paramref name="services" />.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <typeparam name="TInterface">The interface.</typeparam>
    /// <typeparam name="TImplementation">The implementation of <see cref="ClientBase" />.</typeparam>
    /// <returns>
    ///     The new instance of <see cref="ClientBaseRegistrationRegistrator{TInterface,TImplementation}" /> to configure
    ///     client.
    /// </returns>
    public static ClientBaseRegistrationRegistrator<TInterface, TImplementation> AddClient<TInterface, TImplementation>(
        this IServiceCollection services)
        where TImplementation : ClientBase, TInterface where TInterface : class
    {
        return new ClientBaseRegistrationRegistrator<TInterface, TImplementation>(services);
    }
}

/// <summary>
///     Fluent builder for configuring and registering a <see cref="ClientBase" />-derived HTTP client
///     in the service collection. Supports URL configuration, token resolvers, Flurl client cache customization,
///     and HTTP context-based auth token forwarding.
/// </summary>
/// <param name="services">The service collection to register the client in.</param>
/// <typeparam name="TInterface">The service interface that the client implements.</typeparam>
/// <typeparam name="TImplementation">The concrete <see cref="ClientBase" /> implementation type.</typeparam>
[PublicAPI]
public class ClientBaseRegistrationRegistrator<TInterface, TImplementation>(IServiceCollection services)
    where TImplementation : ClientBase, TInterface where TInterface : class
{
    private ClientBaseUrlResolver? _baseUrlResolver;
    private Func<IServiceProvider, IEnumerable<Func<DelegatingHandler>>, IFlurlClientCache>? _flurlClientCacheResolver;
    private ClientBaseTokenResolver? _tokenResolver;
    private bool _useClientTokenManager;

    /// <summary>
    ///     Configures the url for client from string.
    /// </summary>
    /// <param name="url">The url to server.</param>
    /// <returns></returns>
    public ClientBaseRegistrationRegistrator<TInterface, TImplementation> FromUrl(string url)
    {
        _baseUrlResolver = ClientBaseUrlResolver.Create(url);
        return this;
    }

    /// <summary>
    ///     Configures the server url for client from <see cref="IConfiguration" />.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="serviceName">The service name.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throw if there's no section with <paramref name="serviceName" />.</exception>
    /// <exception cref="ValidationException">If client configuration is not valid.</exception>
    public ClientBaseRegistrationRegistrator<TInterface, TImplementation> FromConfiguration(
        IConfiguration configuration,
        string serviceName)
    {
        var clientConfiguration = configuration.GetSection(serviceName).Get<ClientConfiguration>()
                                  ?? throw new InvalidOperationException(
                                      $"Configuration section '{serviceName}' not found");
        var validator = new ClientConfigurationValidator();
        validator.ValidateAndThrow(clientConfiguration);

        _baseUrlResolver = ClientBaseUrlResolver.Create(clientConfiguration.ServiceUrl);
        return this;
    }

    /// <summary>
    ///     Adds the auth token resolver to <see cref="ClientBaseRegistrationRegistrator{TInterface,TImplementation}" />.<br />
    ///     Token will be placed in request headers with key <see cref="ClientBase.AuthorizationHeader" />.
    /// </summary>
    /// <param name="tokenResolver"></param>
    /// <returns></returns>
    public ClientBaseRegistrationRegistrator<TInterface, TImplementation> AddAuthTokenResolver(
        Func<Task<string?>> tokenResolver)
    {
        _tokenResolver = new ClientBaseTokenResolver(tokenResolver);
        return this;
    }

    /// <summary>
    ///     Adds the <see cref="IClientTokenManager{TInterface}" /> with <typeparamref name="TInterface" /> type to service
    ///     collection. <br />
    ///     It uses for access to auth token from http context. See default implementation (could be overwritten).
    ///     <seealso cref="FromContextClientTokenManager{TInterface}" />
    /// </summary>
    /// <returns></returns>
    public ClientBaseRegistrationRegistrator<TInterface, TImplementation> AddAuthTokenFromHttpContextResolver()
    {
        return AddCustomAuthTokenResolver<FromContextClientTokenManager<TInterface>>();
    }

    /// <summary>
    ///     Adds the custom auth token resolver with <see cref="TInterface" /> mapping.
    /// </summary>
    /// <param name="factory">The factory.</param>
    /// <typeparam name="TResolver">The <see cref="IClientTokenManager{TInterface}" /> implementation.</typeparam>
    /// <exception cref="InvalidOperationException">If token manager already registered for service.</exception>
    /// <returns></returns>
    public ClientBaseRegistrationRegistrator<TInterface, TImplementation> AddCustomAuthTokenResolver<TResolver>(
        Func<IServiceProvider, IClientTokenManager<TInterface>>? factory = null)
        where TResolver : class, IClientTokenManager<TInterface>
    {
        if (_useClientTokenManager)
        {
            throw new InvalidOperationException(
                $"The {nameof(IClientTokenManager<>)} is already registered for {nameof(TImplementation)}!");
        }

        if (factory is not null)
        {
            services.TryAddSingleton(factory);
        }
        else
        {
            services.TryAddSingleton<IClientTokenManager<TInterface>, TResolver>();
        }

        _useClientTokenManager = true;
        return this;
    }

    /// <summary>
    ///     Configures <see cref="IFlurlClientCache" />. If it wasn't configured the default implementation will be used.
    /// </summary>
    /// <param name="clientCacheResolver">The client cache resolver.</param>
    /// <returns></returns>
    public ClientBaseRegistrationRegistrator<TInterface, TImplementation> ConfigureClientCache(
        Func<IServiceProvider, IEnumerable<Func<DelegatingHandler>>, IFlurlClientCache> clientCacheResolver)
    {
        _flurlClientCacheResolver = clientCacheResolver;
        return this;
    }

    /// <summary>
    ///     Registration of the <typeparamref name="TImplementation" /> in <see cref="IServiceCollection" />.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Register()
    {
        if (_baseUrlResolver is null)
        {
            throw new InvalidOperationException("No service url resolver has been set");
        }

        if (_flurlClientCacheResolver is not null)
        {
            services.TryAddSingleton<IFlurlClientCache>(sp =>
                _flurlClientCacheResolver(sp, sp.GetServices<Func<DelegatingHandler>>()));
        }
        else
        {
            services.TryAddSingleton<IFlurlClientCache>(sp =>
            {
                var middlewares = sp.GetServices<Func<DelegatingHandler>>();
                var cache = new FlurlClientCache().WithDefaults(conf =>
                {
                    foreach (var middleware in middlewares)
                    {
                        conf.AddMiddleware(middleware);
                    }
                });
                return cache;
            });
        }

        var urlResolver = _baseUrlResolver!;

        services.AddSingleton<TInterface, TImplementation>(sp =>
        {
            if (_tokenResolver is not null)
            {
                return ActivatorUtilities.CreateInstance<TImplementation>(sp, urlResolver, _tokenResolver);
            }

            if (_useClientTokenManager)
            {
                return ActivatorUtilities.CreateInstance<TImplementation>(sp, urlResolver,
                    GetTokenResolverWithTokenManager(sp));
            }

            return ActivatorUtilities.CreateInstance<TImplementation>(sp, urlResolver);
        });
    }

    private static ClientBaseTokenResolver GetTokenResolverWithTokenManager(IServiceProvider sp)
    {
        return new ClientBaseTokenResolver(async () =>
        {
            var manager = sp.GetRequiredService<IClientTokenManager<TInterface>>();
            var token = await manager.GetTokenAsync() ?? string.Empty;
            return token;
        });
    }
}

/// <summary>
///     Configuration record for <see cref="ClientBase" /> service URL, typically bound from
///     <see cref="IConfiguration" /> sections.
/// </summary>
[PublicAPI]
public record ClientConfiguration
{
    /// <summary>
    ///     The service url.
    /// </summary>
    public required string ServiceUrl { get; init; }
}

internal class ClientConfigurationValidator : AbstractValidator<ClientConfiguration>
{
    public ClientConfigurationValidator()
    {
        RuleFor(it => it.ServiceUrl).IsValidAbsoluteUrl();
    }
}