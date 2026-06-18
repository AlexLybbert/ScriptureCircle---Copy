using ScriptureCircle.Shared.Annotations;

namespace ScriptureCircle.Application.Abstractions;

public interface IAnnotationService
{
    Task<IReadOnlyList<AnnotationDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AnnotationDto>> GetForReferenceAsync(Guid userId, string volumeId, string bookId, int chapterNumber, CancellationToken cancellationToken);
    Task<AnnotationDto?> GetAsync(Guid id, Guid? userId, CancellationToken cancellationToken);
    Task<AnnotationDto> CreateAsync(Guid userId, CreateAnnotationRequest request, CancellationToken cancellationToken);
    Task<AnnotationDto?> UpdateAsync(Guid id, Guid userId, UpdateAnnotationRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<AnnotationDto?> GetSharedAsync(string shareSlug, CancellationToken cancellationToken);
    Task<IReadOnlyList<AnnotationDto>> GetSubscriptionFeedAsync(Guid userId, CancellationToken cancellationToken);
}
