using ScriptureCircle.Domain.Entities;
using ScriptureCircle.Shared.Notebooks;

namespace ScriptureCircle.Application.Mapping;

public static class NotebookMapper
{
    public static NotebookDto ToDto(Notebook notebook) => new(
        notebook.Id,
        notebook.Title,
        notebook.Type.ToString(),
        notebook.IsPublic,
        notebook.ShareSlug,
        notebook.Annotations.OrderBy(a => a.SortOrder).Select(a => a.AnnotationId).ToList());
}
