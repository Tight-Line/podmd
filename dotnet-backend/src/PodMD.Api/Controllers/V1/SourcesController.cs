using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodMD.Application.Dtos;
using PodMD.Application.Interfaces;

namespace PodMD.Api.Controllers.V1;

[ApiController]
[Route("api/v1/sources")]
[Authorize]
public class SourcesController : ControllerBase
{
    private readonly IKnowledgeBasesService _knowledgeBasesService;

    public SourcesController(IKnowledgeBasesService knowledgeBasesService)
    {
        _knowledgeBasesService = knowledgeBasesService;
    }

    [HttpGet("{sourceId:guid}/knowledge-bases")]
    [ProducesResponseType(typeof(IEnumerable<KnowledgeBaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetKnowledgeBasesForSource(Guid sourceId)
    {
        try
        {
            var knowledgeBases = await _knowledgeBasesService.GetKnowledgeBasesForSourceAsync(sourceId);
            return Ok(knowledgeBases);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An error occurred while retrieving knowledge bases for the source",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }

    [HttpPost("{sourceId:guid}/knowledge-bases/{knowledgeBaseId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AssociateSourceWithKnowledgeBase(Guid sourceId, Guid knowledgeBaseId)
    {
        try
        {
            await _knowledgeBasesService.AddSourceToKnowledgeBaseAsync(knowledgeBaseId, sourceId);
            return Ok(new { Message = "Source associated with knowledge base successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Entity not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An error occurred while associating the source with knowledge base",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }

    [HttpDelete("{sourceId:guid}/knowledge-bases/{knowledgeBaseId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveSourceFromKnowledgeBase(Guid sourceId, Guid knowledgeBaseId)
    {
        try
        {
            await _knowledgeBasesService.RemoveSourceFromKnowledgeBaseAsync(knowledgeBaseId, sourceId);
            return Ok(new { Message = "Source removed from knowledge base successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Entity not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An error occurred while removing the source from knowledge base",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }
}
