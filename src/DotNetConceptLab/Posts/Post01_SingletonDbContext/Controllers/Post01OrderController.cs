using Microsoft.AspNetCore.Mvc;
using DotNetConceptLab.Posts.Post01.Services;

namespace DotNetConceptLab.Posts.Post01.Controllers;

[ApiController]
[Route("api/p01/orders")]
public class Post01OrderController : ControllerBase
{
    private readonly IOrderLogService _orderService;

    // ✅ Inject via interface — not the concrete class
    public Post01OrderController(IOrderLogService orderService)
        => _orderService = orderService;

    [HttpPost]
    public IActionResult AddLog([FromQuery] string message)
    {
        _orderService.AddLog(message);
        return Ok();
    }

    [HttpGet]
    public IActionResult GetLogs()
        => Ok(_orderService.GetLogs());
}
