using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Ebceys.Infrastructure.Services.ExecutedServices;

/// <summary>
///     Interface for services that need to execute logic after DI registration is complete
///     but before the web host starts accepting requests (e.g., database migrations, cache warming).
/// </summary>
[PublicAPI]
public interface IBeforeHostingStartedService
{
    /// <summary>
    ///     Executes the startup action before the host starts accepting requests.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous startup operation.</returns>
    Task ExecuteAsync(CancellationToken cancellationToken);
}

/// <summary>
///     Extension methods for registering and executing <see cref="IBeforeHostingStartedService" /> implementations.
/// </summary>
[PublicAPI]
public static class BeforeHostingStartedServiceExtensions
{
    /// <summary>
    ///     Register implementation of <see cref="IBeforeHostingStartedService" /> to <paramref name="services" />.<br />
    ///     All <see cref="IBeforeHostingStartedService" /> will be executed after all services registration.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <typeparam name="TImplementation">The implementation of <see cref="IBeforeHostingStartedService" />.</typeparam>
    /// <returns>The <paramref name="services" />.</returns>
    public static IServiceCollection AddBeforeHostingStarted<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IBeforeHostingStartedService
    {
        services.AddSingleton<IBeforeHostingStartedService, TImplementation>();
        return services;
    }

    /// <summary>
    ///     Executes the all registered implementations of <see cref="IBeforeHostingStartedService" />.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="token">The cancellation token.</param>
    public static async Task ExecuteAllBeforeHostingStarted(this IServiceProvider services,
        CancellationToken token = default)
    {
        var beforeHostingStartedServices = services.GetServices<IBeforeHostingStartedService>();
        foreach (var beforeHostingStartedService in beforeHostingStartedServices)
        {
            await beforeHostingStartedService.ExecuteAsync(token);
        }
    }
}