using AwesomeAssertions;
using EBCEYS.ContainersEnvironment.HealthChecks;
using Ebceys.Infrastructure.HttpClient.ServiceClient;
using Ebceys.Tests.Infrastructure.Helpers;
using HealthChecks.UI.Core;

namespace Ebceys.Infrastructure.Tests.ClientTests;

public class ServiceSystemClientTests
{
    private IServiceSystemClient _appClient;
    private IServiceSystemClient _authClient;

    [SetUp]
    public void Setup()
    {
        _appClient = AppTestContext.AppContext.CreateServiceSystemClient();
        _authClient = AppTestContext.AuthAppClientContext.CreateServiceSystemClient();
    }

    [Test]
    public async Task When_ServicesStarted_With_PingAll_Result_AllOk()
    {
        var act1 = () => _appClient.PingAsync();
        var act2 = () => _authClient.PingAsync();

        await act1.Should().NotThrowAsync();
        await act2.Should().NotThrowAsync();
    }

    [Test]
    public async Task When_ServicesStarted_With_HealthzAll_Result_AllOk()
    {
        var act1 = () => _appClient.HealthCheckAsync();
        var act2 = () => _authClient.HealthCheckAsync();

        await act1.Should().NotThrowAsync();
        await act2.Should().NotThrowAsync();
    }

    [Test]
    public async Task When_ServicesStarted_With_HealthzStatusAll_Result_AllOk()
    {
        var act1 = await _appClient.HealthStatusCheckAsync();
        var act2 = await _authClient.HealthStatusCheckAsync();

        act1.Should().NotBeNull();
        act2.Should().NotBeNull();

        act1.Entries.Should().Contain(x => x.Key.StartsWith("psql"));
        act1.Entries.Should().Contain(x => x.Key.StartsWith("rabbit-mq"));

        act2.Entries.Should().Contain(x => x.Key.StartsWith("rabbit-mq"));
        act1.Status.Should().Be(UIHealthStatus.Healthy);
        act2.Status.Should().Be(UIHealthStatus.Healthy);
    }

    [Test]
    public async Task When_ServicesStarted_With_HealthzStatusUnhealthy_Result_Unhealthy()
    {
        var randomizer = EbRandomizer.Create();
        var message = randomizer.String();

        var ping = AppTestContext.AppContext.Factory.Services.GetRequiredService<PingServiceHealthStatusInfo>();
        ping.SetUnhealthyStatus(message);
        var act1 = await _appClient.HealthStatusCheckAsync();

        act1.Should().NotBeNull();
        act1.Status.Should().Be(UIHealthStatus.Unhealthy);
        act1.Entries.Should().Contain(x => x.Value.Description == message);
    }

    [Test]
    public async Task When_ServicesStarted_With_Metrics_Result_AllOk()
    {
        var metricsApp = await _appClient.GetMetricsAsync();
        var metricsAuth = await _authClient.GetMetricsAsync();

        metricsApp.Should().NotBeEmpty();
        metricsAuth.Should().NotBeEmpty();
    }
}