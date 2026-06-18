using ScriptureCircle.Domain.Enums;

namespace ScriptureCircle.Domain.Entities;

public sealed class Annotation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public ScriptureReference Reference { get; set; } = new();
    public ContentAnchor ContentAnchor { get; set; } = new();
    public HighlightStyle HighlightStyle { get; set; } = HighlightStyle.Yellow;
    public string? NotePlainText { get; set; }
    public string? NoteHtml { get; set; }
    public AnnotationVisibility Visibility { get; set; } = AnnotationVisibility.Private;
    public string ShareSlug { get; set; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<AnnotationTag> Tags { get; set; } = new List<AnnotationTag>();
    public ICollection<NotebookAnnotation> Notebooks { get; set; } = new List<NotebookAnnotation>();
    public ICollection<AnnotationLink> Links { get; set; } = new List<AnnotationLink>();
}
