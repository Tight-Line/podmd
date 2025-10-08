using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PodMD.Application.Dtos;
using PodMD.Application.Interfaces;
using PodMD.Domain.Entities;
using System.Security.Claims;

namespace PodMD.Api.Controllers.V1;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAuthService _authService;
    private readonly IApiKeyAuthenticationService _apiKeyAuthenticationService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IAuthService authService,
        IApiKeyAuthenticationService apiKeyAuthenticationService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _authService = authService;
        _apiKeyAuthenticationService = apiKeyAuthenticationService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Registration failed",
                Detail = string.Join("; ", result.Errors.Select(e => e.Description)),
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }

        return StatusCode(StatusCodes.Status201Created, new { Message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _signInManager.PasswordSignInAsync(
            request.Email,
            request.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Login failed",
                Detail = "Invalid email or password",
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Login failed",
                Detail = "User not found",
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }

        var token = await _authService.GenerateTokenAsync(user);
        return Ok(token);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Authentication required",
                Detail = "User ID not found in token",
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = "The authenticated user no longer exists",
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        return Ok(userDto);
    }

    [HttpPost("validate-key")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ValidateApiKey()
    {
        try
        {
            // Get API key from X-API-Key header
            if (!Request.Headers.TryGetValue("X-API-Key", out var apiKeyValues))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "API key required",
                    Detail = "X-API-Key header is required for this endpoint",
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://tools.ietf.org/html/rfc7807"
                });
            }

            var apiKey = apiKeyValues.ToString();
            var userDto = await _apiKeyAuthenticationService.AuthenticateApiKeyAsync(apiKey);

            if (userDto == null)
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Invalid or revoked API key",
                    Detail = "The provided API key is invalid, revoked, or has exceeded usage limits",
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://tools.ietf.org/html/rfc7807"
                });
            }

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An error occurred while validating the API key",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807"
            });
        }
    }
}
