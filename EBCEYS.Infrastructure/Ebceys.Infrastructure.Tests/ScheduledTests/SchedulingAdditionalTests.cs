using AwesomeAssertions;
using Ebceys.Infrastructure.Extensions;
using Ebceys.Infrastructure.Tests.ScheduledTests.Helpers;

namespace Ebceys.Infrastructure.Tests.ScheduledTests;

public class SchedulingAdditionalTests
{
    // ── Counter grows over time ───────────────────────────────────────────────

    [Test]
    public async Task When_ServiceStarted_With_ScheduledJob_Result_JobTriggersMultipleTimes()
    {
        // Job is scheduled with interval 100ms — wait for at least 3 executions
        var initialCount = ScheduledTestJob.Times;

        await Task.WaitUntilAsync(
            _ => ScheduledTestJob.Times >= initialCount + 3,
            TimeSpan.FromSeconds(5));

        ScheduledTestJob.Times.Should().BeGreaterThanOrEqualTo(initialCount + 3);
    }

    [Test]
    public async Task When_ServiceStarted_With_ScheduledJob_Result_CounterMonotonicallyIncreases()
    {
        var snapshot1 = ScheduledTestJob.Times;

        await Task.WaitUntilAsync(
            _ => ScheduledTestJob.Times > snapshot1,
            TimeSpan.FromSeconds(5));

        var snapshot2 = ScheduledTestJob.Times;

        await Task.WaitUntilAsync(
            _ => ScheduledTestJob.Times > snapshot2,
            TimeSpan.FromSeconds(5));

        ScheduledTestJob.Times.Should().BeGreaterThan(snapshot2);
        snapshot2.Should().BeGreaterThan(snapshot1);
    }
}