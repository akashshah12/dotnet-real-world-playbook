using DotNetConceptLab.Posts.Post02.Models;

namespace DotNetConceptLab.Posts.Post02.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task<IEnumerable<OrderSummary>> GetRecentSummariesAsync(int userId, CancellationToken ct = default);
    Task<int> CreateAsync(CreateOrderRequest_Post02 request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
public interface IUserRepository         { Task<UserProfile> GetProfileAsync(int userId, CancellationToken ct = default); }
public interface IWalletRepository       { Task<decimal> GetBalanceAsync(int userId, CancellationToken ct = default); }
public interface INotificationRepository { Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default); }

public class OrderRepository : IOrderRepository
{
    private static readonly List<Order> _store = new()
    {
        new Order(1, "Laptop",   1, DateTime.UtcNow.AddDays(-10)),
        new Order(2, "Monitor",  1, DateTime.UtcNow.AddDays(-5)),
        new Order(3, "Keyboard", 2, DateTime.UtcNow.AddDays(-2)),
    };
    private static int _nextId = 4;

    public async Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
    { await Task.Delay(50, ct); return _store.FirstOrDefault(o => o.Id == id); }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    { await Task.Delay(50, ct); return _store.Where(o => o.UserId == userId); }

    public async Task<IEnumerable<OrderSummary>> GetRecentSummariesAsync(int userId, CancellationToken ct = default)
    { await Task.Delay(80, ct); return _store.Where(o => o.UserId == userId).Select(o => new OrderSummary(o.Id, o.Product, o.CreatedAt)); }

    public async Task<int> CreateAsync(CreateOrderRequest_Post02 request, CancellationToken ct = default)
    {
        await Task.Delay(50, ct);
        var order = new Order(_nextId++, request.Product, request.UserId, DateTime.UtcNow);
        _store.Add(order);
        return order.Id;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    { await Task.Delay(50, ct); var o = _store.FirstOrDefault(x => x.Id == id); if (o is not null) _store.Remove(o); }
}

public class UserRepository : IUserRepository
{
    public async Task<UserProfile> GetProfileAsync(int userId, CancellationToken ct = default)
    { await Task.Delay(60, ct); return new UserProfile(userId, $"User {userId}", $"user{userId}@example.com"); }
}

public class WalletRepository : IWalletRepository
{
    public async Task<decimal> GetBalanceAsync(int userId, CancellationToken ct = default)
    { await Task.Delay(70, ct); return 1_500.00m; }
}

public class NotificationRepository : INotificationRepository
{
    public async Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default)
    { await Task.Delay(40, ct); return 3; }
}
