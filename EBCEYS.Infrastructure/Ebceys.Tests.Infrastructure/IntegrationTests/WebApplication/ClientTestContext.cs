using Ebceys.Infrastructure.Extensions;
using Ebceys.Infrastructure.HttpClient;
using Ebceys.Infrastructure.HttpClient.ServiceClient;
using Ebceys.Infrastructure.Models;
using Ebceys.Infrastructure.Services.ExecutedServices;
using Ebceys.Tests.Infrastructure.Helpers;
using Flurl.Http;
using Flurl.Http.Configuration;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Extensions.Logging;

namespace Ebceys.Tests.Infrastructure.IntegrationTests.WebApplication;

/// <summary>
///     Abstract test context for integration testing of <see cref="ClientBase" />-derived HTTP clients
///     against an in-memory test server. Provides client creation, service system client access,
///     and Flurl client cache integration with request routing to the test server.
/// </summary>
/// <typeparam name="TClient">The HTTP client type under test.</typeparam>
/// <typeparam name="TEntrypoint">The application startup/entry point class.</typeparam>
[PublicAPI]
public abstract class ClientTestContext<TClient, TEntrypoint> : ServiceTestContext<TEntrypoint>
    where TEntrypoint : class where TClient : class
{
    /// <summary>
    ///     The service client.
    /// </summary>
    public TClient ServiceClient { get; private set; } = null!;

    /// <summary>
    ///     The services.
    /// </summary>
    public IServiceProvider Services => Factory.Services;


    /// <inheritdoc />
    public override void Initialize(Action<TestClientInitializeOptions>? configurator = null)
    {
        base.Initialize(configurator);
        ServiceClient = CreateServiceClient(BaseAddress);
    }

    /// <summary>
    ///     Creates the service client.
    /// </summary>
    /// <param name="baseAddress">The base address.</param>
    /// <returns>The new instance of <see cref="TClient" />.</returns>
    protected abstract TClient CreateServiceClient(string baseAddress);

    /// <summary>
    ///     Creates the instance of <see cref="IServiceSystemClient" />.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="tokenResolver">The token resolver.</param>
    /// <returns>The new instance of <see cref="IServiceSystemClient" />.</returns>
    public IServiceSystemClient CreateServiceSystemClient(ILoggerFactory? loggerFactory = null,
        ClientBaseTokenResolver? tokenResolver = null)
    {
        var apiInfo = Factory.Services.GetRequiredService<IOptions<ServiceApiInfo>>();
        var cache = CreateFlurlCache();
        return new ServiceSystemClient(apiInfo, cache, loggerFactory ?? new SerilogLoggerFactory(),
            ClientBaseUrlResolver.Create(BaseAddress), tokenResolver);
    }

    /// <summary>
    ///     Creates the <see cref="IFlurlClientCache" /> with default middlewares.
    /// </summary>
    /// <returns>The new instance of <see cref="IFlurlClientCache" />.</returns>
    protected IFlurlClientCache CreateFlurlCache()
    {
        var middlewares = Factory.Services.GetServices<Func<DelegatingHandler>>();
        var clientsCache = new FlurlClientCache();
        clientsCache.WithDefaults(x =>
        {
            foreach (var middleware in middlewares)
            {
                x.AddMiddleware(middleware);
            }
        });
        return clientsCache;
    }
}

/// <summary>
///     Abstract test context that manages the lifecycle of a <see cref="WebApplicationFactory{TEntrypoint}" />
///     for integration testing. Provides initialization, teardown, and request routing capabilities.
/// </summary>
/// <typeparam name="TEntrypoint">The application startup/entry point class.</typeparam>
[PublicAPI]
public abstract class ServiceTestContext<TEntrypoint> where TEntrypoint : class
{
    private bool _isInitialized;

    /// <summary>
    ///     The factory.
    /// </summary>
    public WebApplicationFactory<TEntrypoint> Factory { get; private set; }

    /// <summary>
    ///     The base address.
    /// </summary>
    public string BaseAddress { get; private set; }

    /// <summary>
    ///     Initializes the context.
    /// </summary>
    /// <param name="configurator"></param>
    public virtual void Initialize(Action<TestClientInitializeOptions>? configurator = null)
    {
        if (_isInitialized)
        {
            return;
        }

        var opts = new TestClientInitializeOptions();
        if (configurator != null)
        {
            configurator(opts);
        }

        var appFactory = CreateWebApplicationFactory();
        appFactory.UseProductionAppSettings = opts.UseProductionAppSettings;
        if (opts.BaseAddress != null)
        {
            appFactory.BaseAddress = opts.BaseAddress;
        }

        BaseAddress = appFactory.BaseAddress.ToString();
        Factory = appFactory.WithWebHostBuilder(x =>
        {
            if (!opts.SolutionRelativePath.IsNullOrWhiteSpace())
            {
                x.UseSolutionRelativeContentRoot(opts.SolutionRelativePath!);
            }

            x.ConfigureServices(services =>
            {
                services.AddSingleton<Func<DelegatingHandler>>(c => () =>
                    new RoutingMessageHandler(c
                        .GetRequiredService<
                            IOptions<RoutingMessageHandlerConfiguration>>()));
                services.Configure<RoutingMessageHandlerConfiguration>(c =>
                    c.AddRoute(BaseAddress, Factory.Server.CreateHandler));
            });
            var builderConf = opts.BuilderConfiguration;
            if (builderConf != null)
            {
                builderConf(x);
            }
        });
        Factory.CreateClient();
        Factory.Services.ExecuteAllBeforeHostingStarted().GetAwaiter().GetResult();
        _isInitialized = true;
    }

