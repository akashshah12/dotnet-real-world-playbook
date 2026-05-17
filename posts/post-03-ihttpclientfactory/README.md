# IHttpClientFactory — Why `new HttpClient()` is a Production Bug 🚨

> **Engineering Real World Playbook — Post #3**
> ASP.NET Core Internals | Type: Mistake

---

## 🧠 Overview

`HttpClient` is one of the most misused classes in .NET.

The instinct is natural — you need to make an HTTP call, so you create an `HttpClient`, use it, dispose it. Clean and simple.

Except it is not clean. Under production load it silently exhausts your server's ports and brings your API down — with no meaningful error message to explain why.

`IHttpClientFactory` was introduced in .NET Core 2.1 to fix this. Yet `new HttpClient()` still appears in production codebases today.

---

## 🚨 Problem

```csharp
public async Task<string> GetOrderAsync(int id)
{
    using var client = new HttpClient(); // ❌ new socket every request
    return await client.GetStringAsync($"https://api.example.com/orders/{id}");
}
```

This looks correct. It even disposes properly. The problem is what `Dispose()` actually does to the underlying socket.

---

## 🤔 Misconception

> *"`using var client = new HttpClient()` is safe — I'm disposing it properly."*

**Disposing `HttpClient` does NOT immediately close the socket.**

The TCP connection enters **TIME_WAIT** state — managed by the OS, not by .NET. The OS holds that port open for up to **4 minutes** to handle any delayed packets.

```
Request 1 → opens socket :52341 → disposed → TIME_WAIT 4 minutes
Request 2 → opens socket :52342 → disposed → TIME_WAIT 4 minutes
Request 3 → opens socket :52343 → disposed → TIME_WAIT 4 minutes
...
100 req/sec × 240 sec = 24,000 lingering sockets
OS limit ≈ 28,000 ephemeral ports
→ SocketException: Address already in use. API goes down.
```

**The second trap — static `HttpClient`:**

Developers who know about socket exhaustion often "fix" it with a static singleton. This avoids exhaustion but introduces **DNS staleness**. A static client caches DNS forever. If your upstream changes its IP, your static client keeps hitting the dead address until the app restarts.

---

## ❌ Bad Example (C#)

### Pattern 1 — `new HttpClient()` per request — socket exhaustion

```csharp
// ❌ Wrong/WrongApproaches.cs
public class WrongNewInstanceService
{
    public async Task<OrderDto?> GetOrderAsync(int orderId)
    {
        // ❌ Every call opens a new socket
        // Under load: OS runs out of ports → SocketException
        using var client = new HttpClient();
        client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");

        var response = await client.GetAsync($"/posts/{orderId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrderDto>();
    }
}
```

### Pattern 2 — Static `HttpClient` — DNS staleness

```csharp
// ❌ Solves socket exhaustion but caches DNS forever
// If upstream IP changes — this client never picks it up
private static readonly HttpClient _client = new()
{
    BaseAddress = new Uri("https://jsonplaceholder.typicode.com")
    // No Timeout = defaults to 100 seconds
};
```

### Pattern 3 — No `CancellationToken`, no timeout

```csharp
// ❌ Client disconnects → request keeps running → wasted resources
// ❌ Slow upstream → thread hangs indefinitely
public async Task<OrderDto?> GetOrderAsync(int orderId)
{
    using var client = new HttpClient(); // infinite timeout
    return await client.GetFromJsonAsync<OrderDto>(
        $"https://api.example.com/orders/{orderId}");
    // No CancellationToken. No try/catch. No timeout.
}
```

---

## ✅ Correct Example (C#)

### Typed client registration

```csharp
// Program.cs — One registration, handles everything
builder.Services.AddHttpClient<OrderApiClient>(client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(3,
        attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)))
)
.SetHandlerLifetime(TimeSpan.FromMinutes(5)); // DNS refreshed every 5 min
```

### Typed client implementation

```csharp
// ✅ Services/OrderApiClient.cs
public class OrderApiClient
{
    private readonly HttpClient _client;

    // HttpClient injected by IHttpClientFactory — backed by pooled handler
    public OrderApiClient(HttpClient client) => _client = client;

    public async Task<OrderDto?> GetOrderAsync(int orderId, CancellationToken ct = default)
    {
        var response = await _client.GetAsync($"/posts/{orderId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();

        var post = await response.Content
            .ReadFromJsonAsync<JsonPlaceholderPost>(cancellationToken: ct);

        return post is null ? null : MapToOrderDto(post);
    }
}
```

