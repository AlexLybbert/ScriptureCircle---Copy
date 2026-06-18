using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScriptureCircle.Domain.Entities;

namespace ScriptureCircle.Infrastructure.Data.Configurations;

internal static class ContentAnchorConfiguration
{
    public static void Configure(OwnedNavigationBuilder<Annotation, ContentAnchor> anchor)
    {
        anchor.Property(a => a.ContentItemId).HasMaxLength(160);
        anchor.Property(a => a.ContentType).HasMaxLength(64).IsRequired();
        anchor.Property(a => a.AnchorType).HasMaxLength(64).IsRequired();
        anchor.Property(a => a.ParagraphId).HasMaxLength(128);
    }
}
