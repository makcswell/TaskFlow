using Ebceys.Infrastructure.Services.ExecutedServices;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Ebceys.Infrastructure.Scheduling;

/// <summary>
///     Extension methods for configuring Quartz.NET-based job scheduling in the DI container.
///     Provides methods to register the scheduling infrastructure and to schedule jobs
///     with interval-based or cron-based triggers.
/// </summary>
[PublicAPI]
public static class SchedulingExtensions
{
    /// <summary>
    ///     Adds the scheduling services to <paramref name="services" />.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="waitForJobsToComplete">True for set wait for scheduled jobs ends until running next.</param>
    /// <param name="configureSchedulerJobs">The jobs' configuration.</param>
    /// <returns>The instance of <paramref name="services" />.</returns>
    public static IServiceCollection AddSchedulingServices(
        this IServiceCollection services,
        bool waitForJobsToComplete,
        Func<ISchedulerFactory, Task> configureSchedulerJobs)
    {
        services.AddQuartz(opts =>
        {
            opts.UseInMemoryStore();
            opts.InterruptJobsOnShutdownWithWait = true;
            opts.InterruptJobsOnShutdown = true;
            opts.UseDefaultThreadPool();
        });
        services.AddQuartzHostedService(opts =>
        {
            opts.WaitForJobsToComplete = waitForJobsToComplete;
            opts.AwaitApplicationStarted = true;
        });

        services.AddOptions<ScheduledJobsRunnerOptions>()
            .Configure(opts => { opts.ConfigureSchedulerJobs = configureSchedulerJobs; });

        services.AddBeforeHostingStarted<ScheduledJobsRunner>();

        return services;
    }

    /// <summary>
    ///     Schedules the <typeparamref name="TJob" /> with specified <paramref name="interval" />.
    /// </summary>
    /// <param name="schedulerFactory">The scheduler factory.</param>
    /// <param name="jobIdentity">The job identity.</param>
    /// <param name="interval">The job execution interval.</param>
    /// <param name="delay">The delay.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="TJob">The job to schedule.</typeparam>
    public static async Task ScheduleJobAsync<TJob>(
        this ISchedulerFactory schedulerFactory,
        string jobIdentity,
        TimeSpan interval,
        TimeSpan delay = default,
        CancellationToken cancellationToken = default)
        where TJob : IJob
    {
        var jobDetail = JobBuilder.Create<TJob>()
            .WithIdentity(jobIdentity)
            .Build();

        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        var trigger = TriggerBuilder.Create()
            .StartAt(DateTimeOffset.Now.Add(delay))
            .WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever())
            .Build();

        await scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
    }

    /// <summary>
    ///     Schedules the <typeparamref name="TJob" /> with specified <paramref name="cronExpression" />.
    /// </summary>
    /// <param name="schedulerFactory">The scheduler factory.</param>
    /// <param name="jobIdentity">The job identity.</param>
    /// <param name="cronExpression">The job execution cron expression.</param>
    /// <param name="delay">The delay.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="TJob">The job to schedule.</typeparam>
    public static async Task ScheduleJobAsync<TJob>(
        ISchedulerFactory schedulerFactory,
        string jobIdentity,
        string cronExpression,
        TimeSpan delay = default,
        CancellationToken cancellationToken = default)
        where TJob : IJob
    {
        var jobDetail = JobBuilder.Create<TJob>()
            .WithIdentity(jobIdentity)
            .Build();

        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        var trigger = TriggerBuilder.Create()
            .StartAt(DateTimeOffset.Now.Add(delay))
            .WithCronSchedule(cronExpression)
            .Build();

        await scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
    }
}