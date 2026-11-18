using Microsoft.EntityFrameworkCore;

namespace Elwise.Api.Data;

/// <summary>
/// The server's database. It has one table, Prices.
/// </summary>
public sealed class ElwiseDbContext : DbContext
{
    public ElwiseDbContext(DbContextOptions<ElwiseDbContext> options)
        : base(options)
    {
    }

    public DbSet<StoredPrice> Prices => Set<StoredPrice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // The same quarter can only be saved once per price area.
        modelBuilder.Entity<StoredPrice>()
            .HasIndex(price => new { price.Area, price.StartUtc })
            .IsUnique();
    }
}