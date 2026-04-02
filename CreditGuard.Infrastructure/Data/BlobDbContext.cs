using CreditGuard.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreditGuard.Infrastructure.Data;

public class BlobDbContext : DbContext
{
    public BlobDbContext(DbContextOptions<BlobDbContext> options) : base(options)
    {
    }

    public DbSet<Blob> Blobs => Set<Blob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
