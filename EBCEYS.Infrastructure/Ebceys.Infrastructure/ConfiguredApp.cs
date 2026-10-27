using Ebceys.Infrastructure.Services.ExecutedServices;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Ebceys.Infrastructure;

/// <summary>
///     Represents a configured web application that wraps <see cref="WebApplicationBuilder" /> and provides
///     methods to build and run it. Implements <see cref="IAsyncDisposable" />
///     for proper lifecycle management.
/// </summary>
/// <param name="builder">The web application builder.</param>
[PublicAPI]
public sealed class ConfiguredApp(IHostBuilder builder) : IDisposable, IAsyncDisposable
{
    private IHost? _app;

    /// <summary>
    ///     The application service provider.
    ///     Available only after <see cref="RunAsync" /> has been called.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if accessed before <see cref="RunAsync" />.</exception>
    public IServiceProvider ServiceProvider =>
        _app?.Services ?? throw new InvalidOperationException(
            $"Application has not been built yet. Call {nameof(RunAsync)} first.");

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            _app.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Runs the application.
    /// </summary>
    /// <param name="configureConf">An optional action to further configure the application configuration.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The <see cref="Task" /> that represents the app running instance.</returns>
    public async Task RunAsync(Action<IConfigurationBuilder>? configureConf = null,
        CancellationToken token = default)
    {
        if (configureConf is not null)
        {
            builder.ConfigureHostConfiguration(configureConf);
        }

        _app = builder.Build();

        await _app.Services.ExecuteAllBeforeHostingStarted(token);

        await _app.RunAsync(token);
    }
}