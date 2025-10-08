# API Key Generation & Management Implementation

## Feature Overview

**Objective:** Enable users to generate API keys for machine-to-machine authentication, allowing programmatic access to the PodMD system without interactive user sessions.

**Key Requirements:**

- Users can generate, manage, and revoke API keys
- Secure storage with hashed keys and plaintext return only on creation
- Request validation via API key authentication
- Usage tracking with configurable limits
- Compatible with existing Clean Architecture and security patterns

## Technical Specifications

### 1. Entity Design

**File:** `PodMD.Domain/Entities/ApiKey.cs`

```csharp
public sealed class ApiKey
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string HashedKey { get; set; } = string.Empty;
    public string? Permissions { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public string Status { get; set; } = "active";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}
```

**Constraints:**

- HashedKey: Unique index required
- Status: Enum values ("active", "revoked")
- UserId: Foreign key to ApplicationUser
- UsageCount: Auto-increment on successful usage

### 2. DTO Specifications

**File:** `PodMD.Application/Dtos/ApiKeyDtos.cs`

```csharp
public record CreateApiKeyRequest(string? Permissions, int? UsageLimit);

public record CreateApiKeyResponse(Guid Id, string Key, string? Permissions, int? UsageLimit);

public record UpdateApiKeyRequest(string? Permissions, int? UsageLimit, string? Status);

public record ApiKeyDto(
    Guid Id,
    string UserId,
    string? Permissions,
    int? UsageLimit,
    int UsageCount,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastUsedAt
);
```

### 3. API Endpoints

**Base Path:** `/api/apikeys`

| Method | Endpoint            | Request             | Response                   | Constraints           | Description           |
| ------ | ------------------- | ------------------- | -------------------------- | --------------------- | --------------------- |
| POST   | `/api/apikeys`      | CreateApiKeyRequest | CreateApiKeyResponse (201) | JWT auth              | Create new API key    |
| GET    | `/api/apikeys`      | -                   | ApiKeyDto[] (200)          | JWT auth, user filter | List user's API keys  |
| GET    | `/api/apikeys/{id}` | -                   | ApiKeyDto (200/404)        | JWT auth, ownership   | Get specific key      |
| PUT    | `/api/apikeys/{id}` | UpdateApiKeyRequest | ApiKeyDto (200/400/404)    | JWT auth, ownership   | Update key properties |
| DELETE | `/api/apikeys/{id}` | -                   | 204/404                    | JWT auth, ownership   | Revoke key            |

**Validation Endpoint:**
| POST | `/api/auth/validate-key` | - | UserDto (200/401/403) | API key in header | Validate and authenticate API key |

### 4. Security Implementation

**API Key Format:**

- Base64-encoded 32-byte random bytes (for 512-bit security)
- Authorization header: `Authorization: Bearer {api_key}`

**Storage:**

- SHA-256 hash of the API key for database lookup
- HashedKey field: SHA256 hash stored as hexadecimal string

**Validation Flow:**

1. Extract API key from Authorization header
2. SHA-256 hash the provided key
3. Query ApiKey table for matching HashedKey
4. Check status == "active" and usage limits
5. Increment usage count on successful validation
6. Return 401 if no match, 403 if revoked/limited

## Architecture Implementation

### Layer Separation (Clean Architecture)

#### Domain Layer

- **ApiKey entity** - Pure domain model
- **IApiKeyRepository interface** - Data access contract

#### Application Layer

- **IApiKeyService** - Business logic interface
- **IApiKeyAuthenticationService** - Authentication interface
- **ApiKeyService** - CRUD and management operations
- **ApiKeyAuthenticationService** - API key validation and user resolution
- **ApiKeySettings** - Configuration (key length, hash algorithm)

#### Infrastructure Layer

- **ApiKeyRepository** - EF Core implementation
- **Migration** - Database schema changes

#### API Layer

- **ApiKeysController** - REST endpoints
- **Middleware/Service registration** - Dependency injection setup

### Key Implementation Details

#### API Key Generation

