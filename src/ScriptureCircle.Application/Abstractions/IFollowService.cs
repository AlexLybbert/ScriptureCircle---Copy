namespace ScriptureCircle.Application.Abstractions;

public enum FollowResult
{
    Success,
    NotFound,
    CannotFollowSelf
}

public interface IFollowService
{
    Task<FollowResult> SubscribeAsync(Guid subscriberUserId, Guid creatorUserId, CancellationToken cancellationToken);
    Task<bool> UnsubscribeAsync(Guid subscriberUserId, Guid creatorUserId, CancellationToken cancellationToken);
}
