using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScriptureCircle.Domain.Entities;

namespace ScriptureCircle.Infrastructure.Data.Configurations;

internal sealed class NotebookConfiguration : IEntityTypeConfiguration<Notebook>
{
    public void Configure(EntityTypeBuilder<Notebook> entity)
    {
        entity.HasKey(n => n.Id);
        entity.HasIndex(n => n.ShareSlug).IsUnique();
        entity.Property(n => n.Title).HasMaxLength(160).IsRequired();
        entity.Property(n => n.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        entity.Property(n => n.ShareSlug).HasMaxLength(64).IsRequired();
        entity.HasOne(n => n.User).WithMany(u => u.Notebooks).HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
