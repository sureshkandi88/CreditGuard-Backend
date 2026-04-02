using System.Threading.Tasks;
using CreditGuard.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;

namespace CreditGuard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly ICollectionService _collectionService;
    private readonly IGroupService _groupService;

    public DashboardController(IWalletService walletService, ICollectionService collectionService, IGroupService groupService)
    {
        _walletService = walletService;
        _collectionService = collectionService;
        _groupService = groupService;
    }

    private string CreditorId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<IActionResult> GetDashboardStats()
    {
        var balance = await _walletService.GetBalanceAsync(CreditorId);
        var (collected, expected) = await _collectionService.GetDashboardCollectionStatsAsync();
        
        var groups = await _groupService.GetAllGroupsAsync();
        var activeGroupsCount = groups.Count(g => g.IsActive);

        return Ok(new
        {
            WalletBalance = balance,
            TodayTotalCollected = collected,
            TodayExpectedCollection = expected,
            ActiveGroupsCount = activeGroupsCount
        });
    }
}
