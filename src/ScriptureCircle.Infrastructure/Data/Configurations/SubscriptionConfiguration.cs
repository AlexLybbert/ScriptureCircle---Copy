using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScriptureCircle.Domain.Entities;

namespace ScriptureCircle.Infrastructure.Data.Configurations;

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> entity)
    {
        entity.HasKey(s => new { s.SubscriberUserId, s.CreatorUserId });
        entity.HasOne(s => s.SubscriberUser).WithMany(u => u.Following).HasForeignKey(s => s.SubscriberUserId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(s => s.CreatorUser).WithMany(u => u.CreatorSubscriptions).HasForeignKey(s => s.CreatorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
