# JWT Authentication Implementation Task

Follow your custom instructions.

## Overview

Implement secure user authentication using JSON Web Tokens (JWT) to enable clients to obtain tokens upon login and use them for accessing protected API endpoints. This follows Clean Architecture principles and integrates with the existing ASP.NET Core Identity system.

## Objective

Enable secure user authentication using JSON Web Tokens (JWT) so that clients can obtain a token upon login and use it to access protected API endpoints.

## Scope

**Must Include:**

- Username/password authentication
- JWT token issuance upon successful login
- JWT validation for protected endpoints ([Authorize])
- Swagger UI support for entering JWT tokens via the Authorize button

**Excluded:**

- Other authentication types (OAuth, social login, etc.)

## Entity Specifications

### ApplicationUser Entity

| Field Name    | Data Type      | Required | Constraints        | Description                           |
| ------------- | -------------- | -------- | ------------------ | ------------------------------------- |
| Id            | string         | Yes      | Primary Key        | Unique identifier for the user        |
| UserName      | string         | Yes      | Unique             | Username for authentication           |
| Email         | string         | No       | Unique             | User's email address                  |
| PasswordHash  | string         | Yes      | -                  | Hashed password                       |
| SecurityStamp | string         | Yes      | -                  | Security stamp for token invalidation |
| FirstName     | string         | Yes      | -                  | User's first name                     |
| LastName      | string         | Yes      | -                  | User's last name                      |
| CreatedAt     | DateTimeOffset | Yes      | Auto-set to UtcNow | When the user was created             |
| UpdatedAt     | DateTimeOffset | No       | -                  | When the user was last updated        |

**Notes:**

- Inherits from ASP.NET Core Identity `IdentityUser`
- Additional custom fields for first/last name and audit timestamps
- Database schema already exists via Identity tables

## DTO Specifications

### LoginRequest

- email (string, required) - User's email for authentication
- password (string, required) - User's password

### RegisterRequest (Create)

- firstName (string, required) - User's first name
- lastName (string, required) - User's last name
- email (string, required) - User's email address
- password (string, required) - User's password

### AuthResponse

- token (string) - JWT access token
- expiration (DateTime) - Token expiration timestamp

### UpdateUserRequest (Update)

- firstName (string, optional) - User's first name
- lastName (string, optional) - User's last name
- email (string, optional) - User's email address

### UserDto (Read)

- id (string) - User identifier
- firstName (string) - User's first name
- lastName (string) - User's last name
- email (string) - User's email address
- createdAt (DateTimeOffset) - Account creation timestamp
- updatedAt (DateTimeOffset?) - Last update timestamp

**Validation Rules:**

- Email format validation
- Password strength requirements (to be configured)
- Required fields must not be null/empty

## API Endpoint Specifications

| Method | Endpoint           | Description                                                                | Auth Required | Request Body    | Response                   | Status Codes                 |
| ------ | ------------------ | -------------------------------------------------------------------------- | ------------- | --------------- | -------------------------- | ---------------------------- |
| POST   | /api/auth/login    | Authenticate user with email/password and return JWT token with expiration | No            | LoginRequest    | AuthResponse               | 200 OK, 401 Unauthorized     |
| POST   | /api/auth/register | Register a new user account                                                | No            | RegisterRequest | UserDto or success message | 201 Created, 400 Bad Request |
| GET    | /api/auth/me       | Get current authenticated user's information                               | Yes (JWT)     | -               | UserDto                    | 200 OK, 401 Unauthorized     |

**Constraints:**

- JWT tokens must include user claims (id, email, roles)
- Token expiration should be configurable (default: 24 hours)
- Protected endpoints require valid JWT in Authorization header
- Swagger UI must support JWT Bearer token input

## File Structure

### New Files to Create

1. `src/PodMD.Application/Dtos/AuthResponse.cs`

   - JWT response DTO with token and expiration

