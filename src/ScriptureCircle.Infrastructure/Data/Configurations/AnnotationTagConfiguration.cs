using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScriptureCircle.Domain.Entities;

namespace ScriptureCircle.Infrastructure.Data.Configurations;

internal sealed class AnnotationTagConfiguration : IEntityTypeConfiguration<AnnotationTag>
{
    public void Configure(EntityTypeBuilder<AnnotationTag> entity)
    {
        entity.HasKey(at => new { at.AnnotationId, at.TagId });
        entity.HasOne(at => at.Annotation).WithMany(a => a.Tags).HasForeignKey(at => at.AnnotationId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(at => at.Tag).WithMany(t => t.Annotations).HasForeignKey(at => at.TagId).OnDelete(DeleteBehavior.Restrict);
    }
}