### How the factory actually works

```
IHttpClientFactory
    └── HttpMessageHandler pool
            ├── Handler #1 → active (shared across requests)
            ├── Handler #2 → active
            └── Handler #3 → recycling (new one being created, DNS refreshed)

500 requests → same 3-5 pooled handlers reused → ZERO new sockets
```

---

## 🔍 Explanation

### Why `IHttpClientFactory` solves both problems

It manages a pool of **`HttpMessageHandler`** instances — not `HttpClient` instances.

- Creating `HttpClient` via factory is cheap — they all share pooled handlers
- **Socket exhaustion solved:** handlers reused, no new socket per request
- **DNS staleness solved:** handlers recycled every 2 minutes by default — latest DNS on each recycle
- **Thread safe:** multiple requests share handlers concurrently

### Named vs Typed client

| | Named Client | Typed Client |
|---|---|---|
| Access via | `_factory.CreateClient("name")` | Constructor injection |
| Encapsulation | Configuration in `Program.cs` | Configuration + logic in one class |
| Best for | Multiple different APIs | One dedicated class per upstream |
| Testability | Harder to mock | Easy — mock the typed class |
| **Use when** | Quick/ad-hoc calls | ✅ Recommended for real integrations |

---

## ⚙️ When to Use / Best Practices

| Rule | Reason |
|---|---|
| ✅ Always use `IHttpClientFactory` | Pooled handlers — no socket exhaustion |
| ✅ Prefer typed clients | Clean encapsulation, one class per upstream |
| ✅ Always set `Timeout` | Default 100s is too long — use 10–30s |
| ✅ Pass `CancellationToken` everywhere | Cancel downstream when client disconnects |
| ✅ Add Polly retry policy | Transient failures are normal at scale |
| ✅ Add circuit breaker | Prevent hammering a failing upstream |
| ✅ Set `HandlerLifetime` | Default 2 min — adjust to your DNS TTL |
| ❌ Never `new HttpClient()` | Socket exhaustion under load |
| ❌ Never static `HttpClient` | DNS staleness — fails silently after IP change |
| ❌ Never `builder.Services.AddSingleton<HttpClient>()` | Same DNS staleness problem |

---

## ⚠️ Real-World Impact

**Socket exhaustion:** API making 10 external calls per request, 50 concurrent users = 500 `HttpClient` instances/second. Each holds a socket 4 minutes. Within 2 minutes: 120,000 lingering sockets vs 28,000 OS limit. Server starts rejecting connections. API appears down. Root cause: one constructor call.

**DNS staleness:** Payment service uses static `HttpClient`. Payment provider migrates infrastructure, new IP deployed. Old IP decommissioned. Static client hits dead IP. Every payment fails. Support calls start. Engineers restart the app. Fixed — until the next IP change.

**No timeout:** Upstream API is slow due to a DB issue. Every request to your API hangs for 100 seconds waiting for the response. Thread pool exhausts. Your API stops responding. One slow upstream kills your entire service.

---

## 📚 Key Takeaway

> `new HttpClient()` is one of the most common production bugs in .NET.
> It looks right. Tests pass. Silently kills your server under load.

`IHttpClientFactory` costs you **one line in `Program.cs`** and constructor injection in your service. That is the entire migration.

**There is no valid reason to use `new HttpClient()` in production ASP.NET Core code.**

---

## 📁 Folder Structure

```
src/DotNetConceptLab/Posts/Post03_HttpClientFactory/
│
├── Controllers/
│   └── Post02OrderController.cs      ← Route: api/p03/orders
│                                        Injects: IOrderService
│                                        No HttpClient knowledge here
│
├── Models/
│   └── Models.cs                     ← OrderDto
│                                        CreateOrderRequest
│                                        JsonPlaceholderPost  (upstream response model)
│                                        JsonPlaceholderCreateRequest
│
├── Services/
│   ├── IOrderService.cs              ← Contract: GetOrderAsync, GetOrdersByUserAsync,
│   │                                    CreateOrderAsync
│   ├── OrderApiClient.cs             ← Typed HttpClient — all HTTP logic lives here
│   │                                    Maps upstream JSON → our OrderDto
│   └── OrderService.cs               ← Business logic — delegates HTTP to OrderApiClient
│
└── Wrong/                            ← ⚠️ Code examples only — NOT registered in DI
    └── WrongApproaches.cs            ← Pattern 1: new per request (socket exhaustion)
                                         Pattern 2: static singleton (DNS staleness)
                                         Pattern 3: DI singleton (DNS staleness)
                                         Pattern 4: no timeout/cancellation
```

