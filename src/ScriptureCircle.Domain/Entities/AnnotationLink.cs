namespace ScriptureCircle.Domain.Entities;

public sealed class AnnotationLink
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AnnotationId { get; set; }
    public Annotation Annotation { get; set; } = null!;
    public ScriptureReference Reference { get; set; } = new();
    public string Label { get; set; } = string.Empty;
}
