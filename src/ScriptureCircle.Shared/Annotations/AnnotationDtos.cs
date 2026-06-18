using ScriptureCircle.Shared.Scriptures;

namespace ScriptureCircle.Shared.Annotations;

public sealed record AnnotationDto(
    Guid Id,
    Guid UserId,
    string AuthorName,
    ScriptureReferenceDto Reference,
    string HighlightStyle,
    string Visibility,
    string? NotePlainText,
    string? NoteHtml,
    string ShareSlug,
    IReadOnlyList<string> Tags,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    ContentAnchorDto? ContentAnchor = null);

public sealed record ContentAnchorDto(
    string AnchorType,
    int? StartVerse,
    int? EndVerse,
    int? StartOffset,
    int? EndOffset,
    string? ParagraphId);

public sealed record CreateAnnotationRequest(
    ScriptureReferenceDto Reference,
    string HighlightStyle,
    string Visibility,
    string? NotePlainText,
    string? NoteHtml,
    IReadOnlyList<string>? Tags,
    ContentAnchorDto? ContentAnchor = null);

public sealed record UpdateAnnotationRequest(
    string HighlightStyle,
    string Visibility,
    string? NotePlainText,
    string? NoteHtml,
    IReadOnlyList<string>? Tags);
