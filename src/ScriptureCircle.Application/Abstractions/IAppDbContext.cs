using Microsoft.EntityFrameworkCore;
using ScriptureCircle.Domain.Entities;

namespace ScriptureCircle.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<AppUser> Users { get; }
    DbSet<Note> Notes { get; }
    DbSet<UserSubscription> Subscriptions { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<ContentItem> ContentItems { get; }
    DbSet<Annotation> Annotations { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Notebook> Notebooks { get; }
    DbSet<NotebookAnnotation> NotebookAnnotations { get; }
    DbSet<AnnotationLink> AnnotationLinks { get; }
    DbSet<Subscription> UserFollows { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
