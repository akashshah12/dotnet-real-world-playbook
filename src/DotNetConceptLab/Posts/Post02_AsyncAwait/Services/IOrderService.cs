using DotNetConceptLab.Posts.Post02.Models;

namespace DotNetConceptLab.Posts.Post02.Services;

public interface IOrderService
{
    Task<Order?> GetOrderAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId, CancellationToken ct = default);
    Task<int> CreateOrderAsync(CreateOrderRequest_Post02 request, CancellationToken ct = default);
    Task DeleteOrderAsync(int id, CancellationToken ct = default);
    Task<DashboardDto> GetDashboardAsync(int userId, CancellationToken ct = default);
}
