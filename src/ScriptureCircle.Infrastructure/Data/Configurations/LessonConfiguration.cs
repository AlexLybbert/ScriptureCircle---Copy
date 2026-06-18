using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScriptureCircle.Domain.Entities;

namespace ScriptureCircle.Infrastructure.Data.Configurations;

internal sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> entity)
    {
        entity.HasKey(l => l.Id);
        entity.Property(l => l.Title).HasMaxLength(160).IsRequired();
        entity.Property(l => l.Summary).HasMaxLength(4000).IsRequired();
        entity.OwnsOne(l => l.Reference, ScriptureReferenceConfiguration.Configure);
        entity.HasOne(l => l.CreatedByUser).WithMany(u => u.Lessons).HasForeignKey(l => l.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
