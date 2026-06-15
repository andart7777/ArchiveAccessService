using ArchiveAccess.Api.Repositories;

namespace ArchiveAccess.Api.Services;

public sealed class AuditService : IAuditService
{
    private readonly IArchiveRepository _repository;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IArchiveRepository repository,
        ILogger<AuditService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task WriteAsync(
        int? userId,
        string action,
        string entityType,
        int? entityId,
        string? details)
    {
        await _repository.AddAuditLogAsync(userId, action, entityType, entityId, details);

        _logger.LogInformation(
            "Audit event: user={UserId}, action={Action}, entity={EntityType}, entityId={EntityId}",
            userId,
            action,
            entityType,
            entityId);
    }
}