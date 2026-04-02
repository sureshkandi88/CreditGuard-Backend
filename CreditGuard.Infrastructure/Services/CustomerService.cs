using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CreditGuard.Core.Entities;
using CreditGuard.Core.Interfaces;
using CreditGuard.Core.Utilities;
using CreditGuard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CreditGuard.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer> CreateCustomerAsync(string aadhaar, string firstName, string lastName, string phone, string address, string? photoId)
    {
        var exists = await _context.Customers.AnyAsync(c => c.Aadhaar == aadhaar);
        if (exists) throw new Exception("Customer with this Aadhaar already exists.");

        var customer = new Customer
        {
            Aadhaar = aadhaar,
            FirstName = firstName,
            LastName = lastName,
            Phone = phone,
            Address = address,
            PhotoId = photoId,
            CreatedAt = DateHelper.GetCurrentUnixTimeSeconds()
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer?> GetCustomerAsync(string customerId)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
    }

    public async Task<IEnumerable<Customer>> SearchCustomersAsync(string query)
    {
        var lowerQuery = query.ToLower();
        return await _context.Customers
            .Where(c => c.Aadhaar.Contains(lowerQuery) || c.Phone.Contains(lowerQuery) || c.FirstName.ToLower().Contains(lowerQuery) || c.LastName.ToLower().Contains(lowerQuery))
            .OrderByDescending(c => c.CreatedAt)
            .Take(50)
            .ToListAsync();
    }

    public async Task<bool> IsCustomerInActiveGroupAsync(string customerId)
    {
        var hasActiveGroup = await _context.GroupMembers
            .Include(gm => gm.Group)
            .ThenInclude(g => g!.Credits)
            .Where(gm => gm.CustomerId == customerId)
            .AnyAsync(gm => gm.Group!.Credits.Any(c => c.Status == "ACTIVE"));

        return hasActiveGroup;
    }
}
