using DotNetConceptLab.AsyncAwait.Correct.Models;

namespace DotNetConceptLab.AsyncAwait.Correct.Services;

public interface IOrderService
{
    Task<Order?> GetOrderAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId, CancellationToken ct = default);
    Task<int> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct = default);
    Task DeleteOrderAsync(int id, CancellationToken ct = default);
    Task<DashboardDto> GetDashboardAsync(int userId, CancellationToken ct = default);
}
