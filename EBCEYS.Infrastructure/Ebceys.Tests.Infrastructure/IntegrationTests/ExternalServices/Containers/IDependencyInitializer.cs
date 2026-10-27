using JetBrains.Annotations;

namespace Ebceys.Tests.Infrastructure.IntegrationTests.ExternalServices.Containers;

/// <summary>
///     Interface for initializing and tearing down external test dependencies (e.g., Docker containers).
///     Implementations manage the lifecycle of infrastructure components required for integration tests.
/// </summary>
/// <typeparam name="TDependency">The type of the initialized dependency (e.g., a Testcontainers container).</typeparam>
[PublicAPI]
public interface IDependencyInitializer<TDependency>
{
    /// <summary>
    ///     Initializes the external dependency (e.g., starts a Docker container).
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The initialized dependency instance.</returns>
    Task<TDependency> InitializeAsync(CancellationToken token = default);

    /// <summary>
    ///     Tears down the external dependency (e.g., stops and removes a Docker container).
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> representing the teardown operation.</returns>
    Task TeardownAsync(CancellationToken token = default);
}