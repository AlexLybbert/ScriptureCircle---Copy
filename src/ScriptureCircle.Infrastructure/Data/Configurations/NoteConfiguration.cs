using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScriptureCircle.Domain.Entities;

namespace ScriptureCircle.Infrastructure.Data.Configurations;

internal sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> entity)
    {
        entity.HasKey(n => n.Id);
        entity.Property(n => n.Body).HasMaxLength(4000).IsRequired();
        entity.OwnsOne(n => n.Reference, ScriptureReferenceConfiguration.Configure);
        entity.HasOne(n => n.User).WithMany(u => u.Notes).HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
