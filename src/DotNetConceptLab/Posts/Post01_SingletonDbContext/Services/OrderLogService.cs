using DotNetConceptLab.Posts.Post01.Data;

namespace DotNetConceptLab.Posts.Post01.Services;

public class OrderLogService : IOrderLogService
{
    private readonly AppDbContext _context;

    public OrderLogService(AppDbContext context)
        => _context = context;

    public void AddLog(string message)
        => _context.Logs.Add(message);

    public List<string> GetLogs()
        => _context.Logs;
}
