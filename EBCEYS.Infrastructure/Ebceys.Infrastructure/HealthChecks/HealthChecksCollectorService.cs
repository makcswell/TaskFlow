using System.Collections.Concurrent;
using RabbitMQ.Client;

namespace Ebceys.Infrastructure.HealthChecks;

/// <summary>
///     Internal service that collects health check dependencies (RabbitMQ connections and PostgreSQL connection strings)
///     registered during application startup. Used by <see cref="HealthchecksRegistrationExtensions" /> to automatically
///     register health checks for all known external dependencies.
/// </summary>
internal static class HealthChecksCollectorService
{
    /// <summary>
    ///     The collection of RabbitMQ connection factories registered for health checking.
    /// </summary>
    public static readonly ConcurrentBag<IConnectionFactory> Rabbits = [];

    /// <summary>
    ///     The collection of PostgreSQL connection strings registered for health checking.
    /// </summary>
    public static readonly ConcurrentBag<string> Psqls = [];
}