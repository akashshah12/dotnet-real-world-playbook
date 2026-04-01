using DotNetConceptLab.Models;

public class StudentService : IStudentService
{
    public Student GetStudent(int id)
    {
        return new Student
        {
            Id = id,
            Name = "Akash"
        };
    }
}