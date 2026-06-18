using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Application.Services;
using ScriptureCircle.Infrastructure;
using ScriptureCircle.Infrastructure.Authentication;

namespace ScriptureCircle.Api.Configuration;

internal static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(environment.ContentRootPath, ".keys")));

        services.AddCors(options =>
        {
            options.AddPolicy("Client", policy =>
                policy.WithOrigins(GetClientOrigins(configuration))
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        services.AddInfrastructure(configuration);
        services.AddApplicationServices();
        services.AddJwtAuthentication(configuration);
        services.AddAuthorization();

        return services;
    }

    private static string[] GetClientOrigins(IConfiguration configuration)
    {
        return (configuration.GetSection("ClientOrigins").Get<string[]>() ?? ["http://localhost:5173"])
            .Select(origin => origin.Trim().TrimEnd('/'))
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<NoteService>();
        services.AddScoped<LessonService>();
        services.AddScoped<SubscriptionService>();
        services.AddScoped<IAnnotationService, AnnotationService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<INotebookService, NotebookService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IFollowService, FollowService>();

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        return services;
    }
}
