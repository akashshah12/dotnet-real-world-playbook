using Microsoft.AspNetCore.Mvc;
using DotNetConceptLab.Models;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var student = _studentService.GetStudent(id);
        return Ok(student);
    }
}