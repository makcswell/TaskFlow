using Ebceys.Infrastructure.DatabaseRegistration.Conversions;
using Microsoft.EntityFrameworkCore;
using TasksService.Models;

namespace TasksService.Data;

/// <summary>
/// Контекст базы данных.
/// </summary>
/// <param name="options">Опции контекста.</param>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Таблица задач.
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