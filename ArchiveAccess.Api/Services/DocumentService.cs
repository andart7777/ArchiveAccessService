using ArchiveAccess.Api.Dtos;
using ArchiveAccess.Api.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace ArchiveAccess.Api.Services;

public sealed class DocumentService : IDocumentService
{
    private readonly IArchiveRepository _repository;
    private readonly IAuditService _auditService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IArchiveRepository repository,
        IAuditService auditService,
        IMemoryCache cache,
        ILogger<DocumentService> logger)
    {
        _repository = repository;
        _auditService = auditService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DocumentListItemDto>> GetDocumentsAsync(
        string? query,
        string? type,
        string? status,
        string? dateFrom,
        string? dateTo,
        int userId)
    {
        var documents = await _repository.GetDocumentsAsync();

        var filtered = documents.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.Trim();

            if (normalizedQuery.Length < 2)
            {
                throw new ArgumentException("Поисковый запрос должен содержать не менее двух символов.");
            }

            filtered = filtered.Where(document =>
                document.Number.Contains(normalizedQuery, StringComparison.CurrentCultureIgnoreCase)
                || document.Title.Contains(normalizedQuery, StringComparison.CurrentCultureIgnoreCase)
                || document.Author.Contains(normalizedQuery, StringComparison.CurrentCultureIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            filtered = filtered.Where(document =>
                document.Type.Equals(type, StringComparison.CurrentCultureIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filtered = filtered.Where(document =>
                document.StatusCode.Equals(status, StringComparison.OrdinalIgnoreCase)
                || document.Status.Equals(status, StringComparison.CurrentCultureIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(dateFrom))
        {
            var from = ParseDate(dateFrom, "Дата начала периода указана некорректно.");

            filtered = filtered.Where(document =>
                DateOnly.Parse(document.CreatedAt) >= from);
        }

        if (!string.IsNullOrWhiteSpace(dateTo))
        {
            var to = ParseDate(dateTo, "Дата окончания периода указана некорректно.");

            filtered = filtered.Where(document =>
                DateOnly.Parse(document.CreatedAt) <= to);
        }

        var result = filtered.ToList();

        await _auditService.WriteAsync(
            userId,
            "documents.search",
            "Document",
            null,
            $"Returned {result.Count} documents.");

        _logger.LogInformation("Documents were requested by user {UserId}. Count={Count}", userId, result.Count);

        return result;
    }

    public async Task<DocumentCardDto?> GetDocumentCardAsync(
        int documentId,
        int userId,
        string roleCode)
    {
        var card = await _repository.GetDocumentCardAsync(documentId);

        if (card is null)
        {
            return null;
        }

        var actions = BuildAvailableActions(card, userId, roleCode);

        await _auditService.WriteAsync(
            userId,
            "documents.card.open",
            "Document",
            documentId,
            $"Document card {documentId} was opened.");

        return new DocumentCardDto
        {
            Id = card.Id,
            Number = card.Number,
            Title = card.Title,
            Type = card.Type,
            Status = card.Status,
            StatusCode = card.StatusCode,
            Author = card.Author,
            Department = card.Department,
            SourceSystem = card.SourceSystem,
            CreatedAt = card.CreatedAt,
            FileName = card.FileName,
            ApprovalSteps = card.ApprovalSteps,
            AvailableActions = actions
        };
    }

    public async Task<ApprovalResultDto> MakeDecisionAsync(
        int documentId,
        int userId,
        string roleCode,
        ApproveRequestDto request)
    {
        if (roleCode is not "approver" and not "admin")
        {
            throw new UnauthorizedAccessException("У пользователя нет прав на согласование документа.");
        }

        var decision = request.Decision.Trim().ToLowerInvariant();

        if (decision is not "approve" and not "reject" and not "revision")
        {
            throw new ArgumentException("Недопустимое решение. Используйте approve, reject или revision.");
        }

        var currentStep = await _repository.GetCurrentPendingStepAsync(documentId);

        if (currentStep is null)
        {
            throw new InvalidOperationException("Для документа отсутствует активный этап согласования.");
        }

        if (roleCode != "admin" && currentStep.ParticipantUserId != userId)
        {
            throw new UnauthorizedAccessException("Пользователь не является участником текущего этапа согласования.");
        }

        var stepStatusCode = decision switch
        {
            "approve" => "approved",
            "reject" => "rejected",
            "revision" => "revision",
            _ => throw new ArgumentException("Недопустимое решение.")
        };

        var documentStatusCode = decision switch
        {
            "approve" => "approved",
            "reject" => "rejected",
            "revision" => "revision",
            _ => throw new ArgumentException("Недопустимое решение.")
        };

        await _repository.UpdateApprovalStepAsync(
            currentStep.Id,
            stepStatusCode,
            request.Comment);

        await _repository.UpdateDocumentStatusAsync(
            documentId,
            documentStatusCode);

        await _auditService.WriteAsync(
            userId,
            $"documents.decision.{decision}",
            "Document",
            documentId,
            request.Comment);

        return new ApprovalResultDto(
            documentId,
            decision,
            documentStatusCode,
            "Решение по документу сохранено.");
    }

    public async Task<IReadOnlyList<DirectoryValueDto>> GetDocumentTypesAsync()
    {
        return await _cache.GetOrCreateAsync("document-types", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _repository.GetDocumentTypesAsync();
        }) ?? [];
    }

    public async Task<IReadOnlyList<DirectoryValueDto>> GetDocumentStatusesAsync()
    {
        return await _cache.GetOrCreateAsync("document-statuses", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _repository.GetDocumentStatusesAsync();
        }) ?? [];
    }

    public async Task<int> GetDocumentsCountAsync()
    {
        return await _repository.CountDocumentsAsync();
    }

    private static IReadOnlyList<string> BuildAvailableActions(
        DocumentCardDto card,
        int userId,
        string roleCode)
    {
        var actions = new List<string> { "view" };

        if (roleCode == "admin")
        {
            actions.Add("edit");
            actions.Add("approve");
            actions.Add("reject");
            actions.Add("revision");

            return actions;
        }

        if (roleCode == "author" && card.StatusCode is "created" or "revision")
        {
            actions.Add("edit");
        }

        var activeStep = card.ApprovalSteps
            .FirstOrDefault(step => step.StatusCode == "in_approval");

        if (roleCode == "approver"
            && activeStep is not null
            && activeStep.ParticipantUserId == userId)
        {
            actions.Add("approve");
            actions.Add("reject");
            actions.Add("revision");
        }

        return actions;
    }

    private static DateOnly ParseDate(string value, string errorMessage)
    {
        if (!DateOnly.TryParse(value, out var date))
        {
            throw new ArgumentException(errorMessage);
        }

        return date;
    }
}