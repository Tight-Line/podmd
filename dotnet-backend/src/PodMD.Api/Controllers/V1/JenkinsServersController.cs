using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodMD.Application.Dtos;
using PodMD.Application.Interfaces;

namespace PodMD.Api.Controllers.V1;

[ApiController]
[Route("api/v1/jenkins-servers")]
[Authorize]
public class JenkinsServersController : ControllerBase
{
    private readonly IJenkinsServersService _jenkinsServersService;

    public JenkinsServersController(IJenkinsServersService jenkinsServersService)
    {
        _jenkinsServersService = jenkinsServersService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(JenkinsServersResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateJenkinsServersRequest request)
    {
        try
        {
            var response = await _jenkinsServersService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Jenkins server data",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An error occurred while creating the Jenkins server",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<JenkinsServersResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var servers = await _jenkinsServersService.GetAllAsync();
            return Ok(servers);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An error occurred while retrieving Jenkins servers",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JenkinsServersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var server = await _jenkinsServersService.GetByIdAsync(id);
            if (server == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Jenkins server not found",
                    Detail = $"No Jenkins server found with ID '{id}'",
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7807"
                });
            }
            return Ok(server);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An error occurred while retrieving the Jenkins server",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(JenkinsServersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJenkinsServersRequest request)
    {
        try
        {
            var response = await _jenkinsServersService.UpdateAsync(id, request);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Jenkins server not found",
                Detail = $"No Jenkins server found with ID '{id}'",
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Jenkins server data",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An error occurred while updating the Jenkins server",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _jenkinsServersService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Jenkins server not found",
                Detail = $"No Jenkins server found with ID '{id}'",
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An error occurred while deleting the Jenkins server",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }
}
