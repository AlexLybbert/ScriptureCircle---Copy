using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScriptureCircle.Domain.Entities;

namespace ScriptureCircle.Infrastructure.Data.Configurations;

internal static class ScriptureReferenceConfiguration
{
    public static void Configure<TOwner>(OwnedNavigationBuilder<TOwner, ScriptureReference> reference)
        where TOwner : class
    {
        reference.Property(r => r.VolumeId).HasMaxLength(80).IsRequired();
        reference.Property(r => r.BookId).HasMaxLength(80).IsRequired();
        reference.Property(r => r.BookTitle).HasMaxLength(160).IsRequired();
    }
}
