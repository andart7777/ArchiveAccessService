namespace ArchiveAccess.Api.Dtos;

public sealed record LoginResponseDto(
    string Token,
    UserSessionDto User
);