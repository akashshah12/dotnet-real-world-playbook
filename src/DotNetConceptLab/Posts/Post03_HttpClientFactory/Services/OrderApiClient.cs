using System.Net;
using System.Net.Http.Json;
using DotNetConceptLab.Posts.Post03_HttpClientFactory.Models;

namespace DotNetConceptLab.Posts.Post03_HttpClientFactory.Services;

/// <summary>
/// ✅ TYPED CLIENT — the correct way to use HttpClient in ASP.NET Core.
///
/// IHttpClientFactory injects an HttpClient here that is backed by a
/// POOLED HttpMessageHandler. This means:
///   - No socket exhaustion (handlers are reused across requests)
///   - No DNS staleness (handlers are recycled every 2 minutes by default)
///   - Thread safe — safe to inject as Scoped
///
/// We use https://jsonplaceholder.typicode.com as our simulated upstream API.
/// In a real project, replace the base address with your actual upstream.
/// </summary>
public class OrderApiClient
{
    private readonly HttpClient _client;

    // ✅ HttpClient injected by IHttpClientFactory — backed by pooled handler
    public OrderApiClient(HttpClient client) => _client = client;

    public async Task<OrderDto?> GetOrderAsync(int orderId, CancellationToken ct = default)
    {
        var response = await _client.GetAsync($"/posts/{orderId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var post = await response.Content
            .ReadFromJsonAsync<JsonPlaceholderPost>(cancellationToken: ct);

        return post is null ? null : MapToOrderDto(post);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(
        int userId,
        CancellationToken ct = default)
    {
        var response = await _client.GetAsync($"/posts?userId={userId}", ct);
        response.EnsureSuccessStatusCode();

        var posts = await response.Content
            .ReadFromJsonAsync<IEnumerable<JsonPlaceholderPost>>(cancellationToken: ct);

        return posts?.Select(p => MapToOrderDto(p)) ?? Enumerable.Empty<OrderDto>();
    }

    public async Task<OrderDto?> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken ct = default)
    {
        // Map our request to what JSONPlaceholder expects
        var upstreamRequest = new JsonPlaceholderCreateRequest(
            Title:  request.Product,
            Body:   request.Description,
            UserId: request.UserId
        );

        var response = await _client.PostAsJsonAsync("/posts", upstreamRequest, ct);
        response.EnsureSuccessStatusCode();

        var post = await response.Content
            .ReadFromJsonAsync<JsonPlaceholderPost>(cancellationToken: ct);

        return post is null ? null : MapToOrderDto(post, request.Amount);
    }

    // ── Private mapping — upstream model → our domain model ──────────────────
    private static OrderDto MapToOrderDto(JsonPlaceholderPost post, decimal amount = 0)
        => new(
            Id:          post.Id,
            Product:     post.Title.Length > 40
                             ? post.Title[..40] + "..."
                             : post.Title,
            Description: post.Body,
            UserId:      post.UserId,
            Amount:      amount > 0 ? amount : Math.Round(post.Id * 49.99m, 2),
            Status:      "Active"
        );
}
