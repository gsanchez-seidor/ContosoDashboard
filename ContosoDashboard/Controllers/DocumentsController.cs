using System.Security.Claims;
using ContosoDashboard.Services;
using ContosoDashboard.Services.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContosoDashboard.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? category,
        [FromQuery] int? projectId,
        [FromQuery] DateTime? fromDateUtc,
        [FromQuery] DateTime? toDateUtc,
        [FromQuery] string? sort,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized();
        }

        var documents = await _documentService.GetVisibleDocumentsAsync(
            userId,
            new DocumentListRequest
            {
                Category = category,
                ProjectId = projectId,
                FromDateUtc = fromDateUtc,
                ToDateUtc = toDateUtc,
                Sort = string.IsNullOrWhiteSpace(sort) ? "uploadDate" : sort
            },
            cancellationToken);

        return Ok(documents);
    }

    [HttpPost]
    [RequestSizeLimit(26 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] UploadDocumentApiRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized();
        }

        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest("File is required.");
        }

        await using var stream = request.File.OpenReadStream();

        try
        {
            var result = await _documentService.UploadDocumentAsync(
                userId,
                stream,
                request.File.FileName,
                request.File.ContentType,
                request.File.Length,
                new UploadDocumentRequest
                {
                    Title = request.Title,
                    Category = request.Category,
                    Description = request.Description,
                    ProjectId = request.ProjectId,
                    Tags = ParseTags(request.Tags)
                },
                cancellationToken);

            return Created($"/api/documents/{result.DocumentId}", result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(new List<DocumentSummaryDto>());
        }

        var results = await _documentService.SearchDocumentsAsync(userId, q, cancellationToken);
        return Ok(results);
    }

    [HttpGet("{documentId:int}")]
    public async Task<IActionResult> GetById(int documentId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized();
        }

        var document = await _documentService.GetDocumentDetailsAsync(documentId, userId, cancellationToken);
        return document == null ? NotFound() : Ok(document);
    }

    [HttpPatch("{documentId:int}")]
    public async Task<IActionResult> UpdateMetadata(int documentId, [FromBody] UpdateDocumentMetadataRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _documentService.UpdateMetadataAsync(userId, documentId, request, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("Version conflict", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(ex.Message);
            }

            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{documentId:int}/replace")]
    [RequestSizeLimit(26 * 1024 * 1024)]
    public async Task<IActionResult> Replace(int documentId, [FromForm] ReplaceDocumentApiRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized();
        }

        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest("File is required.");
        }

        await using var stream = request.File.OpenReadStream();
        try
        {
            var result = await _documentService.ReplaceDocumentAsync(
                userId,
                documentId,
                stream,
                request.File.FileName,
                request.File.ContentType,
                request.File.Length,
                request.ExpectedVersionToken,
                cancellationToken);

            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("Version conflict", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(ex.Message);
            }

            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{documentId:int}/download")]
    public async Task<IActionResult> Download(int documentId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _documentService.DownloadAsync(userId, documentId, false, cancellationToken);
            Response.Headers["X-Download-Options"] = "noopen";
            return File(result.Stream, result.ContentType, result.FileName, enableRangeProcessing: false);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{documentId:int}/preview")]
    public async Task<IActionResult> Preview(int documentId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _documentService.DownloadAsync(userId, documentId, true, cancellationToken);
            Response.Headers["Content-Disposition"] = "inline";
            return File(result.Stream, result.ContentType, enableRangeProcessing: false);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{documentId:int}")]
    public async Task<IActionResult> Delete(int documentId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized();
        }

        try
        {
            var deleted = await _documentService.SoftDeleteAsync(userId, documentId, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{documentId:int}/shares")]
    public async Task<IActionResult> Share(int documentId, [FromBody] ShareDocumentApiRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _documentService.ShareAsync(userId, documentId, request.RecipientUserIds, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("project/{projectId:int}")]
    public async Task<IActionResult> ProjectDocuments(int projectId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized();
        }

        var result = await _documentService.GetProjectDocumentsAsync(userId, projectId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("shared")]
    public async Task<IActionResult> Shared(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized();
        }

        var result = await _documentService.GetSharedDocumentsAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("tasks/{taskId:int}")]
    public async Task<IActionResult> TaskDocuments(int taskId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized();
        }

        var result = await _documentService.GetTaskDocumentsAsync(userId, taskId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("tasks/{taskId:int}/links/{documentId:int}")]
    public async Task<IActionResult> LinkTaskDocument(int taskId, int documentId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized();
        }

        var linked = await _documentService.LinkDocumentToTaskAsync(userId, taskId, documentId, cancellationToken);
        return linked ? Ok() : Forbid();
    }

    private int GetCurrentUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claimValue, out var userId) ? userId : 0;
    }

    private static List<string> ParseTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
        {
            return [];
        }

        return tags
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
