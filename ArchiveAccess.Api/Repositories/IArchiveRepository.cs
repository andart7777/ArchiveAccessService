using ArchiveAccess.Api.Dtos;
using ArchiveAccess.Api.Models;

namespace ArchiveAccess.Api.Repositories;

public interface IArchiveRepository
{
    Task<UserRecord?> GetUserByUsernameAsync(string username);

    Task<UserRecord?> GetUserByIdAsync(int userId);

    Task<IReadOnlyList<DocumentListItemDto>> GetDocumentsAsync();

    Task<DocumentCardDto?> GetDocumentCardAsync(int documentId);

    Task<IReadOnlyList<ApprovalStepDto>> GetApprovalStepsAsync(int documentId);

    Task<ApprovalStepRecord?> GetCurrentPendingStepAsync(int documentId);

    Task UpdateApprovalStepAsync(
        int stepId,
        string decisionStatusCode,
        string? comment);

    Task UpdateDocumentStatusAsync(
        int documentId,
        string statusCode);

    Task<IReadOnlyList<DirectoryValueDto>> GetDocumentTypesAsync();

    Task<IReadOnlyList<DirectoryValueDto>> GetDocumentStatusesAsync();

    Task<int> CountDocumentsAsync();

    Task AddAuditLogAsync(
        int? userId,
        string action,
        string entityType,
        int? entityId,
        string? details);
}