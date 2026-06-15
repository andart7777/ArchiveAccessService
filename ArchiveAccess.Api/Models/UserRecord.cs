namespace ArchiveAccess.Api.Models;

public sealed class UserRecord
{
    public int Id { get; init; }

    public string Username { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string PasswordHash { get; init; } = string.Empty;

    public string RoleCode { get; init; } = string.Empty;

    public string RoleName { get; init; } = string.Empty;

    public string Department { get; init; } = string.Empty;
}