using Microsoft.AspNetCore.Mvc;
using DotNetConceptLab.AsyncAwait.Correct.Models;
using DotNetConceptLab.AsyncAwait.Correct.Services;

namespace DotNetConceptLab.AsyncAwait.Correct.Controllers;

/// <summary>
/// ✅ CORRECT async/await usage
/// - Every action is async Task<IActionResult>
/// - CancellationToken injected from HttpContext.RequestAborted automatically
/// - No .Result, .Wait(), or async void anywhere
/// - All methods suffixed with Async
/// </summary>
[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly DotNetConceptLab.AsyncAwait.Correct.Services.IOrderService _orderService;

    public OrderController(DotNetConceptLab.AsyncAwait.Correct.Services.IOrderService orderService)
        => _orderService = orderService;

    // ✅ Thread released during DB call — scales to hundreds of concurrent requests
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrderAsync(int id, CancellationToken ct)
    {
        var order = await _orderService.GetOrderAsync(id, ct);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetOrdersByUserAsync(int userId, CancellationToken ct)
    {
        var orders = await _orderService.GetOrdersByUserAsync(userId, ct);
        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrderAsync(
        [FromBody] CreateOrderRequest request,
        CancellationToken ct)
    {
        var orderId = await _orderService.CreateOrderAsync(request, ct);
        return CreatedAtAction(nameof(GetOrderAsync), new { id = orderId }, new { id = orderId });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteOrderAsync(int id, CancellationToken ct)
    {
        await _orderService.DeleteOrderAsync(id, ct);
        return NoContent();
    }

    // ✅ Dashboard: 4 independent queries run in parallel via Task.WhenAll in service
    // ~100ms total instead of ~400ms sequential
    [HttpGet("dashboard/{userId:int}")]
    public async Task<IActionResult> GetDashboardAsync(int userId, CancellationToken ct)
    {
        var dashboard = await _orderService.GetDashboardAsync(userId, ct);
        return Ok(dashboard);
    }
}
