using DotNetConceptLab.Posts.Post03_HttpClientFactory.Models;

namespace DotNetConceptLab.Posts.Post03_HttpClientFactory.Services;

/// <summary>
/// ✅ Service layer — business logic lives here.
/// HTTP transport is fully encapsulated in OrderApiClient.
/// Controller → OrderService (business logic) → OrderApiClient (HTTP) → Upstream API
/// </summary>
public class OrderService : IOrderService
{
    private readonly OrderApiClient _apiClient;

    public OrderService(OrderApiClient apiClient)
        => _apiClient = apiClient;

    public async Task<OrderDto?> GetOrderAsync(int orderId, CancellationToken ct = default)
    {
        if (orderId <= 0)
            throw new ArgumentException("Order ID must be greater than zero.", nameof(orderId));

        return await _apiClient.GetOrderAsync(orderId, ct);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(
        int userId,
        CancellationToken ct = default)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

        var orders = await _apiClient.GetOrdersByUserAsync(userId, ct);

        // Business logic: sort by amount descending
        return orders.OrderByDescending(o => o.Amount);
    }

    public async Task<OrderDto?> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken ct = default)
    {
        return await _apiClient.CreateOrderAsync(request, ct);
    }
}
