using CreditGuard.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreditGuard.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Creditor> Creditors => Set<Creditor>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerGroup> CustomerGroups => Set<CustomerGroup>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<Credit> Credits => Set<Credit>();
    public DbSet<DailyCollection> DailyCollections => Set<DailyCollection>();
    public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Creditor>()
            .HasIndex(c => c.Username)
            .IsUnique();

        modelBuilder.Entity<Creditor>()
            .HasIndex(c => c.Phone)
            .IsUnique();

        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Aadhaar)
            .IsUnique();
            
        // Creditor -> WalletTransactions
        modelBuilder.Entity<WalletTransaction>()
            .HasOne(wt => wt.Creditor)
            .WithMany(c => c.WalletTransactions)
            .HasForeignKey(wt => wt.CreditorId);

        // Group -> GroupLeader (Customer)
        modelBuilder.Entity<CustomerGroup>()
            .HasOne(g => g.GroupLeader)
            .WithMany()
            .HasForeignKey(g => g.GroupLeaderId);

        // GroupMember -> Group
        modelBuilder.Entity<GroupMember>()
            .HasOne(gm => gm.Group)
            .WithMany(g => g.GroupMembers)
            .HasForeignKey(gm => gm.GroupId);

        // GroupMember -> Customer
        modelBuilder.Entity<GroupMember>()
            .HasOne(gm => gm.Customer)
            .WithMany()
            .HasForeignKey(gm => gm.CustomerId);

        // Credit -> Group
        modelBuilder.Entity<Credit>()
            .HasOne(c => c.Group)
            .WithMany(g => g.Credits)
            .HasForeignKey(c => c.GroupId);

        // DailyCollection -> Credit
        modelBuilder.Entity<DailyCollection>()
            .HasOne(dc => dc.Credit)
            .WithMany(c => c.DailyCollections)
            .HasForeignKey(dc => dc.CreditId);

        // DailyCollection -> Creditor
        modelBuilder.Entity<DailyCollection>()
            .HasOne(dc => dc.Creditor)
            .WithMany()
            .HasForeignKey(dc => dc.CollectedBy);

        // PaymentAllocation -> DailyCollection
        modelBuilder.Entity<PaymentAllocation>()
            .HasOne(pa => pa.DailyCollection)
            .WithMany(dc => dc.PaymentAllocations)
            .HasForeignKey(pa => pa.DailyCollectionId);

        // PaymentAllocation -> Customer
        modelBuilder.Entity<PaymentAllocation>()
            .HasOne(pa => pa.Customer)
            .WithMany()
            .HasForeignKey(pa => pa.CustomerId);
    }
}
