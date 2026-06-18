using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScriptureCircle.Domain.Entities;

namespace ScriptureCircle.Infrastructure.Data.Configurations;

internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> entity)
    {
        entity.HasKey(t => t.Id);
        entity.HasIndex(t => new { t.UserId, t.Name }).IsUnique();
        entity.Property(t => t.Name).HasMaxLength(80).IsRequired();
        entity.HasOne(t => t.User).WithMany(u => u.Tags).HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
