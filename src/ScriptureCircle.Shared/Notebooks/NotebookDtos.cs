namespace ScriptureCircle.Shared.Notebooks;

public sealed record NotebookDto(Guid Id, string Title, string Type, bool IsPublic, string ShareSlug, IReadOnlyList<Guid> AnnotationIds);

public sealed record CreateNotebookRequest(string Title, string Type, bool IsPublic);

public sealed record UpdateNotebookRequest(string Title, string Type, bool IsPublic);

public sealed record AddNotebookAnnotationRequest(Guid AnnotationId);

public sealed record ReorderNotebookAnnotationsRequest(IReadOnlyList<Guid> AnnotationIds);
