using ScriptureCircle.Shared.Annotations;
using ScriptureCircle.Shared.Notebooks;
using ScriptureCircle.Shared.Profiles;

namespace ScriptureCircle.Application.Abstractions;

public interface IProfileService
{
    Task<PublicProfileDto?> GetAsync(string profileSlug, Guid? currentUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AnnotationDto>?> GetPublicAnnotationsAsync(string profileSlug, CancellationToken cancellationToken);
    Task<IReadOnlyList<NotebookDto>?> GetPublicNotebooksAsync(string profileSlug, CancellationToken cancellationToken);
}
