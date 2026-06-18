namespace ScriptureCircle.Domain.Entities;

public sealed class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public ICollection<AnnotationTag> Annotations { get; set; } = new List<AnnotationTag>();
}
