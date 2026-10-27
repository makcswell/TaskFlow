using AwesomeAssertions;
using Ebceys.Infrastructure.Middlewares;
using Ebceys.Infrastructure.TestApplication.BoundedContext.Requests;
using Ebceys.Infrastructure.TestApplication.Client.Implementations;
using Ebceys.Tests.Infrastructure.Helpers;

namespace Ebceys.Infrastructure.Tests.Middlewares;

public class RequestLoggingMiddlewareTests
{
    private static readonly EbRandomizer Randomizer = EbRandomizer.Create();
    private ITestApplicationClient _client;

    [SetUp]
    public void Setup()
    {
        AppTestContext.AppLogCatcher.Logs.Clear();
        _client = AppTestContext.AppContext.ServiceClient;
    }

    [Test]
    public async Task When_Requests_With_NoLoggingInOpts_Result_CatcherDontLogBodies()
    {
        var someBody = Randomizer.String(10);
        await _client.TestClient.GenerateTokenAsync(CancellationToken.None);
        await _client.TestClient.PostBodyAsync(new SomeBodyRequest(someBody), CancellationToken.None);
        await _client.TestClient.GetQueryAsync(11, CancellationToken.None);

        AppTestContext.AppLogCatcher.Logs.Should().AllSatisfy(x =>
        {
            x.Message.Should().NotContain("{").And.NotContain("}")
                .And.NotContain(someBody)
                .And.Contain(RequestLoggingMiddleware.NoBodyLoggingString);
        });
    }

    [Test]
    public async Task When_Request_With_NoAttrOrOpts_Result_AllLogged()
    {
        var someName = Randomizer.String(10);
        await _client.TestClient.PostCommandAsync(someName, CancellationToken.None);

        AppTestContext.AppLogCatcher.Logs.Should().AllSatisfy(x => x.Message.Contains(someName).Should().BeTrue());
    }

    [Test]
    public async Task When_Requests_With_NoLoggingInAttr_Result_CatcherDontLogBodies()
    {
        var someBody = Randomizer.String(10);
        await _client.TestClient.GetCommandAsync(CancellationToken.None);

        AppTestContext.AppLogCatcher.Logs
            .Where(x => x.Message.StartsWith("<== RESPONSE")).Should().AllSatisfy(x =>
            {
                x.Message.Should().NotContain("{").And.NotContain("}")
                    .And.NotContain(someBody)
                    .And.Contain(RequestLoggingMiddleware.NoBodyLoggingString);
            });

        var someName = Randomizer.String(10);
        await _client.TestClient.PostCommandAsync(someName, CancellationToken.None);
        AppTestContext.AppLogCatcher.Logs.Clear();
        var newName = Randomizer.String(10);
        await _client.TestClient.PutCommandAsync(someName, new ChangeNameRequest(newName), CancellationToken.None);

        AppTestContext.AppLogCatcher.Logs.Where(x => x.Message.StartsWith("REQUEST")).Should().AllSatisfy(x =>
        {
            x.Message.Should().NotContain("{").And.NotContain("}")
                .And.NotContain(newName)
                .And.Contain(RequestLoggingMiddleware.NoBodyLoggingString);
        });
    }
}