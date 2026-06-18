using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Infrastructure.Authentication;
using ScriptureCircle.Infrastructure.Data;
using ScriptureCircle.Infrastructure.Scriptures;

namespace ScriptureCircle.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<OpenScriptureOptions>(configuration.GetSection(OpenScriptureOptions.SectionName));

        services.AddDbContext<AppDbContext>(options =>
        {
            if (configuration.GetValue<string>("DatabaseProvider") == "InMemory")
            {
                options.UseInMemoryDatabase(configuration.GetValue<string>("DatabaseName") ?? "ScriptureCircle");
            }
            else
            {
                var connectionString = NormalizePostgresConnectionString(configuration.GetConnectionString("DefaultConnection"));
                options.UseNpgsql(connectionString);
            }
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IAuthService, AuthService>();
        services.AddHttpClient<OpenScriptureService>();
        services.AddScoped<IScriptureProvider>(sp => sp.GetRequiredService<OpenScriptureService>());
        services.AddScoped<IScriptureService>(sp => sp.GetRequiredService<OpenScriptureService>());

        return services;
    }

    private static string NormalizePostgresConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required when using PostgreSQL.");
        }

        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
        {
            return connectionString;
        }

        var credentials = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(credentials[0]);
        var password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : string.Empty;
        var database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));

        return $"Host={uri.Host};Port={uri.Port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }
}
