using Ebceys.Infrastructure.Services.ExecutedServices;
using JetBrains.Annotations;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

#pragma warning disable ASPDEPR008

namespace Ebceys.Tests.Infrastructure.IntegrationTests.WebApplication;

/// <summary>
///     Abstract test web application factory that creates an in-memory test server for integration testing.
///     Extends <see cref="WebApplicationFactory{TStartup}" /> with support for Serilog logging,
///     optional production appsettings loading, and automatic execution of
///     <see cref="IBeforeHostingStartedService" /> implementations during host creation.
/// </summary>
/// <typeparam name="TStartup">The application startup class.</typeparam>
[PublicAPI]
public abstract class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    /// <summary>
    ///     The base address.
    /// </summary>
    public Uri BaseAddress
    {
        get => ClientOptions.BaseAddress;
        set => ClientOptions.BaseAddress = value;
    }

    /// <summary>
    ///     The service id.
    /// </summary>
    public abstract string ServiceId { get; }

    /// <summary>
    ///     Indicates that <see cref="TestWebApplicationFactory{TStartup}" /> should use production appsettings.json file.
    /// </summary>
    public bool UseProductionAppSettings { get; set; }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((cxt, cfg) =>
        {
            if (UseProductionAppSettings)
            {
                cfg.AddJsonFile("appsettings.json", true, true);
                SetConfiguration(cfg);
            }
        });
        builder.ConfigureTestServices(ConfigureTestServices).ConfigureLogging(logging =>
        {
            logging.Services.AddSerilog((services, config) =>
            {
                var conf = services.GetRequiredService<IConfiguration>();
                config.ReadFrom.Configuration(conf);
            });
        });
    }

    /// <inheritdoc />
    protected override IWebHostBuilder CreateWebHostBuilder()
    {
        return CreateWebHostBuilder();
    }

    /// <inheritdoc />
    protected override TestServer CreateServer(IWebHostBuilder builder)
    {
        return base.CreateServer(new WebHostBuilderDecorator(builder));
    }

    /// <inheritdoc />
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var hostBuilder = new HostBuilderDecorator(builder);
        return hostBuilder.Build();
    }

    /// <summary>
    ///     Sets the configuration.
    /// </summary>
    /// <param name="configurationBuilder">The configuration builder.</param>
    protected virtual void SetConfiguration(IConfigurationBuilder configurationBuilder)
    {
    }

    /// <summary>
    ///     Configures the test services.
    /// </summary>
    /// <param name="services">The services.</param>
    protected abstract void ConfigureTestServices(IServiceCollection services);

    private IWebHostBuilder CreateWebHostBuilder(params string[] args)
    {
        return WebHost.CreateDefaultBuilder(args).ConfigureAppConfiguration((ctx, builder) =>
        {
            builder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ServiceId", ServiceId }
            });
        }).UseStartup<TStartup>().UseShutdownTimeout(TimeSpan.FromSeconds(5));
    }

    private class HostBuilderDecorator(IHostBuilder inner) : IHostBuilder
    {
        public IHost Build()
        {
            var host = inner.Build();
            host.Services.ExecuteAllBeforeHostingStarted().GetAwaiter().GetResult();
            return host;
        }

        public IHostBuilder ConfigureAppConfiguration(
            Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            return inner.ConfigureAppConfiguration(configureDelegate);
        }

        public IHostBuilder ConfigureContainer<TContainerBuilder>(
            Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            return inner.ConfigureContainer(configureDelegate);
        }

        public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            return inner.ConfigureHostConfiguration(configureDelegate);
        }

        public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            return inner.ConfigureServices(configureDelegate);
        }

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
            IServiceProviderFactory<TContainerBuilder> factory) where TContainerBuilder : notnull
        {
            return inner.UseServiceProviderFactory(factory);
        }

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
            Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
            where TContainerBuilder : notnull
        {
            return inner.UseServiceProviderFactory(factory);
        }

        public IDictionary<object, object> Properties { get; }
    }

    private class WebHostBuilderDecorator(IWebHostBuilder inner) : IWebHostBuilder
    {
        public IWebHost Build()
        {
            var host = inner.Build();
            host.Services.ExecuteAllBeforeHostingStarted().GetAwaiter().GetResult();
            return host;
        }

        public IWebHostBuilder ConfigureAppConfiguration(
            Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            return inner.ConfigureAppConfiguration(configureDelegate);
        }

        public IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            return inner.ConfigureServices(configureServices);
        }

        public IWebHostBuilder ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
        {
            return inner.ConfigureServices(configureServices);
        }

        public string? GetSetting(string key)
        {
            return inner.GetSetting(key);
        }

        public IWebHostBuilder UseSetting(string key, string? value)
        {
            return inner.UseSetting(key, value);
        }
    }
}