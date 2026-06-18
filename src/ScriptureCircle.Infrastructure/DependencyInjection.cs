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
                var connectionString = NormalizePostgresConnectionString(GetPostgresConnectionString(configuration));

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

    private static string? GetPostgresConnectionString(IConfiguration configuration)
    {
        return configuration["DATABASE_URL"]
            ?? configuration["POSTGRES_URL"]
            ?? configuration["POSTGRES_CONNECTION_STRING"]
            ?? configuration["DATABASE_CONNECTION_STRING"]
            ?? configuration.GetConnectionString("DefaultConnection");
    }

    private static string NormalizePostgresConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "PostgreSQL is enabled but no connection string is configured. Set DATABASE_URL, POSTGRES_URL, POSTGRES_CONNECTION_STRING, DATABASE_CONNECTION_STRING, or ConnectionStrings:DefaultConnection. On Render, sync render.yaml as a Blueprint or add DATABASE_URL manually from the scripture-circle-db Internal Database URL.");
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
        var port = uri.IsDefaultPort ? 5432 : uri.Port;

        return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }
}
