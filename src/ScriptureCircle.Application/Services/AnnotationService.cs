using Microsoft.EntityFrameworkCore;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Application.Common;
using ScriptureCircle.Application.Mapping;
using ScriptureCircle.Domain.Entities;
using ScriptureCircle.Domain.Enums;
using ScriptureCircle.Shared.Annotations;
using ScriptureCircle.Shared.Scriptures;

namespace ScriptureCircle.Application.Services;

public sealed class AnnotationService(IAppDbContext db) : IAnnotationService
{
    public async Task<IReadOnlyList<AnnotationDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var annotations = await QueryAnnotations()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.UpdatedAt)
            .ToListAsync(cancellationToken);

        return annotations.Select(AnnotationMapper.ToDto).ToList();
    }

    public async Task<IReadOnlyList<AnnotationDto>> GetForReferenceAsync(
        Guid userId,
        string volumeId,
        string bookId,
        int chapterNumber,
        CancellationToken cancellationToken)
    {
        var annotations = await db.Annotations
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Tags).ThenInclude(t => t.Tag)
            .Where(a => a.UserId == userId
                && a.Reference.VolumeId == volumeId
                && a.Reference.BookId == bookId
                && a.Reference.Chapter == chapterNumber)
            .OrderByDescending(a => a.UpdatedAt)
            .ToListAsync(cancellationToken);

        return annotations.Select(AnnotationMapper.ToDto).ToList();
    }

    public async Task<AnnotationDto?> GetAsync(Guid id, Guid? userId, CancellationToken cancellationToken)
    {
        var annotation = await QueryAnnotations()
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);

        return annotation is null || !CanRead(annotation, userId) ? null : AnnotationMapper.ToDto(annotation);
    }

    public async Task<AnnotationDto> CreateAsync(Guid userId, CreateAnnotationRequest request, CancellationToken cancellationToken)
    {
        var annotation = new Annotation
        {
            UserId = userId,
            Reference = ScriptureReferenceMapper.ToEntity(request.Reference),
            ContentAnchor = ToContentAnchor(request.Reference, request.ContentAnchor),
            HighlightStyle = EnumParser.Parse<HighlightStyle>(request.HighlightStyle),
            Visibility = EnumParser.Parse<AnnotationVisibility>(request.Visibility),
            NotePlainText = request.NotePlainText?.Trim(),
            NoteHtml = request.NoteHtml?.Trim()
        };

        await UpsertScriptureContentItemAsync(request.Reference, cancellationToken);
        await ApplyTagsAsync(annotation, userId, request.Tags, cancellationToken);
        db.Annotations.Add(annotation);
        await db.SaveChangesAsync(cancellationToken);

        return (await GetAsync(annotation.Id, userId, cancellationToken))!;
    }

    public async Task<AnnotationDto?> UpdateAsync(Guid id, Guid userId, UpdateAnnotationRequest request, CancellationToken cancellationToken)
    {
        var annotation = await db.Annotations
            .Include(a => a.Tags)
            .SingleOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);

        if (annotation is null)
        {
            return null;
        }

        annotation.HighlightStyle = EnumParser.Parse<HighlightStyle>(request.HighlightStyle);
        annotation.Visibility = EnumParser.Parse<AnnotationVisibility>(request.Visibility);
        annotation.NotePlainText = request.NotePlainText?.Trim();
        annotation.NoteHtml = request.NoteHtml?.Trim();
        annotation.UpdatedAt = DateTimeOffset.UtcNow;
        annotation.Tags.Clear();
        await ApplyTagsAsync(annotation, userId, request.Tags, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return await GetAsync(id, userId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var annotation = await db.Annotations.SingleOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);
        if (annotation is null)
        {
            return false;
        }

        db.Annotations.Remove(annotation);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<AnnotationDto?> GetSharedAsync(string shareSlug, CancellationToken cancellationToken)
    {
        var annotation = await QueryAnnotations()
            .SingleOrDefaultAsync(a => a.ShareSlug == shareSlug, cancellationToken);

        return annotation is null || annotation.Visibility == AnnotationVisibility.Private ? null : AnnotationMapper.ToDto(annotation);
    }

    public async Task<IReadOnlyList<AnnotationDto>> GetSubscriptionFeedAsync(Guid userId, CancellationToken cancellationToken)
    {
        var creatorIds = db.UserFollows
            .Where(s => s.SubscriberUserId == userId)
            .Select(s => s.CreatorUserId);

        var annotations = await QueryAnnotations()
            .Where(a => creatorIds.Contains(a.UserId) && a.Visibility == AnnotationVisibility.Public)
            .OrderByDescending(a => a.UpdatedAt)
            .ToListAsync(cancellationToken);

        return annotations.Select(AnnotationMapper.ToDto).ToList();
    }

    private IQueryable<Annotation> QueryAnnotations() =>
        db.Annotations
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Tags).ThenInclude(t => t.Tag);

    private async Task ApplyTagsAsync(Annotation annotation, Guid userId, IReadOnlyList<string>? names, CancellationToken cancellationToken)
    {
        foreach (var name in (names ?? []).Select(n => n.Trim()).Where(n => n.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var tag = await db.Tags.SingleOrDefaultAsync(t => t.UserId == userId && t.Name == name, cancellationToken);
            if (tag is null)
            {
                tag = new Tag { UserId = userId, Name = name };
                db.Tags.Add(tag);
            }

            annotation.Tags.Add(new AnnotationTag { Annotation = annotation, Tag = tag });
        }
    }

    private static bool CanRead(Annotation annotation, Guid? userId) =>
        annotation.UserId == userId || annotation.Visibility is AnnotationVisibility.Public or AnnotationVisibility.Unlisted;

    private static ContentAnchor ToContentAnchor(ScriptureReferenceDto reference, ContentAnchorDto? anchor)
    {
        var startVerse = anchor?.StartVerse ?? reference.VerseStart;
        var endVerse = anchor?.EndVerse ?? reference.VerseEnd ?? startVerse;
        var hasOffsets = anchor?.StartOffset is not null || anchor?.EndOffset is not null;

        if (hasOffsets)
        {
            if (startVerse is null || endVerse is null)
            {
                throw new ArgumentException("Phrase annotations must include a verse.");
            }

            if (startVerse != endVerse)
            {
                throw new ArgumentException("Phrase annotations must be within a single verse.");
            }

            if (anchor?.StartOffset is null || anchor.EndOffset is null || anchor.StartOffset < 0 || anchor.EndOffset <= anchor.StartOffset)
            {
                throw new ArgumentException("Phrase annotations must include a valid text range.");
            }
        }

        return new ContentAnchor
        {
            ContentItemId = ScriptureContentItemId(reference),
            ContentType = "Scripture",
            AnchorType = hasOffsets ? "Phrase" : reference.VerseStart is null ? "Chapter" : "VerseRange",
            StartVerse = startVerse,
            EndVerse = endVerse,
            StartOffset = anchor?.StartOffset,
            EndOffset = anchor?.EndOffset,
            ParagraphId = anchor?.ParagraphId?.Trim()
        };
    }

    private async Task UpsertScriptureContentItemAsync(ScriptureReferenceDto reference, CancellationToken cancellationToken)
    {
        var id = ScriptureContentItemId(reference);
        var exists = await db.ContentItems.AnyAsync(c => c.Id == id, cancellationToken);
        if (exists)
        {
            return;
        }

        db.ContentItems.Add(new ContentItem
        {
            Id = id,
            ContentType = "Scripture",
            Title = $"{reference.BookTitle} {reference.Chapter}",
            SourceId = $"{reference.VolumeId}/{reference.BookId}/{reference.Chapter}",
            VolumeId = reference.VolumeId,
            BookId = reference.BookId,
            ChapterNumber = reference.Chapter
        });
    }

    private static string ScriptureContentItemId(ScriptureReferenceDto reference) =>
        $"scripture:{reference.VolumeId}:{reference.BookId}:{reference.Chapter}";
}
