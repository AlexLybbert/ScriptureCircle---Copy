using ScriptureCircle.Shared.Tags;

namespace ScriptureCircle.Application.Abstractions;

public interface ITagService
{
    Task<IReadOnlyList<TagDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken);
    Task<TagDto> CreateAsync(Guid userId, CreateTagRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid userId, Guid tagId, CancellationToken cancellationToken);
}
