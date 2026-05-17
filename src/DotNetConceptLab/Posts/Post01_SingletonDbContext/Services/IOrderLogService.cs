namespace DotNetConceptLab.Posts.Post01.Services;

// Renamed from IOrderService → IOrderLogService
// Reason: every post uses "IOrderService" — unique name avoids any ambiguity
// even though namespaces already prevent conflicts

public interface IOrderLogService
{
    void AddLog(string message);
    List<string> GetLogs();
}
