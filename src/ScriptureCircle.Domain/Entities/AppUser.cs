namespace ScriptureCircle.Domain.Entities;

public sealed class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ProfileSlug { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<Note> Notes { get; set; } = new List<Note>();
    public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<Notebook> Notebooks { get; set; } = new List<Notebook>();
    public ICollection<Subscription> CreatorSubscriptions { get; set; } = new List<Subscription>();
    public ICollection<Subscription> Following { get; set; } = new List<Subscription>();
}
