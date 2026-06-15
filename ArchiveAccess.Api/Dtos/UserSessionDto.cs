namespace ArchiveAccess.Api.Dtos;

public sealed record UserSessionDto(
    int Id,
    string Username,
    string FullName,
    string RoleCode,
    string RoleName,
    string Department
);