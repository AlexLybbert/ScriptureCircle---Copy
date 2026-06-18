using Microsoft.EntityFrameworkCore;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Application.Mapping;
using ScriptureCircle.Domain.Entities;
using ScriptureCircle.Shared.Notes;
using ScriptureCircle.Shared.Scriptures;

namespace ScriptureCircle.Application.Services;

public sealed class NoteService(IAppDbContext db)
{
    public async Task<IReadOnlyList<NoteDto>> GetForReferenceAsync(ScriptureReferenceDto reference, Guid? currentUserId, CancellationToken cancellationToken)
    {
        var notes = await db.Notes
            .Include(n => n.User)
            .Where(n =>
                n.Reference.VolumeId == reference.VolumeId &&
                n.Reference.BookId == reference.BookId &&
                n.Reference.Chapter == reference.Chapter &&
                (n.IsPublic || n.UserId == currentUserId))
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);

        return notes.Select(NoteMapper.ToDto).ToList();
    }

    public async Task<IReadOnlyList<NoteDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken)
    {
        var notes = await db.Notes
            .Include(n => n.User)
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);

        return notes.Select(NoteMapper.ToDto).ToList();
    }

    public async Task<NoteDto> CreateAsync(Guid userId, CreateNoteRequest request, CancellationToken cancellationToken)
    {
        var note = new Note
        {
            UserId = userId,
            Reference = ScriptureReferenceMapper.ToEntity(request.Reference),
            Body = request.Body.Trim(),
            IsPublic = request.IsPublic
        };

        db.Notes.Add(note);
        await db.SaveChangesAsync(cancellationToken);

        var saved = await db.Notes.Include(n => n.User).SingleAsync(n => n.Id == note.Id, cancellationToken);
        return NoteMapper.ToDto(saved);
    }

    public async Task<NoteDto?> UpdateAsync(Guid userId, Guid noteId, UpdateNoteRequest request, CancellationToken cancellationToken)
    {
        var note = await db.Notes.Include(n => n.User).SingleOrDefaultAsync(n => n.Id == noteId && n.UserId == userId, cancellationToken);
        if (note is null)
        {
            return null;
        }

        note.Body = request.Body.Trim();
        note.IsPublic = request.IsPublic;
        note.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return NoteMapper.ToDto(note);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid noteId, CancellationToken cancellationToken)
    {
        var note = await db.Notes.SingleOrDefaultAsync(n => n.Id == noteId && n.UserId == userId, cancellationToken);
        if (note is null)
        {
            return false;
        }

        db.Notes.Remove(note);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

}
