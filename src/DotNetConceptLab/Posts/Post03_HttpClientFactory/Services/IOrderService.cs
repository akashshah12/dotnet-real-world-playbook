using DotNetConceptLab.Posts.Post03_HttpClientFactory.Models;

namespace DotNetConceptLab.Posts.Post03_HttpClientFactory.Services;

public interface IOrderService
{
    Task<OrderDto?> GetOrderAsync(int orderId, CancellationToken ct = default);
    Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId, CancellationToken ct = default);
    Task<OrderDto?> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct = default);
}
