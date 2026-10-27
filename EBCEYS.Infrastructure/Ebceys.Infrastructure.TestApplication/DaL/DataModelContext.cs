using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ebceys.Infrastructure.TestApplication.DaL;

public class DataModelContext(DbContextOptions<DataModelContext> opts) : DbContext(opts)
{
    public DbSet<TestTableDbo> TestTable => Set<TestTableDbo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TestTableDbo>();
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasIndex(e => e.Id).IsUnique();
        entity.HasIndex(e => e.Name).IsUnique();
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql();
        base.OnConfiguring(optionsBuilder);
    }
}

[Table("test_table", Schema = "dbo")]
public class TestTableDbo
{
    [Key] [Required] public int Id { get; set; }

    [Column("name")] public required string Name { get; set; }
}