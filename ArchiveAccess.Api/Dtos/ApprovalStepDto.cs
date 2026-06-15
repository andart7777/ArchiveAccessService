namespace ArchiveAccess.Api.Dtos;

public sealed class ApprovalStepDto
{
    public int Id { get; init; }

    public int StepOrder { get; init; }

    public string Department { get; init; } = string.Empty;

    public string Participant { get; init; } = string.Empty;

    public int ParticipantUserId { get; init; }

    public string Status { get; init; } = string.Empty;

    public string StatusCode { get; init; } = string.Empty;

    public string? DecisionComment { get; init; }

    public string? DecisionDate { get; init; }
}