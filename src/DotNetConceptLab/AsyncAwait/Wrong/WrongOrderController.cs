// ============================================================
// ❌ WRONG APPROACH — Do NOT use any of these in production
// This file exists purely to show what goes wrong and why
// ============================================================

using Microsoft.AspNetCore.Mvc;

namespace DotNetConceptLab.AsyncAwait.Wrong;

// ❌ MISTAKE 1 — Blocking with .Result
// Blocks a thread pool thread. In classic ASP.NET causes a deadlock.
// In ASP.NET Core kills scalability — defeats the entire point of async.
[ApiController]
[Route("api/wrong/orders")]
public class WrongOrderController : ControllerBase
{
    private readonly IWrongOrderService _service;

    public WrongOrderController(IWrongOrderService service)
        => _service = service;

    // ❌ .Result blocks the thread — deadlock risk, zero scalability benefit
    [HttpGet("{id}")]
    public IActionResult GetOrder(int id)
    {
        var order = _service.GetOrderAsync(id).Result; // 💀
        return Ok(order);
    }

    // ❌ .Wait() — same problem, different syntax
    [HttpDelete("{id}")]
    public IActionResult DeleteOrder(int id)
    {
        _service.DeleteOrderAsync(id).Wait(); // 💀
        return NoContent();
    }

    // ❌ Sequential awaits on independent calls — 3x slower than it needs to be
    [HttpGet("dashboard/{userId}")]
    public async Task<IActionResult> GetDashboard(int userId)
    {
        var user    = await _service.GetUserAsync(userId);    // 1s
        var orders  = await _service.GetOrdersAsync(userId);  // 1s
        var balance = await _service.GetBalanceAsync(userId); // 1s
        // Total: ~3 seconds. Should be ~1 second.
        return Ok(new { user, orders, balance });
    }
}

// ❌ MISTAKE 2 — async void in business logic
// Exceptions thrown here are unobservable — they vanish silently
// or crash the process depending on the host
public class WrongOrderService : IWrongOrderService
{
    private static readonly List<Order> _orders = new()
    {
        new Order(1, "Laptop", 1),
        new Order(2, "Monitor", 2),
    };

    // ❌ async void — exception cannot be caught by caller
    public async void ProcessOrder_Wrong(int orderId)
    {
        await Task.Delay(100);
        throw new Exception("This exception disappears into the void"); // 👻
    }

    // ❌ No CancellationToken — cannot be cancelled when client disconnects
    public async Task<Order?> GetOrderAsync(int id)
    {
        await Task.Delay(50); // simulate DB
        return _orders.FirstOrDefault(o => o.Id == id);
    }

    public async Task DeleteOrderAsync(int id)
    {
        await Task.Delay(50);
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order != null) _orders.Remove(order);
    }

    public async Task<string> GetUserAsync(int userId)
    {
        await Task.Delay(1000); // simulate slow call
        return $"User {userId}";
    }

    public async Task<List<Order>> GetOrdersAsync(int userId)
    {
        await Task.Delay(1000);
        return _orders.Where(o => o.UserId == userId).ToList();
    }

    public async Task<decimal> GetBalanceAsync(int userId)
    {
        await Task.Delay(1000);
        return 1500.00m;
    }
}

public interface IWrongOrderService
{
    Task<Order?> GetOrderAsync(int id);
    Task DeleteOrderAsync(int id);
    Task<string> GetUserAsync(int userId);
    Task<List<Order>> GetOrdersAsync(int userId);
    Task<decimal> GetBalanceAsync(int userId);
}

public record Order(int Id, string Product, int UserId);
