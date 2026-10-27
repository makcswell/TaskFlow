using Quartz;

namespace Ebceys.Infrastructure.Tests.ScheduledTests.Helpers;

public class ScheduledTestJob : IJob
{
    public static int Times { get; private set; }
    public static bool IsCalled { get; private set; }

    public Task Execute(IJobExecutionContext context)
    {
        IsCalled = true;
        Times++;
        return Task.CompletedTask;
    }
}