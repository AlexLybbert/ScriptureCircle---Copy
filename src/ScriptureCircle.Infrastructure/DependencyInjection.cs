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
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            }
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IAuthService, AuthService>();
        services.AddHttpClient<OpenScriptureService>();
        services.AddScoped<IScriptureProvider>(sp => sp.GetRequiredService<OpenScriptureService>());
        services.AddScoped<IScriptureService>(sp => sp.GetRequiredService<OpenScriptureService>());

        return services;
    }
}
