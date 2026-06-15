namespace ArchiveAccess.Api.Models;

public sealed class ApprovalStepRecord
{
    public int Id { get; init; }

    public int DocumentId { get; init; }

    public int ParticipantUserId { get; init; }

    public int StepOrder { get; init; }

    public string StatusCode { get; init; } = string.Empty;
}