using ScriptureCircle.Shared.Scriptures;

namespace ScriptureCircle.Shared.Lessons;

public sealed record LessonDto(
    Guid Id,
    Guid CreatedByUserId,
    string AuthorName,
    string Title,
    string Summary,
    ScriptureReferenceDto Reference,
    bool IsPublic,
    DateTimeOffset CreatedAt);

public sealed record CreateLessonRequest(string Title, string Summary, ScriptureReferenceDto Reference, bool IsPublic);
