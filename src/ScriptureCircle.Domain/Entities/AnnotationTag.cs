namespace ScriptureCircle.Domain.Entities;

public sealed class AnnotationTag
{
    public Guid AnnotationId { get; set; }
    public Annotation Annotation { get; set; } = null!;
    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
