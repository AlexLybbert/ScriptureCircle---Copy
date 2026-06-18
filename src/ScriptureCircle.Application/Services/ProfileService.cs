using Microsoft.EntityFrameworkCore;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Application.Mapping;
using ScriptureCircle.Domain.Entities;
using ScriptureCircle.Domain.Enums;
using ScriptureCircle.Shared.Annotations;
using ScriptureCircle.Shared.Notebooks;
using ScriptureCircle.Shared.Profiles;

namespace ScriptureCircle.Application.Services;

public sealed class ProfileService(IAppDbContext db) : IProfileService
{
    public async Task<PublicProfileDto?> GetAsync(string profileSlug, Guid? currentUserId, CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.ProfileSlug == profileSlug, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var isSubscribed = currentUserId is not null &&
            await db.UserFollows.AnyAsync(s => s.SubscriberUserId == currentUserId && s.CreatorUserId == user.Id, cancellationToken);

        return new PublicProfileDto(user.Id, user.DisplayName, user.ProfileSlug, isSubscribed);
    }

    public async Task<IReadOnlyList<AnnotationDto>?> GetPublicAnnotationsAsync(string profileSlug, CancellationToken cancellationToken)
    {
        var user = await FindBySlugAsync(profileSlug, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var annotations = await db.Annotations
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Tags).ThenInclude(t => t.Tag)
            .Where(a => a.UserId == user.Id && a.Visibility == AnnotationVisibility.Public)
            .OrderByDescending(a => a.UpdatedAt)
            .ToListAsync(cancellationToken);

        return annotations.Select(AnnotationMapper.ToDto).ToList();
    }

    public async Task<IReadOnlyList<NotebookDto>?> GetPublicNotebooksAsync(string profileSlug, CancellationToken cancellationToken)
    {
        var user = await FindBySlugAsync(profileSlug, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var notebooks = await db.Notebooks
            .AsNoTracking()
            .Include(n => n.Annotations)
            .Where(n => n.UserId == user.Id && n.IsPublic)
            .OrderBy(n => n.Title)
            .ToListAsync(cancellationToken);

        return notebooks.Select(NotebookMapper.ToDto).ToList();
    }

    private Task<AppUser?> FindBySlugAsync(string profileSlug, CancellationToken cancellationToken) =>
        db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.ProfileSlug == profileSlug, cancellationToken);
}
