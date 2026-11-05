using Ebceys.Infrastructure.DatabaseRegistration.Conversions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Data;

namespace TasksService.Data;

/// <summary>
/// Database context.
/// </summary>
/// <param name="options">Context options.</param>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Tasks table.
    /// </summary>
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>().HasKey(x => x.Id);
    }

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetConverter>();
    }
}