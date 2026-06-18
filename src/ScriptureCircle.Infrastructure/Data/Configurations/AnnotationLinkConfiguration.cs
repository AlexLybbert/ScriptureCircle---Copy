using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScriptureCircle.Domain.Entities;

namespace ScriptureCircle.Infrastructure.Data.Configurations;

internal sealed class AnnotationLinkConfiguration : IEntityTypeConfiguration<AnnotationLink>
{
    public void Configure(EntityTypeBuilder<AnnotationLink> entity)
    {
        entity.HasKey(l => l.Id);
        entity.Property(l => l.Label).HasMaxLength(160).IsRequired();
        entity.OwnsOne(l => l.Reference, ScriptureReferenceConfiguration.Configure);
        entity.HasOne(l => l.Annotation).WithMany(a => a.Links).HasForeignKey(l => l.AnnotationId).OnDelete(DeleteBehavior.Cascade);
    }
}
