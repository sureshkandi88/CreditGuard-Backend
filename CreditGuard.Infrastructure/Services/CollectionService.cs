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

public class CollectionService : ICollectionService
{
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;
    private readonly IGroupService _groupService;

    public CollectionService(AppDbContext context, IWalletService walletService, IGroupService groupService)
    {
        _context = context;
        _walletService = walletService;
        _groupService = groupService;
    }

    public async Task<DailyCollection> RecordCollectionAsync(string creditId, long amountPaid, string collectedBy, string? notes, bool isAdvancePayment)
    {
        var credit = await _context.Credits
            .Include(c => c.Group)
            .ThenInclude(g => g.GroupMembers)
            .Include(c => c.Group)
            .Include(c => c.DailyCollections)
            .FirstOrDefaultAsync(c => c.Id == creditId);

        if (credit == null) throw new Exception("Credit not found.");
        if (credit.Status != "ACTIVE") throw new Exception("Can only collect against active credits.");

        // Calculate total due and total paid so far
        long totalDue = credit.TotalPrincipal + credit.TotalInterest;
        long currentlyPaid = credit.DailyCollections.Sum(dc => dc.AmountPaid);
        long remaining = totalDue - currentlyPaid;

        if (amountPaid > remaining) throw new Exception($"Amount paid cannot exceed remaining balance of {remaining}.");

        var dailyCollection = new DailyCollection
        {
            CreditId = creditId,
            CollectionDate = DateHelper.GetCurrentUnixTimeSeconds(),
            AmountPaid = amountPaid,
            CollectedBy = collectedBy,
            Notes = notes,
            IsAdvancePayment = isAdvancePayment
        };

        // Payment allocations based on ratio
        foreach (var member in credit.Group.GroupMembers)
        {
            long amountAllocated = (long)Math.Round(amountPaid * member.Ratio, MidpointRounding.AwayFromZero);
            dailyCollection.PaymentAllocations.Add(new PaymentAllocation
            {
                CustomerId = member.CustomerId,
                AmountAllocated = amountAllocated
            });
        }

        _context.DailyCollections.Add(dailyCollection);

        // Add money to wallet
        await _walletService.CreditMoneyAsync(collectedBy, amountPaid, $"Collection from Group {credit.Group.Name}");

        // Check if fully paid
        if (currentlyPaid + amountPaid >= totalDue)
        {
            credit.Status = "COMPLETED";
            await _groupService.DeactivateGroupAsync(credit.GroupId);
        }

        await _context.SaveChangesAsync();
        return dailyCollection;
    }

    public async Task<IEnumerable<DailyCollection>> GetTodaysCollectionsAsync()
    {
        long now = DateHelper.GetCurrentUnixTimeSeconds();
        
        var allCollections = await _context.DailyCollections
            .Include(dc => dc.Credit)
            .ThenInclude(c => c.Group)
            .Include(dc => dc.Creditor)
            .ToListAsync();

        return allCollections.Where(dc => DateHelper.IsSameDay(dc.CollectionDate, now)).OrderByDescending(dc => dc.CollectionDate);
    }

    public async Task<IEnumerable<DailyCollection>> GetCollectionsByCreditAsync(string creditId)
    {
        return await _context.DailyCollections
            .Include(dc => dc.Creditor)
            .Include(dc => dc.PaymentAllocations)
            .Where(dc => dc.CreditId == creditId)
            .OrderBy(dc => dc.CollectionDate)
            .ToListAsync();
    }

    public async Task<(long TotalCollectedToday, long TotalExpectedToday)> GetDashboardCollectionStatsAsync()
    {
        long now = DateHelper.GetCurrentUnixTimeSeconds();
        
        // Get all active credits to calculate expected amount
        var activeCredits = await _context.Credits
            .Where(c => c.Status == "ACTIVE")
            .ToListAsync();

        long totalExpected = activeCredits.Sum(c => c.DailyInstallment);

        // Get today's collected amount
        var todaysCollections = await GetTodaysCollectionsAsync();
        long totalCollectedToday = todaysCollections.Sum(dc => dc.AmountPaid);

        return (totalCollectedToday, totalExpected);
    }
}
