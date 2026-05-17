// ============================================================
// ❌ WRONG APPROACHES — Learning reference only
// NONE of these classes are registered in DI (Program.cs)
// They exist to show what goes wrong and why
// ============================================================

using System.Net.Http.Json;
using DotNetConceptLab.Posts.Post03_HttpClientFactory.Models;

namespace DotNetConceptLab.Posts.Post03_HttpClientFactory.Wrong;

// ─────────────────────────────────────────────────────────────
// ❌ PATTERN 1 — new HttpClient() per request
//
// Problem: Every request opens a new TCP socket.
// Dispose() releases the HttpClient but the OS holds
// the socket in TIME_WAIT state for ~4 minutes.
//
// Under load:
//   100 req/sec × 4 min TIME_WAIT = 24,000 lingering sockets
//   OS port limit on Linux ≈ 28,000
//   Result → SocketException: Address already in use
// ─────────────────────────────────────────────────────────────
public class WrongNewInstanceService
{
    public async Task<OrderDto?> GetOrderAsync(int orderId)
    {
        // ❌ New socket opened every single call
        using var client = new HttpClient();
        client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");

        var response = await client.GetAsync($"/posts/{orderId}");
        response.EnsureSuccessStatusCode();

        var post = await response.Content.ReadFromJsonAsync<JsonPlaceholderPost>();
        return post is null ? null : new OrderDto(post.Id, post.Title, post.Body, post.UserId, 99.99m, "Active");
    }
}

// ─────────────────────────────────────────────────────────────
// ❌ PATTERN 2 — Static / Singleton HttpClient
//
// Problem: Solves socket exhaustion BUT introduces DNS staleness.
// A static HttpClient caches DNS forever.
// If https://jsonplaceholder.typicode.com changes its IP,
// this client keeps hitting the old IP until the app is restarted.
//
// Also: No timeout configured = requests can hang indefinitely.
// ─────────────────────────────────────────────────────────────
public class WrongStaticClientService
{
    // ❌ DNS cached forever — no handler recycling
    private static readonly HttpClient _client = new()
    {
        BaseAddress = new Uri("https://jsonplaceholder.typicode.com")
        // No Timeout set = defaults to 100 seconds
    };

    public async Task<OrderDto?> GetOrderAsync(int orderId)
    {
        // Works until upstream changes its IP. Then silently fails.
        var post = await _client.GetFromJsonAsync<JsonPlaceholderPost>($"/posts/{orderId}");
        return post is null ? null : new OrderDto(post.Id, post.Title, post.Body, post.UserId, 99.99m, "Active");
    }
}

// ─────────────────────────────────────────────────────────────
// ❌ PATTERN 3 — HttpClient registered as Singleton in DI
//
// Problem: Same DNS staleness as static.
// The same handler is used forever — DNS changes ignored.
//
// The mistake: registering HttpClient directly, not via IHttpClientFactory.
// builder.Services.AddSingleton<HttpClient>(); ← never do this
// ─────────────────────────────────────────────────────────────
public class WrongSingletonInjectedService
{
    private readonly HttpClient _client;

    // ❌ HttpClient injected as singleton from DI — DNS staleness
    public WrongSingletonInjectedService(HttpClient client)
        => _client = client;

    public async Task<OrderDto?> GetOrderAsync(int orderId)
    {
        var post = await _client.GetFromJsonAsync<JsonPlaceholderPost>(
            $"https://jsonplaceholder.typicode.com/posts/{orderId}");

        return post is null ? null : new OrderDto(post.Id, post.Title, post.Body, post.UserId, 99.99m, "Active");
    }
}

// ─────────────────────────────────────────────────────────────
// ❌ PATTERN 4 — No CancellationToken, no error handling
//
// Problem:
//   - Client disconnects → request keeps running → wasted resources
//   - No try/catch → any network error results in unhandled exception
//   - No timeout → slow upstream hangs the thread indefinitely
// ─────────────────────────────────────────────────────────────
public class WrongNoCancellationService
{
    public async Task<OrderDto?> GetOrderAsync(int orderId)
    {
        using var client = new HttpClient(); // ❌ wrong + no timeout

        // ❌ No CancellationToken — can't be cancelled
        // ❌ No try/catch — network error = unhandled exception
        var post = await client.GetFromJsonAsync<JsonPlaceholderPost>(
            $"https://jsonplaceholder.typicode.com/posts/{orderId}");

        return post is null ? null : new OrderDto(post.Id, post.Title, post.Body, post.UserId, 99.99m, "Active");
    }
}
