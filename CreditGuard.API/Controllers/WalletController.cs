using System;
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
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    private string CreditorId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("User not found");

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var balance = await _walletService.GetBalanceAsync(CreditorId);
        return Ok(new { Balance = balance });
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddMoney([FromBody] AddMoneyRequest request)
    {
        await _walletService.AddMoneyAsync(CreditorId, request.Amount, request.Reason);
        return Ok(new { Message = "Money added successfully." });
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        var txs = await _walletService.GetTransactionsAsync(CreditorId);
        return Ok(txs.Select(t => new { t.Id, t.Amount, t.Type, t.Reason, t.CreatedAt }));
    }
}
