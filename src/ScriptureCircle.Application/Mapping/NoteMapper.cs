using ScriptureCircle.Domain.Entities;
using ScriptureCircle.Shared.Notes;

namespace ScriptureCircle.Application.Mapping;

public static class NoteMapper
{
    public static NoteDto ToDto(Note note) => new(
        note.Id,
        note.UserId,
        note.User?.DisplayName ?? "Unknown",
        ScriptureReferenceMapper.ToDto(note.Reference),
        note.Body,
        note.IsPublic,
        note.CreatedAt,
        note.UpdatedAt);
}
