using Microsoft.EntityFrameworkCore;
using RoyalCode.OperationHint.Tests.Models;

namespace RoyalCode.OperationHint.Tests;

public class LocalDbContext : DbContext
{
    public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) 
    {
        Database.EnsureCreated();
    }

    public DbSet<SimpleEntity> SimpleEntities { get; set; } = null!;

    public DbSet<ComplexEntity> ComplexEntities { get; set; } = null!;
}
