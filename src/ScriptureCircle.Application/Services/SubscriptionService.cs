using Microsoft.EntityFrameworkCore;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Domain.Entities;
using ScriptureCircle.Shared.Subscriptions;

namespace ScriptureCircle.Application.Services;

public sealed class SubscriptionService(IAppDbContext db)
{
    public async Task<IReadOnlyList<SubscriptionDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken)
    {
        var subscriptions = await db.Subscriptions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(cancellationToken);

        return subscriptions.Select(ToDto).ToList();
    }

    public async Task<SubscriptionDto> UpsertAsync(Guid userId, UpsertSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var subscription = await db.Subscriptions
            .SingleOrDefaultAsync(s => s.UserId == userId && s.Name == request.Name, cancellationToken);

        if (subscription is null)
        {
            subscription = new UserSubscription { UserId = userId, Name = request.Name.Trim() };
            db.Subscriptions.Add(subscription);
        }

        subscription.Plan = request.Plan.Trim();
        subscription.IsActive = request.IsActive;
        subscription.EndsAt = request.EndsAt;

        await db.SaveChangesAsync(cancellationToken);
        return ToDto(subscription);
    }

    private static SubscriptionDto ToDto(UserSubscription subscription) => new(
        subscription.Id,
        subscription.Name,
        subscription.Plan,
        subscription.IsActive,
        subscription.StartedAt,
        subscription.EndsAt);
}
