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
public class CollectionsController : ControllerBase
{
    private readonly ICollectionService _collectionService;

    public CollectionsController(ICollectionService collectionService)
    {
        _collectionService = collectionService;
    }

    private string CreditorId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpPost]
    public async Task<IActionResult> RecordCollection([FromBody] RecordCollectionRequest request)
    {
        var collection = await _collectionService.RecordCollectionAsync(request.CreditId, request.AmountPaid, CreditorId, request.Notes, request.IsAdvancePayment);
        return Ok(new { Message = "Collection recorded successfully.", CollectionId = collection.Id });
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetTodaysCollections()
    {
        var collections = await _collectionService.GetTodaysCollectionsAsync();
        return Ok(collections.Select(c => new
        {
            c.Id, c.CreditId, c.AmountPaid, c.CollectionDate, c.Notes, c.IsAdvancePayment,
            GroupName = c.Credit?.Group?.Name
        }));
    }

    [HttpGet("credit/{creditId}")]
    public async Task<IActionResult> GetByCredit(string creditId)
    {
        var collections = await _collectionService.GetCollectionsByCreditAsync(creditId);
        return Ok(collections.Select(c => new
        {
            c.Id, c.AmountPaid, c.CollectionDate, c.Notes, c.IsAdvancePayment,
            CollectedBy = c.Creditor?.FirstName + " " + c.Creditor?.LastName,
            Allocations = c.PaymentAllocations.Select(pa => new { pa.CustomerId, pa.AmountAllocated })
        }));
    }
}
