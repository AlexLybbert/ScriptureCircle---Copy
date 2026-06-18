using Microsoft.EntityFrameworkCore;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Domain.Entities;
using ScriptureCircle.Shared.Tags;

namespace ScriptureCircle.Application.Services;

public sealed class TagService(IAppDbContext db) : ITagService
{
    public async Task<IReadOnlyList<TagDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.Tags
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Name)
            .Select(t => new TagDto(t.Id, t.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<TagDto> CreateAsync(Guid userId, CreateTagRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tag name is required.");
        }

        var tag = await db.Tags.SingleOrDefaultAsync(t => t.UserId == userId && t.Name == name, cancellationToken);
        if (tag is null)
        {
            tag = new Tag { UserId = userId, Name = name };
            db.Tags.Add(tag);
            await db.SaveChangesAsync(cancellationToken);
        }

        return new TagDto(tag.Id, tag.Name);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid tagId, CancellationToken cancellationToken)
    {
        var tag = await db.Tags.SingleOrDefaultAsync(t => t.Id == tagId && t.UserId == userId, cancellationToken);
        if (tag is null)
        {
            return false;
        }

        db.Tags.Remove(tag);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
