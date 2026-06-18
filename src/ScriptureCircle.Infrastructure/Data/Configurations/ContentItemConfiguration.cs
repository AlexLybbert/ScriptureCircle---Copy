using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScriptureCircle.Domain.Entities;

namespace ScriptureCircle.Infrastructure.Data.Configurations;

internal sealed class ContentItemConfiguration : IEntityTypeConfiguration<ContentItem>
{
    public void Configure(EntityTypeBuilder<ContentItem> entity)
    {
        entity.HasKey(c => c.Id);
        entity.Property(c => c.Id).HasMaxLength(160);
        entity.Property(c => c.ContentType).HasMaxLength(64).IsRequired();
        entity.Property(c => c.Title).HasMaxLength(256).IsRequired();
        entity.Property(c => c.SourceId).HasMaxLength(256);
        entity.Property(c => c.VolumeId).HasMaxLength(64);
        entity.Property(c => c.BookId).HasMaxLength(64);
    }
}
