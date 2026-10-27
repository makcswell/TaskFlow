using Ebceys.Infrastructure.AuthorizationTestApplication;
using Ebceys.Infrastructure.AuthorizationTestApplication.Client;
using Ebceys.Infrastructure.HttpClient;
using Ebceys.Tests.Infrastructure.IntegrationTests.WebApplication;
using Serilog.Extensions.Logging;

namespace Ebceys.Infrastructure.Tests.AppInitializer;

public class AuthAppWebApplicationFactory(Action<IServiceCollection> beforeHostingStarted)
    : TestWebApplicationFactory<AuthorizationStartup>
{
    public override string ServiceId => "auth-app";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(beforeHostingStarted);
        base.ConfigureWebHost(builder);
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
    }
}

public class AuthAppTestClientContext(Action<IServiceCollection> beforeHostingStarted)
    : ClientTestContext<IAuthAppClient, AuthorizationStartup>
{
    protected override TestWebApplicationFactory<AuthorizationStartup> CreateWebApplicationFactory()
    {
        return new AuthAppWebApplicationFactory(beforeHostingStarted);
    }

    protected override IAuthAppClient CreateServiceClient(string baseAddress)
    {
        var cache = CreateFlurlCache();
        return new AuthAppClient(cache, new SerilogLoggerFactory(), ClientBaseUrlResolver.Create(baseAddress));
    }
}