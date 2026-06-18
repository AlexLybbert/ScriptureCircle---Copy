using ScriptureCircle.Shared.Notebooks;

namespace ScriptureCircle.Application.Abstractions;

public interface INotebookService
{
    Task<IReadOnlyList<NotebookDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken);
    Task<NotebookDto?> GetAsync(Guid userId, Guid notebookId, CancellationToken cancellationToken);
    Task<NotebookDto> CreateAsync(Guid userId, CreateNotebookRequest request, CancellationToken cancellationToken);
    Task<NotebookDto?> UpdateAsync(Guid userId, Guid notebookId, UpdateNotebookRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid userId, Guid notebookId, CancellationToken cancellationToken);
    Task<bool> AddAnnotationAsync(Guid userId, Guid notebookId, Guid annotationId, CancellationToken cancellationToken);
    Task<bool> RemoveAnnotationAsync(Guid userId, Guid notebookId, Guid annotationId, CancellationToken cancellationToken);
    Task<bool> ReorderAnnotationsAsync(Guid userId, Guid notebookId, ReorderNotebookAnnotationsRequest request, CancellationToken cancellationToken);
    Task<NotebookDto?> GetSharedAsync(string shareSlug, CancellationToken cancellationToken);
}
