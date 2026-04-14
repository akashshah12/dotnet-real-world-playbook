namespace DotNetConceptLab.AsyncAwait.Correct.Models;

public record Order(int Id, string Product, int UserId, DateTime CreatedAt);

public record CreateOrderRequest(string Product, int UserId);

public record UserProfile(int Id, string Name, string Email);

public record OrderSummary(int Id, string Product, DateTime CreatedAt);

public record DashboardDto(
    UserProfile Profile,
    IEnumerable<OrderSummary> RecentOrders,
    decimal WalletBalance,
    int UnreadNotifications
);
