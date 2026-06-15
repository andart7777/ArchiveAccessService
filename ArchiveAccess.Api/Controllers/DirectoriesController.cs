using ArchiveAccess.Api.Dtos;
using ArchiveAccess.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchiveAccess.Api.Controllers;

[ApiController]
[Route("api/directories")]
[Authorize]
public sealed class DirectoriesController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DirectoriesController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet("document-types")]
    public async Task<ActionResult<IReadOnlyList<DirectoryValueDto>>> GetDocumentTypes()
    {
        var result = await _documentService.GetDocumentTypesAsync();

        return Ok(result);
    }

    [HttpGet("document-statuses")]
    public async Task<ActionResult<IReadOnlyList<DirectoryValueDto>>> GetDocumentStatuses()
    {
        var result = await _documentService.GetDocumentStatusesAsync();

        return Ok(result);
    }
}