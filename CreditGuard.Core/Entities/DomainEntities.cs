using System;
using System.Collections.Generic;

namespace CreditGuard.Core.Entities;

public class Creditor
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}

public class WalletTransaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CreditorId { get; set; } = default!;
    public long Amount { get; set; }
    public string Type { get; set; } = default!; // "CREDIT" or "DEBIT"
    public string? Reason { get; set; }
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public Creditor? Creditor { get; set; }
}

public class Customer
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Aadhaar { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string? PhotoId { get; set; }
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}

public class CustomerGroup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = default!;
    public string Location { get; set; } = default!;
    public string GroupLeaderId { get; set; } = default!;
    public string? GroupPhotoId { get; set; }
    public bool IsActive { get; set; }
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public Customer? GroupLeader { get; set; }
    public ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
    public ICollection<Credit> Credits { get; set; } = new List<Credit>();
}

public class GroupMember
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string GroupId { get; set; } = default!;
    public string CustomerId { get; set; } = default!;
    public double Ratio { get; set; } // decimal between 0 and 1

    public CustomerGroup? Group { get; set; }
    public Customer? Customer { get; set; }
}

public class Credit
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string GroupId { get; set; } = default!;
    public long TotalPrincipal { get; set; }
    public long TotalInterest { get; set; }
    public long DailyInstallment { get; set; }
    public long StartDate { get; set; }
    public long EndDate { get; set; }
    public string Status { get; set; } = "ACTIVE"; // "ACTIVE", "CLOSED", "COMPLETED"

    public CustomerGroup? Group { get; set; }
    public ICollection<DailyCollection> DailyCollections { get; set; } = new List<DailyCollection>();
}

public class DailyCollection
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CreditId { get; set; } = default!;
    public long CollectionDate { get; set; }
    public long AmountPaid { get; set; }
    public string CollectedBy { get; set; } = default!;
    public string? Notes { get; set; }
    public bool IsAdvancePayment { get; set; }

    public Credit? Credit { get; set; }
    public Creditor? Creditor { get; set; }
    public ICollection<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();
}

public class PaymentAllocation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DailyCollectionId { get; set; } = default!;
    public string CustomerId { get; set; } = default!;
    public long AmountAllocated { get; set; }

    public DailyCollection? DailyCollection { get; set; }
    public Customer? Customer { get; set; }
}

public class Blob
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = default!;
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}
