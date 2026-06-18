using ScriptureCircle.Domain.Entities;
using ScriptureCircle.Shared.Annotations;

namespace ScriptureCircle.Application.Mapping;

public static class AnnotationMapper
{
    public static AnnotationDto ToDto(Annotation annotation) => new(
        annotation.Id,
        annotation.UserId,
        annotation.User.DisplayName,
        ScriptureReferenceMapper.ToDto(annotation.Reference),
        annotation.HighlightStyle.ToString(),
        annotation.Visibility.ToString(),
        annotation.NotePlainText,
        annotation.NoteHtml,
        annotation.ShareSlug,
        annotation.Tags.Select(t => t.Tag.Name).OrderBy(t => t).ToList(),
        annotation.CreatedAt,
        annotation.UpdatedAt,
        new ContentAnchorDto(
            annotation.ContentAnchor.AnchorType,
            annotation.ContentAnchor.StartVerse,
            annotation.ContentAnchor.EndVerse,
            annotation.ContentAnchor.StartOffset,
            annotation.ContentAnchor.EndOffset,
            annotation.ContentAnchor.ParagraphId));
}
