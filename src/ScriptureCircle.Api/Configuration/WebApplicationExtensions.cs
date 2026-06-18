using ScriptureCircle.Infrastructure.Data;

namespace ScriptureCircle.Api.Configuration;

internal static class WebApplicationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await DatabaseInitializer.SeedAsync(db);
    }
}
