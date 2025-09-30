using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodMD.Api.Dtos;
using PodMD.Application.Dtos;
using PodMD.Application.Interfaces;

namespace PodMD.Api.Controllers.V1;

[ApiController]
[Route("api/v1/clusters/{clusterId}/[controller]")]
[Authorize]
public class LogsController : ControllerBase
{
    private readonly IKubeLogService _logService;

    public LogsController(IKubeLogService logService)
    {
        _logService = logService;
    }

    [HttpPost("pods")]
    [ProducesResponseType(typeof(LogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetPodLogs(Guid clusterId, [FromBody] PodLogRequest request)
    {
        try
        {
            var parameters = new PodLogParameters(
                request.Namespace,
                request.PodName,
                request.ContainerName,
                request.TailLines,
                request.SinceSeconds,
                request.Previous,
                request.LimitBytes,
                request.IncludeDescription
            );

            var result = await _logService.GetPodLogsAsync(clusterId, parameters);
            var response = new LogResponse(result.Logs, result.Description, result.Metadata);
            return Ok(response);
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
                Title = "Invalid log request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Title = "Kubernetes API error",
                Detail = $"Failed to retrieve logs from Kubernetes API: {ex.Message}",
                Status = StatusCodes.Status502BadGateway,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }

    [HttpPost("deployments")]
    [ProducesResponseType(typeof(LogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetDeploymentLogs(Guid clusterId, [FromBody] DeploymentLogRequest request)
    {
        try
        {
            var parameters = new DeploymentLogParameters(
                request.Namespace,
                request.DeploymentName,
                request.Fallback,
                request.IncludeDescription
            );

            var result = await _logService.GetDeploymentLogsAsync(clusterId, parameters);
            var response = new LogResponse(result.Logs, result.Description, result.Metadata);
            return Ok(response);
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
                Title = "Invalid log request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Title = "Kubernetes API error",
                Detail = $"Failed to retrieve logs from Kubernetes API: {ex.Message}",
                Status = StatusCodes.Status502BadGateway,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }
}
