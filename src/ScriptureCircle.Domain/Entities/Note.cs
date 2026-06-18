namespace ScriptureCircle.Domain.Entities;

public sealed class Note
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public ScriptureReference Reference { get; set; } = new();
    public string Body { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
