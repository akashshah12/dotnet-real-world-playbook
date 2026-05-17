namespace DotNetConceptLab.Posts.Post01.Data;

// ❌ This is the BAD example from Post #1 — registered as Singleton in Program.cs
// DbContext is stateful and NOT thread-safe — Singleton causes race conditions
public class AppDbContext
{
    public List<string> Logs { get; set; } = new();
}
