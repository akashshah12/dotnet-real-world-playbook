using Microsoft.AspNetCore.Mvc;
using DotNetConceptLab.Posts.Post03_HttpClientFactory.Models;
using DotNetConceptLab.Posts.Post03_HttpClientFactory.Services;

namespace DotNetConceptLab.Posts.Post03_HttpClientFactory.Controllers;

/// <summary>
/// ✅ Controller for Post #03 — IHttpClientFactory demo
///
/// Route prefix: api/p03
/// This controller demonstrates the CORRECT way to consume an external HTTP API.
/// The actual HTTP call lives in OrderApiClient (typed client).
/// This controller has zero knowledge of HttpClient — it only knows IOrderService.
/// </summary>
[ApiController]
[Route("api/p03/orders")]
[Produces("application/json")]
public class Post03OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public Post03OrderController(IOrderService orderService)
        => _orderService = orderService;

    /// <summary>
    /// Get a single order from the upstream API by ID.
    /// Calls: GET https://jsonplaceholder.typicode.com/posts/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderAsync(int id, CancellationToken ct)
    {
        var order = await _orderService.GetOrderAsync(id, ct);
        return order is null ? NotFound(new { message = $"Order {id} not found." }) : Ok(order);
    }

    /// <summary>
    /// Get all orders for a user from the upstream API.
    /// Calls: GET https://jsonplaceholder.typicode.com/posts?userId={userId}
    /// </summary>
    [HttpGet("user/{userId:int}")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersByUserAsync(int userId, CancellationToken ct)
    {
        var orders = await _orderService.GetOrdersByUserAsync(userId, ct);
        return Ok(orders);
    }

    /// <summary>
    /// Create a new order via the upstream API.
    /// Calls: POST https://jsonplaceholder.typicode.com/posts
    /// Note: JSONPlaceholder simulates creation — returns id: 101 for all POST requests.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> CreateOrderAsync(
        [FromBody] CreateOrderRequest request,
        CancellationToken ct)
    {
        var created = await _orderService.CreateOrderAsync(request, ct);

        if (created is null)
            return StatusCode(StatusCodes.Status502BadGateway,
                new { message = "Upstream API did not return the created order." });

        return Ok(created);
    }
}
