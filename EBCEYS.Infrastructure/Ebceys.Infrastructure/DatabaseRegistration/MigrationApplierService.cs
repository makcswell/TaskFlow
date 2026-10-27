using Ebceys.Infrastructure.Services.ExecutedServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ebceys.Infrastructure.DatabaseRegistration;

/// <summary>
///     Service that applies database migrations on application startup before hosting begins.
///     Only runs if <see cref="DbContextRegistrationOptions.MigrateDb" /> is set to <c>true</c>.
/// </summary>
/// <param name="opts">The db context registration options.</param>
/// <param name="dbContextFactory">The db context factory.</param>
/// <typeparam name="TDbContext">The db context type whose migrations should be applied.</typeparam>
[PublicAPI]
public class MigrationApplierService<TDbContext>(
    IOptions<DbContextRegistrationOptions> opts,
    IDbContextFactory<TDbContext> dbContextFactory) : IBeforeHostingStartedService
    where TDbContext : DbContext
{
    /// <summary>
    ///     Applies the database migrations if <see cref="DbContextRegistrationOptions.MigrateDb" /> is enabled.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous migration operation.</returns>
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!opts.Value.MigrateDb)
        {
            return;
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}