    /// <summary>
    ///     Teardowns the context.
    /// </summary>
    public virtual void Teardown()
    {
        if (!_isInitialized)
        {
            return;
        }

        Factory.Dispose();
        _isInitialized = false;
        FlurlHttp.Clients.Clear();
    }

    /// <summary>
    ///     Creates the web host application factory.
    /// </summary>
    /// <returns></returns>
    protected abstract TestWebApplicationFactory<TEntrypoint> CreateWebApplicationFactory();
}

/// <summary>
///     Options for configuring a test context during initialization, including base address,
///     content root path, builder configuration, and app settings mode.
/// </summary>
[PublicAPI]
public class TestClientInitializeOptions
{
    /// <summary>
    ///     Initiates the new instance of <see cref="TestClientInitializeOptions" />.
    /// </summary>
    public TestClientInitializeOptions()
    {
        var address = $"http://localhost:{PortSelector.GetPort()}/";
        BaseAddress = new Uri(address);
    }

    /// <summary>
    ///     The solution relative path.
    /// </summary>
    public string? SolutionRelativePath { get; set; }

    /// <summary>
    ///     The builder configuration.
    /// </summary>
    public Action<IWebHostBuilder>? BuilderConfiguration { get; set; }

    /// <summary>
    ///     The base address.
    /// </summary>
    public Uri? BaseAddress { get; set; }

    /// <summary>
    ///     The use production app settings.
    /// </summary>
    public bool UseProductionAppSettings { get; set; }
}

/// <inheritdoc />
[PublicAPI]
public class RoutingMessageHandler : DelegatingHandler
{
    private readonly Dictionary<string, Lazy<HostHandler>> _hosts = new();

    /// <inheritdoc />
    public RoutingMessageHandler(IOptions<RoutingMessageHandlerConfiguration> opts)
    {
        foreach (var route in opts.Value.Routes)
        {
            AddHandler(route.Key, route.Value);
        }
    }

    private void AddHandler(Uri origin, Func<HttpMessageHandler> handler)
    {
        var address = $"{origin.Scheme}://{origin.Host}:{origin.Port}/";
        _hosts.Add(address, new Lazy<HostHandler>(() => new HostHandler(handler())));
    }

    private void AddHandler(string origin, Func<HttpMessageHandler> handler)
    {
        AddHandler(new Uri(origin), handler);
    }

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var host = $"{request.RequestUri!.Scheme}://{request.RequestUri.Host}:{request.RequestUri.Port}/";

        if (_hosts.TryGetValue(host, out var lazyHandler))
        {
            var handler = lazyHandler.Value;
            if (!ReplaceHttpHandlerByTest(InnerHandler, handler))
            {
                return handler.SendRequest(request, cancellationToken);
            }
        }

        return base.SendAsync(request, cancellationToken);
    }

    private static bool ReplaceHttpHandlerByTest(
        HttpMessageHandler? innerHandler,
        HostHandler handler)
    {
        if (innerHandler is not DelegatingHandler delegatingHandler)
        {
            return false;
        }

        var msgHandler = delegatingHandler.InnerHandler;
        while (msgHandler is DelegatingHandler msgDelegatingHandler)
        {
            msgHandler = msgDelegatingHandler.InnerHandler;
        }

        if (delegatingHandler != handler && delegatingHandler.InnerHandler != handler)
        {
            delegatingHandler.InnerHandler = handler;
        }

        return true;
    }


    private class HostHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
    {
        public Task<HttpResponseMessage> SendRequest(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return SendAsync(request, cancellationToken);
        }
    }
}

/// <summary>
///     Configuration for routing HTTP requests to in-memory test server handlers based on URL.
///     Used by <see cref="RoutingMessageHandler" /> and <see cref="TestRoutingMessageHandler" />
///     to redirect HTTP client calls to test servers during integration tests.
/// </summary>
[PublicAPI]
public class RoutingMessageHandlerConfiguration
{
    private readonly Dictionary<string, Func<HttpMessageHandler>> _routes = new();

    /// <summary>
    ///     The routes.
    /// </summary>
    public IReadOnlyDictionary<string, Func<HttpMessageHandler>> Routes => _routes;

    /// <summary>
    ///     Clears the routes.
    /// </summary>
    public void ClearRoutes()
    {
        _routes.Clear();
    }

    /// <summary>
    ///     Adds the route.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="handler"></param>
    public void AddRoute(string url, Func<HttpMessageHandler> handler)
    {
        _routes.Add(url, handler);
    }

    /// <summary>
    ///     Adds the route for <paramref name="testContext" />.
    /// </summary>
    /// <param name="testContext">The test context.</param>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TEntrypoint"></typeparam>
    public void AddRoute<TClient, TEntrypoint>(ClientTestContext<TClient, TEntrypoint> testContext)
        where TEntrypoint : class where TClient : class
    {
        AddRoute(testContext.BaseAddress, () => testContext.Factory.Server.CreateHandler());
    }
}