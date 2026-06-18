using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScriptureCircle.Domain.Entities;

namespace ScriptureCircle.Infrastructure.Data.Configurations;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> entity)
    {
        entity.HasKey(u => u.Id);
        entity.HasIndex(u => u.Email).IsUnique();
        entity.HasIndex(u => u.ProfileSlug).IsUnique();
        entity.Property(u => u.DisplayName).HasMaxLength(120).IsRequired();
        entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
        entity.Property(u => u.ProfileSlug).HasMaxLength(140).IsRequired();
        entity.Property(u => u.PasswordHash).IsRequired();
    }
}
