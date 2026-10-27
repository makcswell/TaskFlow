using Ebceys.Infrastructure.HttpClient;
using Ebceys.Infrastructure.TestApplication.Client.Interfaces;
using Flurl.Http.Configuration;

namespace Ebceys.Infrastructure.TestApplication.Client.Implementations;

public class TestApplicationClient(
    IFlurlClientCache clientCache,
    ILoggerFactory loggerFactory,
    ClientBaseUrlResolver baseUrlResolver,
    ClientBaseTokenResolver? tokenResolver = null)
    : ClientBase(clientCache, loggerFactory, baseUrlResolver, tokenResolver), ITestApplicationClient
{
    public ITestClient TestClient { get; } = new TestClient(clientCache, loggerFactory, baseUrlResolver, tokenResolver);
}