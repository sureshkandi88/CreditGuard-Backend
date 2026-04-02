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

public class GroupService : IGroupService
{
    private readonly AppDbContext _context;
    private readonly ICustomerService _customerService;
    private readonly IMemoryCache _cache;

    public GroupService(AppDbContext context, ICustomerService customerService, IMemoryCache cache)
    {
        _context = context;
        _customerService = customerService;
        _cache = cache;
    }

    public async Task<CustomerGroup> CreateGroupAsync(string name, string location, string groupLeaderId, string? groupPhotoId)
    {
        var leaderExists = await _context.Customers.AnyAsync(c => c.Id == groupLeaderId);
        if (!leaderExists) throw new Exception("Group leader does not exist.");

        var group = new CustomerGroup
        {
            Name = name,
            Location = location,
            GroupLeaderId = groupLeaderId,
            GroupPhotoId = groupPhotoId,
            IsActive = false,
            CreatedAt = DateHelper.GetCurrentUnixTimeSeconds()
        };

        _context.CustomerGroups.Add(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task<CustomerGroup?> GetGroupAsync(string groupId)
    {
        return await _context.CustomerGroups
            .Include(g => g.GroupLeader)
            .Include(g => g.GroupMembers)
                .ThenInclude(gm => gm.Customer)
            .Include(g => g.Credits)
            .FirstOrDefaultAsync(g => g.Id == groupId);
    }

    public async Task<IEnumerable<CustomerGroup>> GetAllGroupsAsync()
    {
        return await _context.CustomerGroups
            .Include(g => g.GroupLeader)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task AddMemberToGroupAsync(string groupId, string customerId, double ratio)
    {
        var group = await _context.CustomerGroups.FindAsync(groupId);
        if (group == null) throw new Exception("Group not found.");

        if (group.IsActive)
            throw new Exception("Cannot add members to an active group.");

        var customerInActiveGroup = await _customerService.IsCustomerInActiveGroupAsync(customerId);
        if (customerInActiveGroup)
            throw new Exception("Customer is already part of an active group.");

        var existingMember = await _context.GroupMembers.AnyAsync(gm => gm.GroupId == groupId && gm.CustomerId == customerId);
        if (existingMember)
            throw new Exception("Customer is already in this group.");

        var member = new GroupMember
        {
            GroupId = groupId,
            CustomerId = customerId,
            Ratio = ratio
        };

        _context.GroupMembers.Add(member);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberFromGroupAsync(string groupId, string customerId)
    {
        var group = await _context.CustomerGroups.FindAsync(groupId);
        if (group == null) throw new Exception("Group not found.");
        
        if (group.IsActive)
            throw new Exception("Cannot remove members from an active group.");

        var member = await _context.GroupMembers.FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.CustomerId == customerId);
        if (member == null)
            throw new Exception("Customer is not in this group.");

        _context.GroupMembers.Remove(member);
        await _context.SaveChangesAsync();
    }

    public async Task DeactivateGroupAsync(string groupId)
    {
        var group = await _context.CustomerGroups.FindAsync(groupId);
        if (group != null)
        {
            group.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ActivateGroupAsync(string groupId)
    {
        var group = await _context.CustomerGroups.FindAsync(groupId);
        if (group != null)
        {
            group.IsActive = true;
            await _context.SaveChangesAsync();
        }
    }
}
