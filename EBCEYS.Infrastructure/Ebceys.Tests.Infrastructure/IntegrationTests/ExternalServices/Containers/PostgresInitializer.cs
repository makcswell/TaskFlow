using JetBrains.Annotations;
using Testcontainers.PostgreSql;

namespace Ebceys.Tests.Infrastructure.IntegrationTests.ExternalServices.Containers;

/// <summary>
///     Initializer for a PostgreSQL Testcontainer. Manages the lifecycle of a PostgreSQL Docker container
///     for use in integration tests. Configurable with custom user, password, database, and image.
/// </summary>
[PublicAPI]
public class PostgresInitializer : IDependencyInitializer<PostgreSqlContainer>
{
    /// <summary>
    ///     The postgres container image.
    /// </summary>
    public const string Image = "postgres:13.6";

    private PostgreSqlContainer? _container;

    /// <summary>
    ///     Initiates the new instance of <see cref="PostgresInitializer" />.
    /// </summary>
    /// <param name="user">The db user.</param>
    /// <param name="password">The db password.</param>
    /// <param name="database">The database name.</param>
    /// <param name="image">The image.</param>
    public PostgresInitializer(string user, string password, string? database = null, string image = Image)
    {
        PsqlBuilder = new PostgreSqlBuilder(image)
            .WithUsername(user).WithPassword(password);
        if (database != null)
        {
            PsqlBuilder.WithDatabase(database);
        }
    }

    /// <summary>
    ///     The psql builder.
    /// </summary>
    public PostgreSqlBuilder PsqlBuilder { get; }

    /// <inheritdoc />
    public async Task<PostgreSqlContainer> InitializeAsync(CancellationToken token = default)
    {
        _container = PsqlBuilder.Build();
        await _container.StartAsync(token);
        return _container;
    }

    /// <inheritdoc />
    public Task TeardownAsync(CancellationToken token = default)
    {
        return _container?.StopAsync(token) ?? Task.CompletedTask;
    }
}