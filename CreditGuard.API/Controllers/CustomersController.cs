using System.Threading.Tasks;
using CreditGuard.API.DTOs;
using CreditGuard.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditGuard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
    {
        var customer = await _customerService.CreateCustomerAsync(request.Aadhaar, request.FirstName, request.LastName, request.Phone, request.Address, null);
        return Ok(customer);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var customer = await _customerService.GetCustomerAsync(id);
        if (customer == null) return NotFound();
        return Ok(customer);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest("Query is required.");
        
        var customers = await _customerService.SearchCustomersAsync(q);
        return Ok(customers);
    }

    [HttpGet("{id}/eligibility")]
    public async Task<IActionResult> CheckEligibility(string id)
    {
        var inActiveGroup = await _customerService.IsCustomerInActiveGroupAsync(id);
        return Ok(new { Eligible = !inActiveGroup, InActiveGroup = inActiveGroup });
    }
}
