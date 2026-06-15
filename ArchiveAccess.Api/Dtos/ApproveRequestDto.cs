namespace ArchiveAccess.Api.Dtos;

public sealed record ApproveRequestDto(
    string Decision,
    string? Comment
);