using ArchiveAccess.Api.Dtos;

namespace ArchiveAccess.Api.Services;

public interface IAuthService
{
    Task<UserSessionDto?> ValidateCredentialsAsync(string username, string password);

    string CreateToken(UserSessionDto user);
}