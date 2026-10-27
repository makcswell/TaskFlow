using JetBrains.Annotations;
using Testcontainers.RabbitMq;

namespace Ebceys.Tests.Infrastructure.IntegrationTests.ExternalServices.Containers;

/// <summary>
///     Initializer for a RabbitMQ Testcontainer with the management plugin.
///     Manages the lifecycle of a RabbitMQ Docker container for use in integration tests.
///     Configurable with custom username, password, and image.
/// </summary>
[PublicAPI]
public class RabbitMqInitializer : IDependencyInitializer<RabbitMqContainer>
{
    /// <summary>
    ///     The default image.
    /// </summary>
    public const string Image = "rabbitmq:4.2.3-management";

    private RabbitMqContainer? _container;

    /// <summary>
    ///     Initiates the new instance of <see cref="RabbitMqInitializer" />.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <param name="image">The image.</param>
    public RabbitMqInitializer(string username, string password, string image = Image)
    {
        Builder = new RabbitMqBuilder(image).WithUsername(username).WithPassword(password);
    }

    /// <summary>
    ///     The rabbitMQ builder.
    /// </summary>
    public RabbitMqBuilder Builder { get; }

    /// <inheritdoc />
    public async Task<RabbitMqContainer> InitializeAsync(CancellationToken token = default)
    {
        _container = Builder.Build();
        await _container.StartAsync(token);
        return _container;
    }

    /// <inheritdoc />
    public Task TeardownAsync(CancellationToken token = default)
    {
        return _container?.StopAsync(token) ?? Task.CompletedTask;
    }
}