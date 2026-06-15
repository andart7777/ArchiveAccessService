namespace ArchiveAccess.Api.Dtos;

public sealed record ApprovalResultDto(
    int DocumentId,
    string Decision,
    string NewStatus,
    string Message
);