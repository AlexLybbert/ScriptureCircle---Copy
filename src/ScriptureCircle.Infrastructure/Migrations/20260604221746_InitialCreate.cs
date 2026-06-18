using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScriptureCircle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
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
                """);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ProfileSlug = table.Column<string>(type: "nvarchar(140)", maxLength: 140, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Annotations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reference_VolumeId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Reference_BookId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Reference_BookTitle = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Reference_Chapter = table.Column<int>(type: "int", nullable: false),
                    Reference_VerseStart = table.Column<int>(type: "int", nullable: true),
                    Reference_VerseEnd = table.Column<int>(type: "int", nullable: true),
                    ContentAnchor_ContentItemId = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    ContentAnchor_ContentType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ContentAnchor_AnchorType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ContentAnchor_StartVerse = table.Column<int>(type: "int", nullable: true),
                    ContentAnchor_EndVerse = table.Column<int>(type: "int", nullable: true),
                    ContentAnchor_StartOffset = table.Column<int>(type: "int", nullable: true),
                    ContentAnchor_EndOffset = table.Column<int>(type: "int", nullable: true),
                    ContentAnchor_ParagraphId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    HighlightStyle = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    NotePlainText = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    NoteHtml = table.Column<string>(type: "nvarchar(max)", maxLength: 16000, nullable: true),
                    Visibility = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ShareSlug = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Annotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Annotations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lessons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Reference_VolumeId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Reference_BookId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Reference_BookTitle = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Reference_Chapter = table.Column<int>(type: "int", nullable: false),
                    Reference_VerseStart = table.Column<int>(type: "int", nullable: true),
                    Reference_VerseEnd = table.Column<int>(type: "int", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lessons_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notebooks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    ShareSlug = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notebooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notebooks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reference_VolumeId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Reference_BookId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Reference_BookTitle = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Reference_Chapter = table.Column<int>(type: "int", nullable: false),
                    Reference_VerseStart = table.Column<int>(type: "int", nullable: true),
                    Reference_VerseEnd = table.Column<int>(type: "int", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Plan = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFollows",
                columns: table => new
                {
                    SubscriberUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFollows", x => new { x.SubscriberUserId, x.CreatorUserId });
                    table.ForeignKey(
                        name: "FK_UserFollows_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserFollows_Users_SubscriberUserId",
                        column: x => x.SubscriberUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnnotationLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnnotationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reference_VolumeId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Reference_BookId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Reference_BookTitle = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Reference_Chapter = table.Column<int>(type: "int", nullable: false),
                    Reference_VerseStart = table.Column<int>(type: "int", nullable: true),
                    Reference_VerseEnd = table.Column<int>(type: "int", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnotationLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnnotationLinks_Annotations_AnnotationId",
                        column: x => x.AnnotationId,
                        principalTable: "Annotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotebookAnnotations",
                columns: table => new
                {
                    NotebookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnnotationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotebookAnnotations", x => new { x.NotebookId, x.AnnotationId });
                    table.ForeignKey(
                        name: "FK_NotebookAnnotations_Annotations_AnnotationId",
                        column: x => x.AnnotationId,
                        principalTable: "Annotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotebookAnnotations_Notebooks_NotebookId",
                        column: x => x.NotebookId,
                        principalTable: "Notebooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnnotationTag",
                columns: table => new
                {
                    AnnotationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnotationTag", x => new { x.AnnotationId, x.TagId });
                    table.ForeignKey(
                        name: "FK_AnnotationTag_Annotations_AnnotationId",
                        column: x => x.AnnotationId,
                        principalTable: "Annotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnnotationTag_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnnotationLinks_AnnotationId",
                table: "AnnotationLinks",
                column: "AnnotationId");

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_ShareSlug",
                table: "Annotations",
                column: "ShareSlug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_UserId",
                table: "Annotations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnnotationTag_TagId",
                table: "AnnotationTag",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_CreatedByUserId",
                table: "Lessons",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotebookAnnotations_AnnotationId",
                table: "NotebookAnnotations",
                column: "AnnotationId");

            migrationBuilder.CreateIndex(
                name: "IX_Notebooks_ShareSlug",
                table: "Notebooks",
                column: "ShareSlug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notebooks_UserId",
                table: "Notebooks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_UserId",
                table: "Notes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId_Name",
                table: "Subscriptions",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserId_Name",
                table: "Tags",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserFollows_CreatorUserId",
                table: "UserFollows",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ProfileSlug",
                table: "Users",
                column: "ProfileSlug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnnotationLinks");

            migrationBuilder.DropTable(
                name: "AnnotationTag");

            migrationBuilder.DropTable(
                name: "ContentItems");

            migrationBuilder.DropTable(
                name: "Lessons");

            migrationBuilder.DropTable(
                name: "NotebookAnnotations");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "UserFollows");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Annotations");

            migrationBuilder.DropTable(
                name: "Notebooks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
