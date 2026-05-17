using Polly;
using Post01Data     = DotNetConceptLab.Posts.Post01.Data;
using Post01Services = DotNetConceptLab.Posts.Post01.Services;
using Post02Repos    = DotNetConceptLab.Posts.Post02.Repositories;
using Post02Services = DotNetConceptLab.Posts.Post02.Services;
using Post03Services = DotNetConceptLab.Posts.Post03_HttpClientFactory.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "Dotnet Real World Playbook",
        Version     = "v1",
        Description = "Demonstrates different concepts of .Net"
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// ── POST 01 — Using AddSingleton with DbContext (Mistake) ────────────────
// ❌ Wrong: Singleton for a stateful, non-thread-safe DbContext
builder.Services.AddSingleton<Post01Data.AppDbContext>();

// ✅ Correct would be:
// builder.Services.AddScoped<Post01Data.AppDbContext>();

builder.Services.AddScoped<Post01Services.IOrderLogService,
                            Post01Services.OrderLogService>();

// ── POST 02 — async/await (Concept + Mistake) ────────────────────────────
builder.Services.AddScoped<Post02Repos.IOrderRepository,        Post02Repos.OrderRepository>();
builder.Services.AddScoped<Post02Repos.IUserRepository,         Post02Repos.UserRepository>();
builder.Services.AddScoped<Post02Repos.IWalletRepository,       Post02Repos.WalletRepository>();
builder.Services.AddScoped<Post02Repos.INotificationRepository, Post02Repos.NotificationRepository>();
builder.Services.AddScoped<Post02Services.IOrderService,        Post02Services.OrderService>();

// ── POST 03 — IHttpClientFactory (Mistake) ───────────────────────────────
// ✅ Typed client — pooled HttpMessageHandler, no socket exhaustion,
//    DNS refreshed every 5 minutes, Polly retry + circuit breaker wired in.
//
// Upstream: https://jsonplaceholder.typicode.com (free public test API)
// Swap "ExternalApis:OrderApi" in appsettings.json to point to your own API.

builder.Services.AddHttpClient<Post03Services.OrderApiClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ExternalApis:OrderApi"]
        ?? "https://jsonplaceholder.typicode.com");

    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
// ✅ Retry: 3 attempts with exponential back-off (2s, 4s, 8s)
.AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        onRetry: (outcome, delay, attempt, _) =>
            Console.WriteLine($"[Post25] Retry {attempt} after {delay.TotalSeconds:F0}s " +
                              $"— {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}")
    )
)
// ✅ Circuit breaker: opens after 5 failures, stays open for 30 seconds
.AddTransientHttpErrorPolicy(policy =>
    policy.CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30),
        onBreak:    (_, duration) => Console.WriteLine($"[Post25] Circuit OPEN for {duration.TotalSeconds:F0}s"),
        onReset:    ()            => Console.WriteLine("[Post25] Circuit CLOSED — upstream healthy"),
        onHalfOpen: ()            => Console.WriteLine("[Post25] Circuit HALF-OPEN — testing upstream")
    )
)
// ✅ Recycle handlers every 5 minutes so DNS changes are picked up
.SetHandlerLifetime(TimeSpan.FromMinutes(5));

builder.Services.AddScoped<Post03Services.IOrderService, Post03Services.OrderService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dotnet Real World Playbook"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
