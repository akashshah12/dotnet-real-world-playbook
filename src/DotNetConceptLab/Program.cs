using DotNetConceptLab.AsyncAwait.Correct.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Register services
builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddScoped<IOrderRepository,       OrderRepository>();
builder.Services.AddScoped<IUserRepository,        UserRepository>();
builder.Services.AddScoped<IWalletRepository,      WalletRepository>();
builder.Services.AddScoped<INotificationRepository,NotificationRepository>();
builder.Services.AddScoped<DotNetConceptLab.AsyncAwait.Correct.Services.IOrderService, DotNetConceptLab.AsyncAwait.Correct.Services.OrderService>();

/* POST 1 – DI LIFETIME EXAMPLE */
//builder.Services.AddSingleton<AppDbContext>(); // ❌ Problem
builder.Services.AddSingleton<AppDbContext>(); // ❌ Problem

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
