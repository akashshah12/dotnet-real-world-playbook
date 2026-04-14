using DotNetConceptLab.AsyncAwait.Correct.Models;
using DotNetConceptLab.AsyncAwait.Correct.Repositories;

namespace DotNetConceptLab.AsyncAwait.Correct.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly IUserRepository _userRepo;
    private readonly IWalletRepository _walletRepo;
    private readonly INotificationRepository _notifRepo;

    public OrderService(
        IOrderRepository orderRepo,
        IUserRepository userRepo,
        IWalletRepository walletRepo,
        INotificationRepository notifRepo)
    {
        _orderRepo  = orderRepo;
        _userRepo   = userRepo;
        _walletRepo = walletRepo;
        _notifRepo  = notifRepo;
    }

    // ✅ Async all the way — CancellationToken threaded through
    public async Task<Order?> GetOrderAsync(int id, CancellationToken ct = default)
        => await _orderRepo.GetByIdAsync(id, ct);

    public async Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId, CancellationToken ct = default)
        => await _orderRepo.GetByUserIdAsync(userId, ct);

    public async Task<int> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct = default)
        => await _orderRepo.CreateAsync(request, ct);

    public async Task DeleteOrderAsync(int id, CancellationToken ct = default)
        => await _orderRepo.DeleteAsync(id, ct);

    // ✅ Task.WhenAll — all 4 queries fire simultaneously
    // Total time = slowest single query (~100ms), NOT 4 × 100ms = 400ms
    public async Task<DashboardDto> GetDashboardAsync(int userId, CancellationToken ct = default)
    {
        var profileTask  = _userRepo.GetProfileAsync(userId, ct);
        var ordersTask   = _orderRepo.GetRecentSummariesAsync(userId, ct);
        var balanceTask  = _walletRepo.GetBalanceAsync(userId, ct);
        var notifTask    = _notifRepo.GetUnreadCountAsync(userId, ct);

        // All 4 run in parallel — not one after another
        await Task.WhenAll(profileTask, ordersTask, balanceTask, notifTask);

        return new DashboardDto(
            Profile:              profileTask.Result,
            RecentOrders:         ordersTask.Result,
            WalletBalance:        balanceTask.Result,
            UnreadNotifications:  notifTask.Result
        );
    }
}
