using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CreditGuard.API.DTOs;
using CreditGuard.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditGuard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CreditsController : ControllerBase
{
    private readonly ICreditService _creditService;

    public CreditsController(ICreditService creditService)
    {
        _creditService = creditService;
    }

    private string CreditorId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCreditRequest request)
    {
        var credit = await _creditService.CreateCreditAsync(request.GroupId, request.TotalPrincipal, CreditorId, request.MemberRatios);
        return Ok(credit);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var credit = await _creditService.GetCreditAsync(id);
        if (credit == null) return NotFound();
        return Ok(credit);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var credits = await _creditService.GetActiveCreditsAsync();
        return Ok(credits.Select(c => new
        {
            c.Id, c.TotalPrincipal, c.TotalInterest, c.DailyInstallment, c.StartDate, c.EndDate,
            GroupId = c.GroupId, GroupName = c.Group?.Name
        }));
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> CloseEarly(string id)
    {
        await _creditService.CloseCreditEarlyAsync(id);
        return Ok(new { Message = "Credit closed successfully." });
    }
}
