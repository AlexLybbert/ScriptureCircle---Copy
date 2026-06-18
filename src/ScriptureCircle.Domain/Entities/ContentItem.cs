namespace ScriptureCircle.Domain.Entities;

public sealed class ContentItem
{
    public string Id { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? SourceId { get; set; }
    public string? VolumeId { get; set; }
    public string? BookId { get; set; }
    public int? ChapterNumber { get; set; }
}
