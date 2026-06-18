using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Domain.Entities;
using ScriptureCircle.Shared.Auth;

namespace ScriptureCircle.Infrastructure.Authentication;

public sealed class AuthService(IAppDbContext db, IOptions<JwtOptions> options) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            throw new InvalidOperationException("An account already exists for this email.");
        }

        var user = new AppUser
        {
            DisplayName = request.DisplayName.Trim(),
            Email = email,
            ProfileSlug = await CreateUniqueProfileSlugAsync(request.DisplayName, email, cancellationToken),
            PasswordHash = PasswordHasher.Hash(request.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(user);
    }

    private async Task<string> CreateUniqueProfileSlugAsync(string displayName, string email, CancellationToken cancellationToken)
    {
        var baseSlug = CreateProfileSlug(displayName, email);
        var slug = baseSlug;
        var suffix = 2;
        while (await db.Users.AnyAsync(u => u.ProfileSlug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix += 1;
        }

        return slug;
    }

    private static string CreateProfileSlug(string displayName, string email)
    {
        var source = string.IsNullOrWhiteSpace(displayName) ? email.Split('@')[0] : displayName;
        var slug = new string(source.Trim().ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray());
        slug = string.Join('-', slug.Split('-', StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N")[..12] : slug;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return ToResponse(user);
    }

    private AuthResponse ToResponse(AppUser user) => new(user.Id, user.DisplayName, user.Email, CreateToken(user), user.ProfileSlug);

    private string CreateToken(AppUser user)
    {
        var jwt = options.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.DisplayName)
        };

        var token = new JwtSecurityToken(
            jwt.Issuer,
            jwt.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(jwt.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
