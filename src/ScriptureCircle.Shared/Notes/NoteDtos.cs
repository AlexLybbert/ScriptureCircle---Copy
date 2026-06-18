using ScriptureCircle.Shared.Scriptures;

namespace ScriptureCircle.Shared.Notes;

public sealed record NoteDto(
    Guid Id,
    Guid UserId,
    string AuthorName,
    ScriptureReferenceDto Reference,
    string Body,
    bool IsPublic,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateNoteRequest(ScriptureReferenceDto Reference, string Body, bool IsPublic);
public sealed record UpdateNoteRequest(string Body, bool IsPublic);