### Namespace

```
DotNetConceptLab.Posts.Post03
```

### DI Registration — Program.cs

```csharp
using Post03Services = DotNetConceptLab.Posts.Post03.Services;

// ── POST 03 — IHttpClientFactory ────────────────────────────────────────────
builder.Services.AddHttpClient<Post03Services.OrderApiClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ExternalApis:OrderApi"]
        ?? "https://jsonplaceholder.typicode.com");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(3,
        attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)))
)
.SetHandlerLifetime(TimeSpan.FromMinutes(5));

builder.Services.AddScoped<Post03Services.IOrderService,
                            Post03Services.OrderService>();
```

### Configuration — appsettings.json

```json
{
  "ExternalApis": {
    "OrderApi": "https://jsonplaceholder.typicode.com"
  }
}
```

---

## 🌐 API Calls

Base URL: `https://localhost:<port>`

**Upstream used:** `https://jsonplaceholder.typicode.com` — a free public test API.
No setup required. All endpoints work out of the box.

---

### Get order by ID

```http
GET /api/p035/orders/1
```

```bash
curl -X GET "http://localhost:5088/api/p03/orders/1" \
  -H "Accept: application/json"
```

**Response `200 OK`:**
```json
{
  "id": 1,
  "product": "sunt aut facere repellat provident...",
  "description": "quia et suscipit\nsuscipit recusandae...",
  "userId": 1,
  "amount": 49.99,
  "status": "Active"
}
```

**Response `404 Not Found`** (if id > 100):
```json
{ "message": "Order 999 not found." }
```

---

### Get all orders for a user

```http
GET /api/p03/orders/user/1
```

```bash
curl -X GET "http://localhost:5088/api/p03/orders/user/1" \
  -H "Accept: application/json"
```

**Response `200 OK`** (returns 10 orders for userId=1, sorted by amount desc):
```json
[
  { "id": 10, "product": "optio molestias id quia eum", "userId": 1, "amount": 499.90, "status": "Active" },
  { "id": 9,  "product": "nesciunt iure omnis dolorem",  "userId": 1, "amount": 449.91, "status": "Active" },
  ...
]
```

---

### Create an order

```http
POST /api/p03/orders
Content-Type: application/json
```

```bash
curl -X POST "http://localhost:5088/api/p03/orders" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{
    "product": "MacBook Pro",
    "description": "16-inch M3 Pro chip",
    "userId": 1,
    "amount": 2499.00
  }'
```

**Response `201 Created`:**
```json
{
  "id": 101,
  "product": "MacBook Pro",
  "description": "16-inch M3 Pro chip",
  "userId": 1,
  "amount": 2499.00,
  "status": "Active"
}
```

> JSONPlaceholder always returns `id: 101` for POST requests — this is expected behaviour from the test API. In a real project your upstream would return the actual generated ID.

---

### Observe Polly retry in action

Start the app and watch the console. If `jsonplaceholder.typicode.com` is slow or unreachable:

```
[Post03] Retry 1 after 2s — No connection could be made
[Post03] Retry 2 after 4s — No connection could be made
[Post03] Retry 3 after 8s — No connection could be made
[Post03] Circuit OPEN for 30s
```

After 5 failures within a window:
```
[Post03] Circuit OPEN for 30s
# All requests immediately return 503 — upstream not hammered
[Post03] Circuit HALF-OPEN — testing upstream
[Post03] Circuit CLOSED — upstream healthy
```

---

## 🔗 Further Reading

- [IHttpClientFactory in ASP.NET Core — Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory)
- [HttpClient guidelines — Microsoft](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [Polly — .NET resilience library](https://github.com/App-vNext/Polly)
- [You're using HttpClient wrong — Josef Ottosson](https://josef.codes/you-are-probably-still-using-httpclient-wrong)
