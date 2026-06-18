using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScriptureCircle.Domain.Entities;

namespace ScriptureCircle.Infrastructure.Data.Configurations;

internal sealed class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> entity)
    {
        entity.HasKey(s => s.Id);
        entity.HasIndex(s => new { s.UserId, s.Name }).IsUnique();
        entity.Property(s => s.Name).HasMaxLength(120).IsRequired();
        entity.Property(s => s.Plan).HasMaxLength(80).IsRequired();
        entity.HasOne(s => s.User).WithMany(u => u.Subscriptions).HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
