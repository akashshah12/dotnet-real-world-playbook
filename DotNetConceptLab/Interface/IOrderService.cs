using DotNetConceptLab.Models;

public interface IOrderService
{
    void AddLog(string message);
    List<string> GetLogs();

}