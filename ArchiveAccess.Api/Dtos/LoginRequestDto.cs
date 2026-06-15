namespace ArchiveAccess.Api.Dtos;

public sealed record LoginRequestDto(
    string Username,
    string Password
);