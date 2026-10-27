using Ebceys.Infrastructure.HttpClient;
using Ebceys.Infrastructure.TestApplication;
using Ebceys.Infrastructure.TestApplication.Client.Implementations;
using Ebceys.Tests.Infrastructure.IntegrationTests.WebApplication;
using Serilog.Extensions.Logging;

namespace Ebceys.Infrastructure.Tests.AppInitializer;

public class AppWebApplicationFactory(Action<IServiceCollection> beforeHostingStarted)
    : TestWebApplicationFactory<Startup>
{
    public override string ServiceId { get; } = "app";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(beforeHostingStarted);
        base.ConfigureWebHost(builder);
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
    }
}

public class AppTestClientContext(Action<IServiceCollection> beforeHostingStarted)
    : ClientTestContext<ITestApplicationClient, Startup>
{
    protected override TestWebApplicationFactory<Startup> CreateWebApplicationFactory()
    {
        return new AppWebApplicationFactory(beforeHostingStarted);
    }

    protected override ITestApplicationClient CreateServiceClient(string baseAddress)
    {
        var clientsCache = CreateFlurlCache();

        return new TestApplicationClient(clientsCache, new SerilogLoggerFactory(),
            ClientBaseUrlResolver.Create(baseAddress));
    }
}