namespace ScriptureCircle.Domain.Entities;

public sealed class Subscription
{
    public Guid SubscriberUserId { get; set; }
    public AppUser SubscriberUser { get; set; } = null!;
    public Guid CreatorUserId { get; set; }
    public AppUser CreatorUser { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
