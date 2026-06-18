namespace ScriptureCircle.Domain.Entities;

public sealed class NotebookAnnotation
{
    public Guid NotebookId { get; set; }
    public Notebook Notebook { get; set; } = null!;
    public Guid AnnotationId { get; set; }
    public Annotation Annotation { get; set; } = null!;
    public int SortOrder { get; set; }
}
