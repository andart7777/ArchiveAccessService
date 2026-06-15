using ArchiveAccess.Api.Dtos;

namespace ArchiveAccess.Api.Services;

public interface IDocumentService
{
    Task<IReadOnlyList<DocumentListItemDto>> GetDocumentsAsync(
        string? query,
        string? type,
        string? status,
        string? dateFrom,
        string? dateTo,
        int userId);

    Task<DocumentCardDto?> GetDocumentCardAsync(
        int documentId,
        int userId,
        string roleCode);

    Task<ApprovalResultDto> MakeDecisionAsync(
        int documentId,
        int userId,
        string roleCode,
        ApproveRequestDto request);

    Task<IReadOnlyList<DirectoryValueDto>> GetDocumentTypesAsync();

    Task<IReadOnlyList<DirectoryValueDto>> GetDocumentStatusesAsync();

    Task<int> GetDocumentsCountAsync();
}