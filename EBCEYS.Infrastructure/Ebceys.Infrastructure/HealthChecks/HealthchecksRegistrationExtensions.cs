using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ebceys.Infrastructure.HealthChecks;

/// <summary>
///     Internal extension methods for registering health checks for collected external dependencies.
/// </summary>
internal static class HealthchecksRegistrationExtensions
{
    private const string PsqlHealthNamePrefix = "psql";
    private const string RabbitHealthNamePrefix = "rabbit-mq";

    extension(IHealthChecksBuilder hcBuilder)
    {
        /// <summary>
        ///     Registers RabbitMQ health checks for all RabbitMQ connections collected by
        ///     <see cref="HealthChecksCollectorService" />.
        /// </summary>
        /// <param name="configuration">
        ///     The health check configuration. If <c>null</c>, the default configuration will be used.
        /// </param>
        public void AddRabbitMqHealthChecks(HealthCheckConfiguration? configuration = null)
        {
            configuration ??= new HealthCheckConfiguration();
            var num = 1;
            foreach (var rabbit in HealthChecksCollectorService.Rabbits)
            {
                hcBuilder.AddRabbitMQ(_ => rabbit.CreateConnectionAsync(),
                    $"{RabbitHealthNamePrefix}-{configuration.NameFactory(num++)}",
                    configuration.FailureStatus,
                    configuration.Tags,
                    configuration.Timeout);
            }
        }

        /// <summary>
        ///     Registers PostgreSQL (Npgsql) health checks for all PostgreSQL connection strings collected by
        ///     <see cref="HealthChecksCollectorService" />.
        /// </summary>
        /// <param name="configuration">
        ///     The health check configuration. If <c>null</c>, the default configuration will be used.
        /// </param>
        public void AddNpgsqlHealthChecks(HealthCheckConfiguration? configuration = null)
        {
            configuration ??= new HealthCheckConfiguration();
            var num = 1;
            foreach (var psql in HealthChecksCollectorService.Psqls)
            {
                hcBuilder.AddNpgSql(psql,
                    name: $"{PsqlHealthNamePrefix}-{configuration.NameFactory(num++)}",
                    failureStatus: configuration.FailureStatus,
                    tags: configuration.Tags,
                    timeout: configuration.Timeout);
            }
        }
    }
}

/// <summary>
///     Configuration options for health check registration, including naming, tags, failure status, and timeout.
/// </summary>
[PublicAPI]
public record HealthCheckConfiguration
{
    /// <summary>
    ///     The health check name factory, where param is index of health check if there are more than one health check of same
    ///     type.
    /// </summary>
    public Func<int, string> NameFactory { get; init; } = ind => ind.ToString();

    /// <summary>
    ///     The tags.
    /// </summary>
    public IEnumerable<string>? Tags { get; init; }

    /// <summary>
    ///     The failure status.
    /// </summary>
    public HealthStatus FailureStatus { get; init; } = HealthStatus.Unhealthy;

    /// <summary>
    ///     The timeout.
    /// </summary>
    public TimeSpan? Timeout { get; init; }
}