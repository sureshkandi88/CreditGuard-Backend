using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CreditGuard.Core.Entities;
using CreditGuard.Core.Interfaces;
using CreditGuard.Core.Utilities;
using CreditGuard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CreditGuard.Infrastructure.Services;

public class WalletService : IWalletService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public WalletService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    private string GetCacheKey(string creditorId) => $"wallet_balance_{creditorId}";

    public async Task<long> GetBalanceAsync(string creditorId)
    {
        if (_cache.TryGetValue(GetCacheKey(creditorId), out long balance))
        {
            return balance;
        }

        var credits = await _context.WalletTransactions
            .Where(t => t.CreditorId == creditorId && t.Type == "CREDIT")
            .SumAsync(t => t.Amount);

        var debits = await _context.WalletTransactions
            .Where(t => t.CreditorId == creditorId && t.Type == "DEBIT")
            .SumAsync(t => t.Amount);

        balance = credits - debits;
        _cache.Set(GetCacheKey(creditorId), balance, TimeSpan.FromMinutes(5));

        return balance;
    }

    public async Task AddMoneyAsync(string creditorId, long amount, string? reason)
    {
        if (amount <= 0) throw new Exception("Amount must be positive.");

        var tx = new WalletTransaction
        {
            CreditorId = creditorId,
            Amount = amount,
            Type = "CREDIT",
            Reason = reason ?? "Added money",
            CreatedAt = DateHelper.GetCurrentUnixTimeSeconds()
        };

        _context.WalletTransactions.Add(tx);
        await _context.SaveChangesAsync();
        _cache.Remove(GetCacheKey(creditorId));
    }

    public async Task<bool> DebitMoneyAsync(string creditorId, long amount, string? reason)
    {
        var balance = await GetBalanceAsync(creditorId);
        if (balance < amount) return false;

        var tx = new WalletTransaction
        {
            CreditorId = creditorId,
            Amount = amount,
            Type = "DEBIT",
            Reason = reason ?? "Debited money",
            CreatedAt = DateHelper.GetCurrentUnixTimeSeconds()
        };

        _context.WalletTransactions.Add(tx);
        await _context.SaveChangesAsync();
        _cache.Remove(GetCacheKey(creditorId));
        return true;
    }

    public async Task CreditMoneyAsync(string creditorId, long amount, string? reason)
    {
        var tx = new WalletTransaction
        {
            CreditorId = creditorId,
            Amount = amount,
            Type = "CREDIT",
            Reason = reason ?? "Credited money",
            CreatedAt = DateHelper.GetCurrentUnixTimeSeconds()
        };

        _context.WalletTransactions.Add(tx);
        await _context.SaveChangesAsync();
        _cache.Remove(GetCacheKey(creditorId));
    }

    public async Task<IEnumerable<WalletTransaction>> GetTransactionsAsync(string creditorId)
    {
        return await _context.WalletTransactions
            .Where(t => t.CreditorId == creditorId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
}
