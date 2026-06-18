using ScriptureCircle.Domain.Enums;

namespace ScriptureCircle.Domain.Entities;

public sealed class Notebook
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public NotebookType Type { get; set; } = NotebookType.Personal;
    public bool IsPublic { get; set; }
    public string ShareSlug { get; set; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<NotebookAnnotation> Annotations { get; set; } = new List<NotebookAnnotation>();
}
