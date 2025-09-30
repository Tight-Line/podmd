using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodMD.Api.Dtos;
using PodMD.Application.Analysis;

namespace PodMD.Api.Controllers;

[ApiController]
[Route("api/v1/clusters/{clusterId}/[controller]")]
[Authorize]
public class AnalysisController : ControllerBase
{
    private readonly IAnalysisService _analysisService;

    public AnalysisController(IAnalysisService analysisService)
    {
        _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
    }

    [HttpPost("pods")]
    [ProducesResponseType(typeof(AnalysisResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> AnalyzePodLogs(Guid clusterId, [FromBody] PodAnalysisRequestDto request)
    {
        try
        {
            var result = await _analysisService.AnalyzePodLogsAsync(
                clusterId,
                request.Namespace,
                request.PodName,
                request.ContainerName,
                request.TailLines,
                request.SinceSeconds,
                request.Previous,
                request.LimitBytes,
                request.IncludeDescription,
                HttpContext.RequestAborted);

            var response = new AnalysisResponseDto
            {
                Success = result.Success,
                Message = result.Message
            };

            if (result.Success && result.Data is AnalysisResult analysisResult)
            {
                response.Data = new AnalysisDataDto
                {
                    Errors = analysisResult.Errors.Select(e => new LogErrorDto
                    {
                        GeneralMessage = e.GeneralMessage,
                        Occurrences = e.Occurrences,
                        Solutions = e.Solutions.Select(s => new SolutionDto
                        {
                            Description = s.Description,
                            Steps = s.Steps.Select(step => new StepDto
                            {
                                Title = step.Title,
                                Explanation = step.Explanation,
                                Command = step.Command
                            }).ToList()
                        }).ToList()
                    }).ToList()
                };
            }

            return Ok(response);
        }
        catch (LlmAnalysisException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Title = "LLM Analysis Error",
                Detail = ex.Message,
                Status = StatusCodes.Status502BadGateway,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Pod not found",
                Detail = $"Pod '{request.PodName}' not found in namespace '{request.Namespace}'",
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid analysis request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Title = "Analysis service error",
                Detail = $"Failed to analyze logs: {ex.Message}",
                Status = StatusCodes.Status502BadGateway,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }

    [HttpPost("deployments")]
    [ProducesResponseType(typeof(AnalysisResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> AnalyzeDeploymentLogs(Guid clusterId, [FromBody] DeploymentAnalysisRequestDto request)
    {
        try
        {
            var result = await _analysisService.AnalyzeDeploymentLogsAsync(
                clusterId,
                request.Namespace,
                request.DeploymentName,
                request.Fallback,
                request.IncludeDescription,
                HttpContext.RequestAborted);

            var response = new AnalysisResponseDto
            {
                Success = result.Success,
                Message = result.Message
            };

            if (result.Success && result.Data is AnalysisResult analysisResult)
            {
                response.Data = new AnalysisDataDto
                {
                    Errors = analysisResult.Errors.Select(e => new LogErrorDto
                    {
                        GeneralMessage = e.GeneralMessage,
                        Occurrences = e.Occurrences,
                        Solutions = e.Solutions.Select(s => new SolutionDto
                        {
                            Description = s.Description,
                            Steps = s.Steps.Select(step => new StepDto
                            {
                                Title = step.Title,
                                Explanation = step.Explanation,
                                Command = step.Command
                            }).ToList()
                        }).ToList()
                    }).ToList()
                };
            }

            return Ok(response);
        }
        catch (LlmAnalysisException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Title = "LLM Analysis Error",
                Detail = ex.Message,
                Status = StatusCodes.Status502BadGateway,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Deployment not found",
                Detail = $"Deployment '{request.DeploymentName}' not found in namespace '{request.Namespace}'",
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid analysis request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Title = "Analysis service error",
                Detail = $"Failed to analyze logs: {ex.Message}",
                Status = StatusCodes.Status502BadGateway,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }
}
