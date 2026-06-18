namespace ScriptureCircle.Domain.Entities;

public sealed class ContentAnchor
{
    public string ContentItemId { get; set; } = string.Empty;
    public string ContentType { get; set; } = "Scripture";
    public string AnchorType { get; set; } = "VerseRange";
    public int? StartVerse { get; set; }
    public int? EndVerse { get; set; }
    public int? StartOffset { get; set; }
    public int? EndOffset { get; set; }
    public string? ParagraphId { get; set; }
}
