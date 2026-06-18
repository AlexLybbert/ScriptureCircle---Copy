namespace ScriptureCircle.Domain.Entities;

public sealed class UserSubscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Plan { get; set; } = "Free";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? EndsAt { get; set; }
}
