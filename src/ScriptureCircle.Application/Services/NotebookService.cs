using Microsoft.EntityFrameworkCore;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Application.Common;
using ScriptureCircle.Application.Mapping;
using ScriptureCircle.Domain.Entities;
using ScriptureCircle.Domain.Enums;
using ScriptureCircle.Shared.Notebooks;

namespace ScriptureCircle.Application.Services;

public sealed class NotebookService(IAppDbContext db) : INotebookService
{
    public async Task<IReadOnlyList<NotebookDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken)
    {
        var notebooks = await db.Notebooks
            .AsNoTracking()
            .Include(n => n.Annotations)
            .Where(n => n.UserId == userId)
            .OrderBy(n => n.Title)
            .ToListAsync(cancellationToken);

        return notebooks.Select(NotebookMapper.ToDto).ToList();
    }

    public async Task<NotebookDto?> GetAsync(Guid userId, Guid notebookId, CancellationToken cancellationToken)
    {
        var notebook = await db.Notebooks
            .AsNoTracking()
            .Include(n => n.Annotations)
            .SingleOrDefaultAsync(n => n.Id == notebookId && n.UserId == userId, cancellationToken);

        return notebook is null ? null : NotebookMapper.ToDto(notebook);
    }

    public async Task<NotebookDto> CreateAsync(Guid userId, CreateNotebookRequest request, CancellationToken cancellationToken)
    {
        var notebook = new Notebook
        {
            UserId = userId,
            Title = RequiredTitle(request.Title),
            Type = EnumParser.Parse<NotebookType>(request.Type),
            IsPublic = request.IsPublic
        };

        db.Notebooks.Add(notebook);
        await db.SaveChangesAsync(cancellationToken);
        return NotebookMapper.ToDto(notebook);
    }

    public async Task<NotebookDto?> UpdateAsync(Guid userId, Guid notebookId, UpdateNotebookRequest request, CancellationToken cancellationToken)
    {
        var notebook = await db.Notebooks
            .Include(n => n.Annotations)
            .SingleOrDefaultAsync(n => n.Id == notebookId && n.UserId == userId, cancellationToken);
        if (notebook is null)
        {
            return null;
        }

        notebook.Title = RequiredTitle(request.Title);
        notebook.Type = EnumParser.Parse<NotebookType>(request.Type);
        notebook.IsPublic = request.IsPublic;
        await db.SaveChangesAsync(cancellationToken);

        return NotebookMapper.ToDto(notebook);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid notebookId, CancellationToken cancellationToken)
    {
        var notebook = await db.Notebooks.SingleOrDefaultAsync(n => n.Id == notebookId && n.UserId == userId, cancellationToken);
        if (notebook is null)
        {
            return false;
        }

        db.Notebooks.Remove(notebook);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AddAnnotationAsync(Guid userId, Guid notebookId, Guid annotationId, CancellationToken cancellationToken)
    {
        var notebook = await db.Notebooks
            .Include(n => n.Annotations)
            .SingleOrDefaultAsync(n => n.Id == notebookId && n.UserId == userId, cancellationToken);
        var annotationExists = await db.Annotations.AnyAsync(a => a.Id == annotationId && a.UserId == userId, cancellationToken);
        if (notebook is null || !annotationExists)
        {
            return false;
        }

        if (notebook.Annotations.All(a => a.AnnotationId != annotationId))
        {
            notebook.Annotations.Add(new NotebookAnnotation
            {
                NotebookId = notebookId,
                AnnotationId = annotationId,
                SortOrder = notebook.Annotations.Count
            });
            await db.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<bool> RemoveAnnotationAsync(Guid userId, Guid notebookId, Guid annotationId, CancellationToken cancellationToken)
    {
        var item = await db.NotebookAnnotations
            .SingleOrDefaultAsync(a => a.NotebookId == notebookId && a.AnnotationId == annotationId && a.Notebook.UserId == userId, cancellationToken);
        if (item is null)
        {
            return false;
        }

        db.NotebookAnnotations.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ReorderAnnotationsAsync(Guid userId, Guid notebookId, ReorderNotebookAnnotationsRequest request, CancellationToken cancellationToken)
    {
        var notebook = await db.Notebooks
            .Include(n => n.Annotations)
            .SingleOrDefaultAsync(n => n.Id == notebookId && n.UserId == userId, cancellationToken);
        if (notebook is null)
        {
            return false;
        }

        for (var index = 0; index < request.AnnotationIds.Count; index += 1)
        {
            var item = notebook.Annotations.SingleOrDefault(a => a.AnnotationId == request.AnnotationIds[index]);
            if (item is not null)
            {
                item.SortOrder = index;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<NotebookDto?> GetSharedAsync(string shareSlug, CancellationToken cancellationToken)
    {
        var notebook = await db.Notebooks
            .AsNoTracking()
            .Include(n => n.Annotations)
            .SingleOrDefaultAsync(n => n.ShareSlug == shareSlug && n.IsPublic, cancellationToken);

        return notebook is null ? null : NotebookMapper.ToDto(notebook);
    }

    private static string RequiredTitle(string title)
    {
        var trimmed = title.Trim();
        return string.IsNullOrWhiteSpace(trimmed)
            ? throw new ArgumentException("Notebook title is required.")
            : trimmed;
    }
}
