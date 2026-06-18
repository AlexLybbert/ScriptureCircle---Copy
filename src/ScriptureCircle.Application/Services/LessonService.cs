using Microsoft.EntityFrameworkCore;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Application.Mapping;
using ScriptureCircle.Domain.Entities;
using ScriptureCircle.Shared.Lessons;

namespace ScriptureCircle.Application.Services;

public sealed class LessonService(IAppDbContext db)
{
    public async Task<IReadOnlyList<LessonDto>> GetVisibleAsync(Guid? currentUserId, CancellationToken cancellationToken)
    {
        var lessons = await db.Lessons
            .Include(l => l.CreatedByUser)
            .Where(l => l.IsPublic || l.CreatedByUserId == currentUserId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        return lessons.Select(LessonMapper.ToDto).ToList();
    }

    public async Task<LessonDto> CreateAsync(Guid userId, CreateLessonRequest request, CancellationToken cancellationToken)
    {
        var lesson = new Lesson
        {
            CreatedByUserId = userId,
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            Reference = ScriptureReferenceMapper.ToEntity(request.Reference),
            IsPublic = request.IsPublic
        };

        db.Lessons.Add(lesson);
        await db.SaveChangesAsync(cancellationToken);

        var saved = await db.Lessons.Include(l => l.CreatedByUser).SingleAsync(l => l.Id == lesson.Id, cancellationToken);
        return LessonMapper.ToDto(saved);
    }

}
