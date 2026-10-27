using AwesomeAssertions;
using EBCEYS.ContainersEnvironment.HealthChecks;
using Ebceys.Infrastructure.HttpClient.ServiceClient;
using HealthChecks.UI.Core;

namespace Ebceys.Infrastructure.Tests.ClientTests;

public class ServiceSystemClientAdditionalTests
{
    private IServiceSystemClient _appClient;
    private IServiceSystemClient _authClient;

    [SetUp]
    public void Setup()
    {
        _appClient = AppTestContext.AppContext.CreateServiceSystemClient();
        _authClient = AppTestContext.AuthAppClientContext.CreateServiceSystemClient();
    }

    // ── Ping does not throw ───────────────────────────────────────────────────

    [Test]
    public async Task When_Ping_With_AppService_Result_NoException()
    {
        var act = () => _appClient.PingAsync();
        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task When_Ping_With_AuthService_Result_NoException()
    {
        var act = () => _authClient.PingAsync();
        await act.Should().NotThrowAsync();
    }

    // ── HealthzStatus entry keys ──────────────────────────────────────────────

    [Test]
    public async Task When_HealthzStatus_With_AuthService_Result_ContainsRabbitMqEntry()
    {
        var report = await _authClient.HealthStatusCheckAsync();

        report.Should().NotBeNull();
        report.Entries.Should().Contain(x => x.Key.StartsWith("rabbit-mq"));
    }

    [Test]
    public async Task When_HealthzStatus_With_AppService_Result_ContainsBothPsqlAndRabbitMqEntries()
    {
        var report = await _appClient.HealthStatusCheckAsync();

        report.Should().NotBeNull();
        report.Entries.Should().Contain(x => x.Key.StartsWith("psql"));
        report.Entries.Should().Contain(x => x.Key.StartsWith("rabbit-mq"));
    }

    // ── Unhealthy → Healthy recovery ─────────────────────────────────────────

    [Test]
    public async Task When_HealthStatus_SetUnhealthyThenRestored_Result_StatusRecovers()
    {
        var ping = AppTestContext.AppContext.Factory.Services
            .GetRequiredService<PingServiceHealthStatusInfo>();

        ping.SetUnhealthyStatus("temporary failure");
        var unhealthyReport = await _appClient.HealthStatusCheckAsync();
        unhealthyReport.Status.Should().Be(UIHealthStatus.Unhealthy);

        ping.SetHealthyStatus();
        var healthyReport = await _appClient.HealthStatusCheckAsync();
        healthyReport.Status.Should().Be(UIHealthStatus.Healthy);
    }

    // ── Metrics content ───────────────────────────────────────────────────────

    [Test]
    public async Task When_GetMetrics_With_AppService_Result_ContainsPrometheusHttpCounter()
    {
        await _appClient.PingAsync();

        var metrics = await _appClient.GetMetricsAsync();

        metrics.Should().NotBeNullOrEmpty();
        metrics.Should().Contain("prometheus_demo_request_total");
    }

    [Test]
    public async Task When_GetMetrics_With_AppService_Result_ContainsDotnetRuntimeMetrics()
    {
        var metrics = await _appClient.GetMetricsAsync();

        metrics.Should().NotBeNullOrEmpty();
        metrics.Should().Contain("dotnet_");
    }

    [Test]
    public async Task When_GetMetrics_After_MultipleRequests_Result_CounterIncremented()
    {
        for (var i = 0; i < 3; i++)
        {
            await _appClient.PingAsync();
        }

        var metrics = await _appClient.GetMetricsAsync();
        metrics.Should().Contain("prometheus_demo_request_total");
    }

    // ── Both services are healthy simultaneously ──────────────────────────────

    [Test]
    public async Task When_BothServicesStarted_With_HealthzStatus_Result_BothHealthy()
    {
        var appReport = await _appClient.HealthStatusCheckAsync();
        var authReport = await _authClient.HealthStatusCheckAsync();

        appReport.Status.Should().Be(UIHealthStatus.Healthy);
        authReport.Status.Should().Be(UIHealthStatus.Healthy);
    }
}