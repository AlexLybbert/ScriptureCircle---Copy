namespace ScriptureCircle.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

public static class DatabaseInitializer
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        if (db.Database.IsRelational())
        {
            await db.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await db.Database.EnsureCreatedAsync(cancellationToken);
        }

        await EnsureSqlServerSchemaCompatibilityAsync(db, cancellationToken);
    }

    private static async Task EnsureSqlServerSchemaCompatibilityAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        if (!db.Database.IsSqlServer())
        {
            return;
        }

        await db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[ContentItems]', N'U') IS NULL
            BEGIN
                CREATE TABLE [ContentItems] (
                    [Id] nvarchar(160) NOT NULL,
                    [ContentType] nvarchar(64) NOT NULL,
                    [Title] nvarchar(256) NOT NULL,
                    [SourceId] nvarchar(256) NULL,
                    [VolumeId] nvarchar(64) NULL,
                    [BookId] nvarchar(64) NULL,
                    [ChapterNumber] int NULL,
                    CONSTRAINT [PK_ContentItems] PRIMARY KEY ([Id])
                );
            END;
            """, cancellationToken);

        await db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[Annotations]', N'U') IS NOT NULL AND COL_LENGTH(N'[Annotations]', N'ContentAnchor_ContentItemId') IS NULL
            BEGIN
                ALTER TABLE [Annotations] ADD [ContentAnchor_ContentItemId] nvarchar(160) NOT NULL CONSTRAINT [DF_Annotations_ContentAnchor_ContentItemId] DEFAULT N'';
                ALTER TABLE [Annotations] ADD [ContentAnchor_ContentType] nvarchar(64) NOT NULL CONSTRAINT [DF_Annotations_ContentAnchor_ContentType] DEFAULT N'Scripture';
                ALTER TABLE [Annotations] ADD [ContentAnchor_AnchorType] nvarchar(64) NOT NULL CONSTRAINT [DF_Annotations_ContentAnchor_AnchorType] DEFAULT N'VerseRange';
                ALTER TABLE [Annotations] ADD [ContentAnchor_StartVerse] int NULL;
                ALTER TABLE [Annotations] ADD [ContentAnchor_EndVerse] int NULL;
                ALTER TABLE [Annotations] ADD [ContentAnchor_StartOffset] int NULL;
                ALTER TABLE [Annotations] ADD [ContentAnchor_EndOffset] int NULL;
                ALTER TABLE [Annotations] ADD [ContentAnchor_ParagraphId] nvarchar(128) NULL;
            END;
            """, cancellationToken);

        await db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[Annotations]', N'U') IS NOT NULL AND COL_LENGTH(N'[Annotations]', N'ContentAnchor_ContentItemId') IS NOT NULL
            BEGIN
                UPDATE [Annotations]
                SET
                    [ContentAnchor_ContentItemId] = CONCAT(N'scripture:', [Reference_VolumeId], N':', [Reference_BookId], N':', [Reference_Chapter]),
                    [ContentAnchor_StartVerse] = [Reference_VerseStart],
                    [ContentAnchor_EndVerse] = COALESCE([Reference_VerseEnd], [Reference_VerseStart]),
                    [ContentAnchor_AnchorType] = CASE WHEN [Reference_VerseStart] IS NULL THEN N'Chapter' ELSE N'VerseRange' END;
            END;
            """, cancellationToken);
    }
}
