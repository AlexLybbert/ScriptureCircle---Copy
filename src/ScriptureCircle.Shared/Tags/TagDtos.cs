namespace ScriptureCircle.Shared.Tags;

public sealed record TagDto(Guid Id, string Name);

public sealed record CreateTagRequest(string Name);