2. `src/PodMD.Application/Dtos/UserDto.cs`

   - User data transfer object for API responses

3. `src/PodMD.Application/Dtos/UpdateUserRequest.cs`

   - User update request DTO

4. `src/PodMD.Application/Services/AuthService.cs`

   - Authentication service for token generation and validation

5. `src/PodMD.Api/Controllers/AuthController.cs`
   - Authentication API endpoints (modify existing)

### Files to Modify

1. `src/PodMD.Application/Dtos/LoginRequest.cs`

   - Consider changing Email to UserName if needed for consistency

2. `src/PodMD.Api/Program.cs`

   - Add JWT authentication middleware
   - Configure JWT bearer options
   - Register AuthService in DI container

3. `src/PodMD.Api/appsettings.json`

   - Add JWT configuration section

4. `src/PodMD.Api/Controllers/V1/AuthController.cs`
   - Complete login method to generate JWT
   - Add GET /me endpoint

## Implementation Guidance

### JWT Configuration

Add to `appsettings.json`:

```json
{
  "Jwt": {
    "Secret": "your-256-bit-secret-here",
    "Issuer": "PodMD.Api",
    "Audience": "PodMD.Client",
    "ExpirationHours": 24
  }
}
```

### Program.cs Changes

1. Add JWT authentication services:

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
    };
});
```

2. Register AuthService:

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
```

### AuthService Implementation

```csharp
public interface IAuthService
{
    Task<AuthResponse> GenerateTokenAsync(ApplicationUser user);
}

public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;

    public AuthService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse> GenerateTokenAsync(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddHours(_jwtSettings.ExpirationHours),
            signingCredentials: creds);

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = token.ValidTo
        };
    }
}
```

### AuthController Updates

1. Inject IAuthService
2. Update login method:

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    var result = await _signInManager.PasswordSignInAsync(
        request.Email, request.Password, false, false);

    if (!result.Succeeded)
        return Unauthorized();

    var user = await _userManager.FindByEmailAsync(request.Email);
    var token = await _authService.GenerateTokenAsync(user);

    return Ok(token);
}
```

3. Add GET me endpoint:

```csharp
[HttpGet("me")]
[Authorize]
public async Task<IActionResult> GetMe()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var user = await _userManager.FindByIdAsync(userId);

    var userDto = new UserDto
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };

    return Ok(userDto);
}
```

### Swagger Configuration

Ensure Swagger is configured to support JWT:

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
```

## Testing Requirements

1. **Unit Tests:**

   - AuthService token generation
   - DTO validation
   - Controller action results

2. **Integration Tests:**

   - Full authentication flow
   - JWT validation on protected endpoints
   - Swagger UI JWT input

3. **Manual Testing:**
   - Login via Swagger UI
   - Use JWT token to access protected endpoints
   - Token expiration handling

## Security Considerations

1. **Token Security:**

   - Use strong secret key (256-bit minimum)
   - Implement token refresh mechanism (future enhancement)
   - Store tokens securely on client side

2. **Password Policies:**

   - Configure Identity password requirements
   - Implement password complexity rules

3. **Rate Limiting:**
   - Consider implementing rate limiting on auth endpoints

## Assumptions and Requirements

- ASP.NET Core Identity is properly configured
- Database migrations are up to date
- JWT settings are securely stored in configuration
- Clients can handle JWT tokens appropriately
- Swagger UI is enabled for API documentation

## Success Criteria

1. Users can register with email/password
2. Users can login and receive JWT token
3. JWT tokens work for accessing protected endpoints
4. Swagger UI supports JWT authorization
5. Token expiration is properly handled
6. All endpoints return appropriate HTTP status codes
7. Application builds and runs without errors

## Next Steps

1. Implement the AuthService
2. Update AuthController with JWT generation
3. Configure JWT authentication in Program.cs
4. Add Swagger JWT support
5. Test the complete authentication flow
6. Update protected endpoints to use [Authorize] as needed
