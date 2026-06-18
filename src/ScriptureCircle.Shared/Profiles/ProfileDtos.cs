namespace ScriptureCircle.Shared.Profiles;

public sealed record PublicProfileDto(Guid Id, string DisplayName, string ProfileSlug, bool IsSubscribed);
