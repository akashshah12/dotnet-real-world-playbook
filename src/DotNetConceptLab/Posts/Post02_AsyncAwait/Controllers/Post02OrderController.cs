using Microsoft.AspNetCore.Mvc;
using DotNetConceptLab.Posts.Post02.Models;
using DotNetConceptLab.Posts.Post02.Services;

namespace DotNetConceptLab.Posts.Post02.Controllers;

[ApiController]
[Route("api/p02/orders")]
public class Post02OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public Post02OrderController(IOrderService orderService)
        => _orderService = orderService;

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
        [FromBody] CreateOrderRequest_Post02 request,
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

    // ✅ Dashboard: all 4 queries parallel via Task.WhenAll inside service
    [HttpGet("dashboard/{userId:int}")]
    public async Task<IActionResult> GetDashboardAsync(int userId, CancellationToken ct)
    {
        var dashboard = await _orderService.GetDashboardAsync(userId, ct);
        return Ok(dashboard);
    }
}
