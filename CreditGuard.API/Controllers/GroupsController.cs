using System;
using System.Linq;
using System.Threading.Tasks;
using CreditGuard.API.DTOs;
using CreditGuard.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditGuard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGroupRequest request)
    {
        var group = await _groupService.CreateGroupAsync(request.Name, request.Location, request.GroupLeaderId, null);
        return Ok(group);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var group = await _groupService.GetGroupAsync(id);
        if (group == null) return NotFound();

        var result = new 
        {
            group.Id,
            group.Name,
            group.Location,
            group.IsActive,
            group.GroupLeaderId,
            Leader = group.GroupLeader != null ? new { group.GroupLeader.FirstName, group.GroupLeader.LastName } : null,
            Members = group.GroupMembers.Select(m => new { m.CustomerId, m.Ratio, m.Customer?.FirstName, m.Customer?.LastName }),
            Credits = group.Credits.Select(c => new { c.Id, c.TotalPrincipal, c.Status, c.StartDate, c.EndDate })
        };

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var groups = await _groupService.GetAllGroupsAsync();
        return Ok(groups.Select(g => new { g.Id, g.Name, g.Location, g.IsActive }));
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMember(string id, [FromBody] AddGroupMemberRequest request, [FromQuery] string customerId)
    {
        if (string.IsNullOrEmpty(customerId)) return BadRequest("customerId query param is required.");
        
        await _groupService.AddMemberToGroupAsync(id, customerId, request.Ratio);
        return Ok(new { Message = "Member added successfully." });
    }

    [HttpDelete("{id}/members/{customerId}")]
    public async Task<IActionResult> RemoveMember(string id, string customerId)
    {
        await _groupService.RemoveMemberFromGroupAsync(id, customerId);
        return Ok(new { Message = "Member removed." });
    }
}
