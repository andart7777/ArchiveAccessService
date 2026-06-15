using System.Security.Claims;
using ArchiveAccess.Api.Dtos;
using ArchiveAccess.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchiveAccess.Api.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public sealed class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DocumentListItemDto>>> GetDocuments(
        [FromQuery] string? query,
        [FromQuery] string? type,
        [FromQuery] string? status,
        [FromQuery] string? dateFrom,
        [FromQuery] string? dateTo)
    {
        var userId = GetCurrentUserId();

        var documents = await _documentService.GetDocumentsAsync(
            query,
            type,
            status,
            dateFrom,
            dateTo,
            userId);

        return Ok(documents);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DocumentCardDto>> GetDocumentCard(int id)
    {
        var userId = GetCurrentUserId();
        var roleCode = GetCurrentRoleCode();

        var document = await _documentService.GetDocumentCardAsync(
            id,
            userId,
            roleCode);

        if (document is null)
        {
            return NotFound(new { error = "Документ не найден." });
        }

        return Ok(document);
    }

    [HttpPost("{id:int}/decision")]
    public async Task<ActionResult<ApprovalResultDto>> MakeDecision(
        int id,
        ApproveRequestDto request)
    {
        var userId = GetCurrentUserId();
        var roleCode = GetCurrentRoleCode();

        var result = await _documentService.MakeDecisionAsync(
            id,
            userId,
            roleCode,
            request);

        return Ok(result);
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("Пользователь не определен.");
        }

        return userId;
    }

    private string GetCurrentRoleCode()
    {
        return User.FindFirstValue(ClaimTypes.Role) ?? "user";
    }
}