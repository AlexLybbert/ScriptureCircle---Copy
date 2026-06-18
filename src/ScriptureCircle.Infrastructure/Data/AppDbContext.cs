using Microsoft.EntityFrameworkCore;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Domain.Entities;

namespace ScriptureCircle.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<UserSubscription> Subscriptions => Set<UserSubscription>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<Annotation> Annotations => Set<Annotation>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Notebook> Notebooks => Set<Notebook>();
    public DbSet<NotebookAnnotation> NotebookAnnotations => Set<NotebookAnnotation>();
    public DbSet<AnnotationLink> AnnotationLinks => Set<AnnotationLink>();
    public DbSet<Subscription> UserFollows => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
