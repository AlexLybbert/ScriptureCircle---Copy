namespace ScriptureCircle.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = "ScriptureCircle";
    public string Audience { get; init; } = "ScriptureCircle";
    public string SigningKey { get; init; } = "development-only-key-change-me-at-least-32-chars";
    public int ExpirationMinutes { get; init; } = 120;
}
