public class OrderService : IOrderService
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