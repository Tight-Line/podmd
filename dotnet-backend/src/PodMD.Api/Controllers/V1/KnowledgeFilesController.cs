using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodMD.Api.Controllers.V1;
using PodMD.Application.Dtos;
using PodMD.Application.Interfaces;

namespace PodMD.Api.Controllers.V1;

[ApiController]
[Route("api/v1/knowledgebases/{knowledgeBaseId:guid}/files")]
[Authorize]
public class KnowledgeFilesController : ControllerBase
{
    private readonly IKnowledgeFileService _fileService;
    private readonly IKnowledgeBasesService _knowledgeBasesService;
    private readonly ILogger<KnowledgeFilesController> _logger;

    public KnowledgeFilesController(
        IKnowledgeFileService fileService,
        IKnowledgeBasesService knowledgeBasesService,
        ILogger<KnowledgeFilesController> logger)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _knowledgeBasesService = knowledgeBasesService ?? throw new ArgumentNullException(nameof(knowledgeBasesService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [ProducesResponseType(typeof(IEnumerable<KnowledgeFileDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadFiles(Guid knowledgeBaseId, List<IFormFile> files)
    {
        try
        {
            // Validate knowledge base exists
            var knowledgeBase = await _knowledgeBasesService.GetByIdAsync(knowledgeBaseId);
            if (knowledgeBase == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Knowledge base not found",
                    Detail = $"No knowledge base found with ID '{knowledgeBaseId}'",
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7807"
                });
            }

            if (!files.Any())
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "No files provided",
                    Detail = "At least one file must be provided",
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7807"
                });
            }

            var uploadRequests = new List<CreateKnowledgeFileDto>();

            foreach (var file in files)
            {
                // Basic validation
                if (file.Length == 0)
                {
                    continue; // Skip empty files
                }

                var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var request = new CreateKnowledgeFileDto(
                    knowledgeBaseId,
                    file.FileName,
                    file.ContentType,
                    file.Length,
                    memoryStream);

                uploadRequests.Add(request);
            }

            var results = await _fileService.UploadFilesAsync(uploadRequests);

            _logger.LogInformation("Successfully uploaded {Count} files for knowledge base {KBId}", results.Count(), knowledgeBaseId);
            return CreatedAtAction(nameof(GetFiles), new { knowledgeBaseId }, results);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "File validation failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading files for knowledge base {KBId}", knowledgeBaseId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An error occurred while uploading files",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<KnowledgeFileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFiles(Guid knowledgeBaseId)
    {
        try
        {
            // Validate knowledge base exists
            var knowledgeBase = await _knowledgeBasesService.GetByIdAsync(knowledgeBaseId);
            if (knowledgeBase == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Knowledge base not found",
                    Detail = $"No knowledge base found with ID '{knowledgeBaseId}'",
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7807"
                });
            }

            var files = await _fileService.GetByKnowledgeBaseIdAsync(knowledgeBaseId);
            return Ok(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving files for knowledge base {KBId}", knowledgeBaseId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An error occurred while retrieving files",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }
}

// Separate controller for file-specific operations
[ApiController]
[Route("api/v1/files/{fileId:guid}")]
[Authorize]
public class KnowledgeFileController : ControllerBase
{
    private readonly IKnowledgeFileService _fileService;
    private readonly ILogger<KnowledgeFileController> _logger;

    public KnowledgeFileController(
        IKnowledgeFileService fileService,
        ILogger<KnowledgeFileController> logger)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPut("replace")]
    [ProducesResponseType(typeof(KnowledgeFileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReplaceFile(Guid fileId, IFormFile file, [FromQuery] string? newFileName = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "No file provided",
                    Detail = "A valid file must be provided for replacement",
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7807"
                });
            }

            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var request = new UpdateKnowledgeFileDto(
                file.FileName,  // Always use the uploaded file's name for replacement
                file.ContentType,
                file.Length,
                memoryStream);

            var result = await _fileService.ReplaceFileAsync(fileId, request);

            _logger.LogInformation("File replaced successfully: {FileId}", fileId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Knowledge file not found",
                Detail = $"No knowledge file found with ID '{fileId}'",
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "File replacement failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error replacing file {FileId}", fileId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An error occurred while replacing the file",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteFile(Guid fileId)
    {
        try
        {
            await _fileService.DeleteAsync(fileId);
            _logger.LogInformation("File deleted successfully: {FileId}", fileId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Knowledge file not found",
                Detail = $"No knowledge file found with ID '{fileId}'",
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId}", fileId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An error occurred while deleting the file",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }
}