```csharp
// Use System.Security.Cryptography.RandomNumberGenerator
private static string GenerateApiKey()
{
    var bytes = new byte[32]; // 256 bits
    RandomNumberGenerator.Fill(bytes);
    return Convert.ToBase64String(bytes);
}
```

#### Hashing Strategy

```csharp
// Use SHA-256 for security, HMAC if additional salt needed
private static string HashApiKey(string apiKey)
{
    using var sha256 = SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(apiKey);
    var hash = sha256.ComputeHash(bytes);
    return Convert.ToHexString(hash);
}
```

#### Database Migration

- Add ApiKey table with proper foreign key constraints
- Create unique index on HashedKey
- Add cascade delete for user relationships

#### Controller Implementation

```csharp
[ApiController]
[Route("api/apikeys")]
[Authorize] // JWT required
public class ApiKeysController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;

    [HttpPost]
    public async Task<ActionResult<CreateApiKeyResponse>> Create(CreateApiKeyRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _apiKeyService.CreateApiKeyAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }
}
```

### Security Considerations

#### Input Validation

- Permissions: JSON schema validation if used
- UsageLimit: Positive integer validation
- Status: Enum restriction

#### Authorization

- JWT authentication for all management endpoints
- Ownership validation (user can only access their keys)
- Permission scope checking (future enhancement)

#### Error Handling

- Invalid keys: 401 Unauthorized
- Revoked/limited: 403 Forbidden
- Not found: 404 Not Found
- Server errors: 500 Internal Server Error

#### Rate Limiting

- Implement per-key rate limiting
- Track usage count with atomic updates
- Configurable limits per key or globally

### Testing Requirements

#### Unit Tests

- ApiKeyService business logic
- ApiKeyAuthenticationService validation
- Repository data operations
- Hash generation and validation

#### Integration Tests

- Full API endpoint testing
- Database persistence and retrieval
- Authentication middleware
- Usage limit enforcement

#### Security Tests

- Key exposure prevention
- Hash collision resistance
- Authorization bypass scenarios
- SQL injection prevention

### Deployment & Migration

#### Database Migrations

```csharp
// Migration class
public partial class AddApiKeyTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ApiKeys",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                UserId = table.Column<string>(nullable: false),
                HashedKey = table.Column<string>(nullable: false),
                Permissions = table.Column<string>(nullable: true),
                UsageLimit = table.Column<int>(nullable: true),
                UsageCount = table.Column<int>(nullable: false),
                Status = table.Column<string>(nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(nullable: true),
                LastUsedAt = table.Column<DateTimeOffset>(nullable: true),
                RevokedAt = table.Column<DateTimeOffset>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApiKeys", x => x.Id);
                table.ForeignKey(
                    name: "FK_ApiKeys_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ApiKeys_UserId",
            table: "ApiKeys",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_ApiKeys_HashedKey",
            table: "ApiKeys",
            column: "HashedKey",
            unique: true);
    }
}
```

#### Configuration Updates

- Add ApiKeySettings to appsettings.json
- Register services in Program.cs
- Update Swagger documentation for new endpoints
- Add health checks for API key functionality

### Implementation Checklist

- [ ] Create ApiKey entity in Domain
- [ ] Add IApiKeyRepository interface
- [ ] Define DTOs in Application layer
- [ ] Implement business logic services
- [ ] Create EF Core repository
- [ ] Add database migration
- [ ] Implement API controller endpoints
- [ ] Configure dependency injection
- [ ] Add API key authentication middleware
- [ ] Update Swagger documentation
- [ ] Add security validations
- [ ] Write unit and integration tests
- [ ] Update application documentation

### Success Criteria

- Users can generate API keys via API
- Generated keys work for M2M authentication
- Keys are securely stored (hashed)
- Usage limits are enforced
- Revocation works immediately
- All endpoints return appropriate HTTP status codes
- Requests are logged for security auditing
- No plaintext keys in logs or database
- Compatible with existing JWT authentication
- Clean Architecture principles maintained

### Future Enhancements

- API key expiration dates
- Fine-grained permission scopes
- Webhook notifications for key events
- API key usage analytics dashboard
- Bulk key operations
- Key rotation with grace periods
