namespace ScriptureCircle.Shared.Scriptures;

public sealed record ScriptureReferenceDto(
    string VolumeId,
    string BookId,
    string BookTitle,
    int Chapter,
    int? VerseStart,
    int? VerseEnd);

public sealed record VerseDto(int Number, string Text, bool IsSelected);

public sealed record ScriptureChapterDto(
    string Id,
    string BookTitle,
    string Delineation,
    int Chapter,
    string Summary,
    string PreviousChapterId,
    string NextChapterId,
    IReadOnlyList<VerseDto> Verses);
