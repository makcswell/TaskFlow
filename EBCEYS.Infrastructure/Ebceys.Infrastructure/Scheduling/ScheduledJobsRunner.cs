using Ebceys.Infrastructure.Services.ExecutedServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Ebceys.Infrastructure.Scheduling;

/// <summary>
///     Internal service that configures and registers scheduled Quartz.NET jobs before hosting starts.
///     Invokes the user-provided job configuration delegate from <see cref="ScheduledJobsRunnerOptions" />.
/// </summary>
internal class ScheduledJobsRunner(
    ISchedulerFactory schedulerFactory,
    IOptions<ScheduledJobsRunnerOptions> options,
    ILogger<ScheduledJobsRunner> logger)
    : IBeforeHostingStartedService
{
    /// <summary>
    ///     Configures and schedules all registered jobs using the provided scheduler factory.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> representing the job scheduling operation.</returns>
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Scheduling jobs configuring...");
        return options.Value.ConfigureSchedulerJobs(schedulerFactory);
    }
}

/// <summary>
///     Internal options containing the delegate for configuring scheduled jobs.
/// </summary>
internal class ScheduledJobsRunnerOptions
{
    /// <summary>
    ///     The delegate that configures scheduler jobs using the provided <see cref="ISchedulerFactory" />.
    /// </summary>
    public required Func<ISchedulerFactory, Task> ConfigureSchedulerJobs { get; internal set; }
}