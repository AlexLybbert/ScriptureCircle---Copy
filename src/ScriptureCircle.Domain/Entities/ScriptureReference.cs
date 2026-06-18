using System.ComponentModel.DataAnnotations.Schema;

namespace ScriptureCircle.Domain.Entities;

public sealed class ScriptureReference
{
    public string VolumeId { get; set; } = string.Empty;
    public string BookId { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public int Chapter { get; set; } = 1;
    [NotMapped]
    public int ChapterNumber
    {
        get => Chapter;
        set => Chapter = value;
    }
    public int? VerseStart { get; set; }
    public int? VerseEnd { get; set; }
    [NotMapped]
    public int? StartVerse
    {
        get => VerseStart;
        set => VerseStart = value;
    }
    [NotMapped]
    public int? EndVerse
    {
        get => VerseEnd;
        set => VerseEnd = value;
    }

    public string Display()
    {
        var chapter = $"{BookTitle} {Chapter}";
        return VerseStart is null
            ? chapter
            : VerseEnd is null || VerseEnd == VerseStart
                ? $"{chapter}:{VerseStart}"
                : $"{chapter}:{VerseStart}-{VerseEnd}";
    }
}
