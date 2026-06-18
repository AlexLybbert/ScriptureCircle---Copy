using ScriptureCircle.Domain.Entities;
using ScriptureCircle.Shared.Lessons;

namespace ScriptureCircle.Application.Mapping;

public static class LessonMapper
{
    public static LessonDto ToDto(Lesson lesson) => new(
        lesson.Id,
        lesson.CreatedByUserId,
        lesson.CreatedByUser?.DisplayName ?? "Unknown",
        lesson.Title,
        lesson.Summary,
        ScriptureReferenceMapper.ToDto(lesson.Reference),
        lesson.IsPublic,
        lesson.CreatedAt);
}
