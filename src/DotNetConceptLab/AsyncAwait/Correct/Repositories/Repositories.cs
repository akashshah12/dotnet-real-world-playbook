using DotNetConceptLab.AsyncAwait.Correct.Models;

namespace DotNetConceptLab.AsyncAwait.Correct.Repositories;

// ── Interfaces ──────────────────────────────────────────────────────────────

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task<IEnumerable<OrderSummary>> GetRecentSummariesAsync(int userId, CancellationToken ct = default);
    Task<int> CreateAsync(CreateOrderRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IUserRepository
{
    Task<UserProfile> GetProfileAsync(int userId, CancellationToken ct = default);
}

public interface IWalletRepository
{
    Task<decimal> GetBalanceAsync(int userId, CancellationToken ct = default);
}

public interface INotificationRepository
{
    Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default);
}

// ── In-memory implementations (swap for EF Core in production) ───────────────

public class OrderRepository : IOrderRepository
{
    private static readonly List<Order> _orders = new()
    {
        new Order(1, "Laptop",   1, DateTime.UtcNow.AddDays(-10)),
        new Order(2, "Monitor",  1, DateTime.UtcNow.AddDays(-5)),
        new Order(3, "Keyboard", 2, DateTime.UtcNow.AddDays(-2)),
    };
    private static int _nextId = 4;

    public async Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await Task.Delay(50, ct); // simulate DB latency
        return _orders.FirstOrDefault(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        await Task.Delay(50, ct);
        return _orders.Where(o => o.UserId == userId);
    }

    public async Task<IEnumerable<OrderSummary>> GetRecentSummariesAsync(int userId, CancellationToken ct = default)
    {
        await Task.Delay(80, ct);
        return _orders
            .Where(o => o.UserId == userId)
            .Select(o => new OrderSummary(o.Id, o.Product, o.CreatedAt));
    }

    public async Task<int> CreateAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        await Task.Delay(50, ct);
        var order = new Order(_nextId++, request.Product, request.UserId, DateTime.UtcNow);
        _orders.Add(order);
        return order.Id;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await Task.Delay(50, ct);
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order is not null) _orders.Remove(order);
    }
}

public class UserRepository : IUserRepository
{
    public async Task<UserProfile> GetProfileAsync(int userId, CancellationToken ct = default)
    {
        await Task.Delay(60, ct);
        return new UserProfile(userId, $"User {userId}", $"user{userId}@example.com");
    }
}

public class WalletRepository : IWalletRepository
{
    public async Task<decimal> GetBalanceAsync(int userId, CancellationToken ct = default)
    {
        await Task.Delay(70, ct);
        return 1500.00m;
    }
}

public class NotificationRepository : INotificationRepository
{
    public async Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default)
    {
        await Task.Delay(40, ct);
        return 3;
    }
}
