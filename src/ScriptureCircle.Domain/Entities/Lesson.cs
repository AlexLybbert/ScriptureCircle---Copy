namespace ScriptureCircle.Domain.Entities;

public sealed class Lesson
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CreatedByUserId { get; set; }
    public AppUser? CreatedByUser { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public ScriptureReference Reference { get; set; } = new();
    public bool IsPublic { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
