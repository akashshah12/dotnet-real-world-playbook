# Using AddSingleton with DbContext? That's a Production Bug 🚨

> **Engineering Real World Playbook — Post #1**
> ASP.NET Core Internals | Type: Mistake

---

## 🧠 Overview

In ASP.NET Core, every service you register has a **lifetime** — Singleton, Scoped, or Transient.

Choosing the wrong one for `DbContext` is one of the most silent, dangerous mistakes in .NET development. It won't fail in development. It will fail in production, under load, and at the worst possible time.

---

## 🚨 Problem

```csharp
builder.Services.AddSingleton<AppDbContext>();
```

This looks harmless. The app starts. Tests pass. Then you go live.

Under concurrent requests, multiple threads share a single `DbContext` instance. The result:

- Race conditions on the internal state
- Queries returning wrong data
- Data from User A leaking to User B
- Exceptions that make no sense in isolation

---

## 🤔 Misconception

> *"Singleton means better performance — one object, less overhead."*

**Wrong.** `DbContext` holds internal state — entity tracking, connection state, identity map. It is explicitly **not thread-safe**. Using it as Singleton trades a tiny allocation cost for unpredictable data corruption at scale.

---

## ❌ Bad Example (C#)

```csharp
// ❌ Program.cs — Wrong lifetime
builder.Services.AddSingleton<AppDbContext>(); // One instance for ENTIRE app

// ❌ Controller injects concrete class — not the interface
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService; // ❌ concrete, not IOrderLogService

    public OrderController(OrderService orderService)
        => _orderService = orderService;
}
```

**What happens:**

- Request 1 → writes to `_context.Logs`
- Request 2 → reads from the same `_context.Logs` — sees Request 1's data
- Request 3 → writes while Request 2 is reading → exception or corruption

---

## ✅ Correct Example (C#)

```csharp
// ✅ Program.cs — Correct lifetime
builder.Services.AddScoped<AppDbContext>(); // One instance per HTTP request

// ✅ Controller injects via interface
public class Post01OrderController : ControllerBase
{
    private readonly IOrderLogService _orderService; // ✅ interface

    public Post01OrderController(IOrderLogService orderService)
        => _orderService = orderService;
}
```

**What happens:**

- Request 1 → gets its own `AppDbContext` instance
- Request 2 → gets its own `AppDbContext` instance
- No shared state. No race condition. No data leak.

---

## 🔍 Explanation

### DI Lifetimes — Quick Reference

| Lifetime | Instance created | Disposed | Use for |
|---|---|---|---|
| `Singleton` | Once at startup | App shutdown | Stateless, thread-safe services (config, cache) |
| `Scoped` | Once per HTTP request | End of request | `DbContext`, unit of work, request-scoped services |
| `Transient` | Every time it is injected | When scope ends | Lightweight, stateless utilities |

### Why Singleton breaks DbContext

`DbContext` is designed around a **unit of work** pattern — it tracks changes for the duration of one operation, then gets disposed. Keeping it alive forever:

- Accumulates stale tracked entities in memory
- Causes `ObjectDisposedException` when EF Core tries to use a closed connection
- Under concurrent requests: multiple threads write to the same change tracker simultaneously → data corruption or exceptions

---

## ⚙️ When to Use / Best Practices

| Scenario | Correct lifetime |
|---|---|
| `DbContext` / EF Core | ✅ `AddScoped` |
| Repository that uses `DbContext` | ✅ `AddScoped` |
| Business service that uses a repository | ✅ `AddScoped` |
| Stateless utility (string formatter, calculator) | ✅ `AddTransient` |
| Configuration wrapper, in-memory cache, `HttpClient` factory | ✅ `AddSingleton` |
| `IHttpClientFactory` | ✅ `AddSingleton` (framework handles this) |

> **Never** register a service as `Singleton` if it depends on a `Scoped` service.
> ASP.NET Core will throw at startup: *"Cannot consume scoped service from singleton."*

---

## ⚠️ Real-World Impact

A shared `DbContext` in production with 50 concurrent users:

- Request A adds an order → tracked in the shared context
- Request B reads all orders → sees Request A's uncommitted change
- Request C calls `SaveChanges()` → saves both its own data AND Request A's partial data

The result is data corruption that is nearly impossible to reproduce in testing — because tests are almost never concurrent.

---

## 📚 Key Takeaway

> `DbContext` must always be `AddScoped` — one instance per HTTP request.
> `Singleton` is for stateless, thread-safe services. `DbContext` is neither.

---

## 📁 Folder Structure

```
src/DotNetConceptLab/Posts/Post01_SingletonDbContext/
│
├── Controllers/
│   └── Post01OrderController.cs     ← Route: api/p01/orders
│                                       Injects: IOrderLogService (interface)
│
├── Data/
│   └── AppDbContext.cs              ← Simulated DbContext (in-memory Logs list)
│                                       Registered as Singleton ❌ to demonstrate the bug
│
└── Services/
    ├── IOrderLogService.cs          ← Interface (not IOrderService — avoids confusion
    │                                   with other posts using the same name)
    └── OrderLogService.cs           ← Implementation — depends on AppDbContext
```

### Namespace

```
DotNetConceptLab.Posts.Post01
```

### DI Registration (Program.cs)

```csharp
// ── POST 01 — Using AddSingleton with DbContext (Mistake) ────────────────
// ❌ Wrong: Singleton for a stateful, non-thread-safe DbContext
builder.Services.AddSingleton<Post01Data.AppDbContext>();

// ✅ Correct would be:
// builder.Services.AddScoped<Post01Data.AppDbContext>();

builder.Services.AddScoped<Post01Services.IOrderLogService,
                            Post01Services.OrderLogService>();
```

---

## 🌐 API Calls

Base URL: `https://localhost:<port>`

### Add a log entry

```

### Get all logs

```http
GET /api/p01/orders
```

```bash
curl "https://localhost:7001/api/p01/orders"
```

Response:
```json
["HelloWorld", "AnotherMessage"]
```

> **Try the bug:** Send two POST requests simultaneously and notice both messages appear in GET — they are sharing the same `AppDbContext` instance because it is registered as Singleton. Switch to `AddScoped` in Program.cs to fix it.

---

## 🔗 Further Reading

- [Service lifetimes — Microsoft Docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection#service-lifetimes)
- [DbContext lifetime — EF Core Docs](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#the-dbcontext-lifetime-in-aspnet-core)
