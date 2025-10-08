using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodMD.Application.Dtos;
using PodMD.Application.Interfaces;
using System.Security.Claims;

namespace PodMD.Api.Controllers.V1;

[ApiController]
[Route("api/apikeys")]
[Authorize]
public class ApiKeysController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeysController(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateApiKeyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(CreateApiKeyRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "User not authenticated",
                    Detail = "Unable to identify the authenticated user",
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://tools.ietf.org/html/rfc7807"
                });
            }

            var response = await _apiKeyService.CreateApiKeyAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (Exception ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Failed to create API key",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ApiKeyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "User not authenticated",
                    Detail = "Unable to identify the authenticated user",
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://tools.ietf.org/html/rfc7807"
                });
            }

            var apiKeys = await _apiKeyService.GetAllByUserIdAsync(userId);
            return Ok(apiKeys);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An error occurred while retrieving API keys",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiKeyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "User not authenticated",
                    Detail = "Unable to identify the authenticated user",
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://tools.ietf.org/html/rfc7807"
                });
            }

            var apiKey = await _apiKeyService.GetByIdAsync(id, userId);
            if (apiKey == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "API key not found",
                    Detail = "The specified API key does not exist or does not belong to the current user",
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7807"
                });
            }

            return Ok(apiKey);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An error occurred while retrieving the API key",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiKeyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, UpdateApiKeyRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "User not authenticated",
                    Detail = "Unable to identify the authenticated user",
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://tools.ietf.org/html/rfc7807"
                });
            }

            var updatedApiKey = await _apiKeyService.UpdateApiKeyAsync(id, userId, request);
            if (updatedApiKey == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "API key not found",
                    Detail = "The specified API key does not exist or does not belong to the current user",
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7807"
                });
            }

            return Ok(updatedApiKey);
        }
        catch (Exception ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Failed to update API key",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "User not authenticated",
                    Detail = "Unable to identify the authenticated user",
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://tools.ietf.org/html/rfc7807"
                });
            }

            await _apiKeyService.DeleteApiKeyAsync(id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An error occurred while deleting the API key",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }
}
