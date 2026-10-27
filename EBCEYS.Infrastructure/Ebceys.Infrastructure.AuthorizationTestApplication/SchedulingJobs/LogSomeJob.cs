using Quartz;

namespace Ebceys.Infrastructure.AuthorizationTestApplication.SchedulingJobs;

public class LogSomeJob(ILogger<LogSomeJob> logger) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("LogSomeJob");
        return Task.CompletedTask;
    }
}