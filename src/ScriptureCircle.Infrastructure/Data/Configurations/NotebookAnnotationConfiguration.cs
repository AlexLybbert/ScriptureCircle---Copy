using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScriptureCircle.Domain.Entities;

namespace ScriptureCircle.Infrastructure.Data.Configurations;

internal sealed class NotebookAnnotationConfiguration : IEntityTypeConfiguration<NotebookAnnotation>
{
    public void Configure(EntityTypeBuilder<NotebookAnnotation> entity)
    {
        entity.HasKey(na => new { na.NotebookId, na.AnnotationId });
        entity.HasOne(na => na.Notebook).WithMany(n => n.Annotations).HasForeignKey(na => na.NotebookId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(na => na.Annotation).WithMany(a => a.Notebooks).HasForeignKey(na => na.AnnotationId).OnDelete(DeleteBehavior.Restrict);
    }
}
