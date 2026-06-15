using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ArchiveAccess.Api.Dtos;
using ArchiveAccess.Api.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace ArchiveAccess.Api.Services;

public sealed class AuthService : IAuthService
{
    private readonly IArchiveRepository _repository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IArchiveRepository repository,
        IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<UserSessionDto?> ValidateCredentialsAsync(string username, string password)
    {
        var user = await _repository.GetUserByUsernameAsync(username);

        if (user is null)
        {
            return null;
        }

        var passwordHash = ComputeSha256(password);

        if (!string.Equals(user.PasswordHash, passwordHash, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return new UserSessionDto(
            user.Id,
            user.Username,
            user.FullName,
            user.RoleCode,
            user.RoleName,
            user.Department);
    }

    public string CreateToken(UserSessionDto user)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");

        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.GivenName, user.FullName),
            new(ClaimTypes.Role, user.RoleCode),
            new("role_name", user.RoleName),
            new("department", user.Department)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string ComputeSha256(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}