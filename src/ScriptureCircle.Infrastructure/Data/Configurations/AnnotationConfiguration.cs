using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScriptureCircle.Domain.Entities;

namespace ScriptureCircle.Infrastructure.Data.Configurations;

internal sealed class AnnotationConfiguration : IEntityTypeConfiguration<Annotation>
{
    public void Configure(EntityTypeBuilder<Annotation> entity)
    {
        entity.HasKey(a => a.Id);
        entity.HasIndex(a => a.ShareSlug).IsUnique();
        entity.Property(a => a.ShareSlug).HasMaxLength(64).IsRequired();
        entity.Property(a => a.NotePlainText).HasMaxLength(8000);
        entity.Property(a => a.NoteHtml).HasMaxLength(16000);
        entity.Property(a => a.HighlightStyle).HasConversion<string>().HasMaxLength(32).IsRequired();
        entity.Property(a => a.Visibility).HasConversion<string>().HasMaxLength(32).IsRequired();
        entity.OwnsOne(a => a.Reference, ScriptureReferenceConfiguration.Configure);
        entity.OwnsOne(a => a.ContentAnchor, ContentAnchorConfiguration.Configure);
        entity.HasOne(a => a.User).WithMany(u => u.Annotations).HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
