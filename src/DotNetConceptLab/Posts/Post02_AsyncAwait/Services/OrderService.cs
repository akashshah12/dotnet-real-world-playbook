using DotNetConceptLab.Posts.Post02.Models;
using DotNetConceptLab.Posts.Post02.Repositories;

namespace DotNetConceptLab.Posts.Post02.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository        _orderRepo;
    private readonly IUserRepository         _userRepo;
    private readonly IWalletRepository       _walletRepo;
    private readonly INotificationRepository _notifRepo;

    public OrderService(
        IOrderRepository        orderRepo,
        IUserRepository         userRepo,
        IWalletRepository       walletRepo,
        INotificationRepository notifRepo)
    {
        _orderRepo  = orderRepo;
        _userRepo   = userRepo;
        _walletRepo = walletRepo;
        _notifRepo  = notifRepo;
    }

    public async Task<Order?> GetOrderAsync(int id, CancellationToken ct = default)
        => await _orderRepo.GetByIdAsync(id, ct);

    public async Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId, CancellationToken ct = default)
        => await _orderRepo.GetByUserIdAsync(userId, ct);

    public async Task<int> CreateOrderAsync(CreateOrderRequest_Post02 request, CancellationToken ct = default)
        => await _orderRepo.CreateAsync(request, ct);

    public async Task DeleteOrderAsync(int id, CancellationToken ct = default)
        => await _orderRepo.DeleteAsync(id, ct);

    // ✅ Task.WhenAll — all 4 fire simultaneously, total time = slowest one
    public async Task<DashboardDto> GetDashboardAsync(int userId, CancellationToken ct = default)
    {
        var profileTask  = _userRepo.GetProfileAsync(userId, ct);
        var ordersTask   = _orderRepo.GetRecentSummariesAsync(userId, ct);
        var balanceTask  = _walletRepo.GetBalanceAsync(userId, ct);
        var notifTask    = _notifRepo.GetUnreadCountAsync(userId, ct);

        await Task.WhenAll(profileTask, ordersTask, balanceTask, notifTask);

        return new DashboardDto(
            Profile:             profileTask.Result,
            RecentOrders:        ordersTask.Result,
            WalletBalance:       balanceTask.Result,
            UnreadNotifications: notifTask.Result
        );
    }
}
