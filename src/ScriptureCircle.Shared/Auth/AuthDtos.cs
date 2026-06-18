namespace ScriptureCircle.Shared.Auth;

public sealed record RegisterRequest(string DisplayName, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(Guid UserId, string DisplayName, string Email, string Token, string ProfileSlug);
