using ArchiveAccess.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArchiveAccess.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IConfiguration _configuration;

    public HealthController(
        IDocumentService documentService,
        IConfiguration configuration)
    {
        _documentService = documentService;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var documentsCount = await _documentService.GetDocumentsCountAsync();

        return Ok(new
        {
            status = "healthy",
            documentsCount,
            sources = new
            {
                documentsApi = _configuration["ExternalSources:DocumentsApi"],
                approvalsApi = _configuration["ExternalSources:ApprovalsApi"]
            }
        });
    }
}