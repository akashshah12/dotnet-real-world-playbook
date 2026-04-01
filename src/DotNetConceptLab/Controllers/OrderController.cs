using Microsoft.AspNetCore.Mvc;

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