namespace ArchiveAccess.Api.Services;

public interface IAuditService
{
    Task WriteAsync(
        int? userId,
        string action,
        string entityType,
        int? entityId,
        string? details);
}