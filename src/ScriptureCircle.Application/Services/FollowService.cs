using Microsoft.EntityFrameworkCore;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Domain.Entities;

namespace ScriptureCircle.Application.Services;

public sealed class FollowService(IAppDbContext db) : IFollowService
{
    public async Task<FollowResult> SubscribeAsync(Guid subscriberUserId, Guid creatorUserId, CancellationToken cancellationToken)
    {
        if (subscriberUserId == creatorUserId)
        {
            return FollowResult.CannotFollowSelf;
        }

        var creatorExists = await db.Users.AnyAsync(u => u.Id == creatorUserId, cancellationToken);
        if (!creatorExists)
        {
            return FollowResult.NotFound;
        }

        var exists = await db.UserFollows.AnyAsync(s => s.SubscriberUserId == subscriberUserId && s.CreatorUserId == creatorUserId, cancellationToken);
        if (!exists)
        {
            db.UserFollows.Add(new Subscription { SubscriberUserId = subscriberUserId, CreatorUserId = creatorUserId });
            await db.SaveChangesAsync(cancellationToken);
        }

        return FollowResult.Success;
    }

    public async Task<bool> UnsubscribeAsync(Guid subscriberUserId, Guid creatorUserId, CancellationToken cancellationToken)
    {
        var subscription = await db.UserFollows.SingleOrDefaultAsync(s => s.SubscriberUserId == subscriberUserId && s.CreatorUserId == creatorUserId, cancellationToken);
        if (subscription is null)
        {
            return false;
        }

        db.UserFollows.Remove(subscription);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
