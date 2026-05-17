# Using AddSingleton with DbContext? That’s a production bug 🚨

## 🧠 Overview

In ASP.NET Core, choosing the correct Dependency Injection lifetime is critical.

`DbContext` is designed to work per request. Registering it with the wrong lifetime can cause unexpected behavior, especially in multi-user environments.

---

## 🚨 Problem

Registering `DbContext` as Singleton:

```csharp
builder.Services.AddSingleton<AppDbContext>();
```

This creates a **single shared instance** across all requests.

---

## ❌ Why This Causes Issues

* `DbContext` is **not thread-safe**
* Multiple requests use the same instance
* Data gets shared across users unintentionally

This leads to:

* Race conditions
* Data inconsistency
* Hard-to-debug issues

---

## 💻 Example from Project

### 📁 Data/AppDbContext.cs

```csharp
public class AppDbContext
{
    public List<string> Logs { get; set; } = new();
}
```

---

### 📁 Services/OrderService.cs

```csharp
public class OrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public void AddLog(string message)
    {
        _context.Logs.Add(message);
    }

    public List<string> GetLogs()
    {
        return _context.Logs;
    }
}
```

---

### 📁 Controllers/OrderController.cs

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public IActionResult Add(string message)
    {
        _orderService.AddLog(message);
        return Ok();
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_orderService.GetLogs());
    }
}
```

---

## 🔍 What Happens Internally

### ❌ With Singleton

* One `AppDbContext` instance for entire app
* All users share same `Logs` list

👉 Result:

* Mixed data between users
* Unexpected results

---

### ✅ With Scoped (Correct)

```csharp
builder.Services.AddScoped<AppDbContext>();
```

* New instance per request
* Data remains isolated

---

## ⚙️ When to Use What

* **Scoped** → DbContext, request-based operations
* **Transient** → lightweight, stateless services
* **Singleton** → caching, configuration

---

## ⚠️ Real Impact

Using Singleton for DbContext can cause:

* Shared data across requests
* Concurrency issues
* Unpredictable behavior in production

---

## 📚 Key Takeaway

👉 DbContext should always be registered as **Scoped**, not Singleton.

---


## 🚀 Final Thoughts

Small configuration choices can have a big impact in real applications.

Understanding service lifetimes helps build stable and predictable systems.
