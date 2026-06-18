using ScriptureCircle.Domain.Entities;
using ScriptureCircle.Shared.Scriptures;

namespace ScriptureCircle.Application.Mapping;

public static class ScriptureReferenceMapper
{
    public static ScriptureReference ToEntity(ScriptureReferenceDto dto) => new()
    {
        VolumeId = dto.VolumeId,
        BookId = dto.BookId,
        BookTitle = dto.BookTitle,
        Chapter = dto.Chapter,
        VerseStart = dto.VerseStart,
        VerseEnd = dto.VerseEnd
    };

    public static ScriptureReferenceDto ToDto(ScriptureReference reference) => new(
        reference.VolumeId,
        reference.BookId,
        reference.BookTitle,
        reference.Chapter,
        reference.VerseStart,
        reference.VerseEnd);
}
