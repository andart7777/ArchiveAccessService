namespace ArchiveAccess.Api.Dtos;

public sealed class DocumentCardDto
{
    public int Id { get; init; }

    public string Number { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string StatusCode { get; init; } = string.Empty;

    public string Author { get; init; } = string.Empty;

    public string Department { get; init; } = string.Empty;

    public string SourceSystem { get; init; } = string.Empty;

    public string CreatedAt { get; init; } = string.Empty;

    public string? FileName { get; init; }

    public IReadOnlyList<ApprovalStepDto> ApprovalSteps { get; init; } = [];

    public IReadOnlyList<string> AvailableActions { get; init; } = [];
}