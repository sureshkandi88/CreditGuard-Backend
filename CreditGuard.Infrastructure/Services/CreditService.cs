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

public class CreditService : ICreditService
{
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;
    private readonly IGroupService _groupService;

    public CreditService(AppDbContext context, IWalletService walletService, IGroupService groupService)
    {
        _context = context;
        _walletService = walletService;
        _groupService = groupService;
    }

    public async Task<Credit> CreateCreditAsync(string groupId, long totalPrincipal, string creditorId, Dictionary<string, double>? memberRatios)
    {
        var group = await _context.CustomerGroups
            .Include(g => g.GroupMembers)
            .FirstOrDefaultAsync(g => g.Id == groupId);
            
        if (group == null) throw new Exception("Group not found.");
        if (group.IsActive) throw new Exception("Group already has an active credit.");

        // Check if members have ratios, if not use the provided ones
        if (group.GroupMembers.Count == 0 && (memberRatios == null || !memberRatios.Any()))
            throw new Exception("Group has no members and no ratios were provided.");

        if (memberRatios != null && memberRatios.Any())
        {
            // Validate sum of ratios
            double sum = memberRatios.Values.Sum();
            if (Math.Abs(sum - 1.0) > 0.01) throw new Exception("Sum of ratios must be exactly 1.");

            // Apply new ratios
            foreach (var kvp in memberRatios)
            {
                var member = group.GroupMembers.FirstOrDefault(m => m.CustomerId == kvp.Key);
                if (member == null)
                {
                    member = new GroupMember { GroupId = groupId, CustomerId = kvp.Key, Ratio = kvp.Value };
                    _context.GroupMembers.Add(member);
                }
                else
                {
                    member.Ratio = kvp.Value;
                }
            }
            await _context.SaveChangesAsync(); // save members to avoid issues later
        }

        // Calculations
        long totalInterest = (long)Math.Round(totalPrincipal * 0.10, MidpointRounding.AwayFromZero);
        long totalDue = totalPrincipal + totalInterest;
        long dailyInstallment = (long)Math.Round((double)totalDue / 100, MidpointRounding.AwayFromZero);
        
        long startUnixTime = DateHelper.GetCurrentUnixTimeSeconds();
        long endUnixTime = DateHelper.AddWeekdaysExcludingSunday(startUnixTime, 100);

        // Deduct from wallet
        var debited = await _walletService.DebitMoneyAsync(creditorId, totalPrincipal, $"Credit disbursed to Group {group.Name}");
        if (!debited) throw new Exception("Insufficient wallet balance for credit disbursement.");

        var credit = new Credit
        {
            GroupId = groupId,
            TotalPrincipal = totalPrincipal,
            TotalInterest = totalInterest,
            DailyInstallment = dailyInstallment,
            StartDate = startUnixTime,
            EndDate = endUnixTime,
            Status = "ACTIVE"
        };

        _context.Credits.Add(credit);
        
        await _groupService.ActivateGroupAsync(groupId); // SaveChanges implicitly via group service wait not here.
        await _context.SaveChangesAsync();

        return credit;
    }

    public async Task<Credit?> GetCreditAsync(string creditId)
    {
        return await _context.Credits
            .Include(c => c.Group)
            .FirstOrDefaultAsync(c => c.Id == creditId);
    }

    public async Task<IEnumerable<Credit>> GetActiveCreditsAsync()
    {
        return await _context.Credits
            .Include(c => c.Group)
            .Where(c => c.Status == "ACTIVE")
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();
    }

    public async Task CloseCreditEarlyAsync(string creditId)
    {
        var credit = await _context.Credits.FirstOrDefaultAsync(c => c.Id == creditId);
        if (credit == null) throw new Exception("Credit not found.");
        if (credit.Status != "ACTIVE") throw new Exception("Only active credits can be closed.");

        credit.Status = "CLOSED";
        await _groupService.DeactivateGroupAsync(credit.GroupId);
        await _context.SaveChangesAsync();
    }
}
