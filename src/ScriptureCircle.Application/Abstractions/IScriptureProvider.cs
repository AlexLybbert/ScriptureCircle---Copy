using ScriptureCircle.Shared.Scriptures;

namespace ScriptureCircle.Application.Abstractions;

public interface IScriptureProvider
{
    Task<ScriptureChapterDto> GetChapterAsync(
        string volumeId,
        string bookId,
        int chapterNumber,
        int? selectedStart,
        int? selectedEnd,
        CancellationToken cancellationToken);
}
