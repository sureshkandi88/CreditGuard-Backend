using System;
using System.Collections.Generic;

namespace CreditGuard.API.DTOs;

public class RegisterRequest
{
    public string Username { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class LoginRequest
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class AddMoneyRequest
{
    public long Amount { get; set; }
    public string? Reason { get; set; }
}

public class CreateCustomerRequest
{
    public string Aadhaar { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Address { get; set; } = default!;
    // Photo handled via multipart/form-data separately or base64. 
    // Wait, the prompt said photo can be uploaded as multipart/form-data.
}

public class CreateGroupRequest
{
    public string Name { get; set; } = default!;
    public string Location { get; set; } = default!;
    public string GroupLeaderId { get; set; } = default!;
}

public class AddGroupMemberRequest
{
    public double Ratio { get; set; }
}

public class CreateCreditRequest
{
    public string GroupId { get; set; } = default!;
    public long TotalPrincipal { get; set; }
    public Dictionary<string, double>? MemberRatios { get; set; }
}

public class RecordCollectionRequest
{
    public string CreditId { get; set; } = default!;
    public long AmountPaid { get; set; }
    public string? Notes { get; set; }
    public bool IsAdvancePayment { get; set; }
}
