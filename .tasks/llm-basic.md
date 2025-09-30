# LLM-Powered Kubernetes Log Analysis Feature

## Objective

Analyze raw Kubernetes pod/deployment logs and use an OpenAI compatible LLM to return structured JSON identifying root-cause errors and actionable solutions.

## Scope Definition

### Must Include

- Analyze logs to identify candidate root-cause errors using OpenAI-compatible LLM
- Deduplicate multi-line errors and normalize transient values (timestamps, UUIDs, IPs, ports, pod/container names, memory addresses)
- Apply heuristics for Java, .NET, Python, Go, and general multi-line errors
- Provide actionable solutions per error group with description, steps, and commands (kubectl/helm/etc.)
- Return structured JSON response with specific schema including errors array with general_message, occurrences, and solutions

### Should Exclude

- Normal warnings (severity "warn" or similar) - focus only on errors
- Stack traces beyond main error line(s) - keep analysis concise
- Secrets redaction - optional/ignored for this task

## Architecture Overview

This feature follows Clean Architecture principles with proper separation of concerns:

- **Domain**: Core business entities
- **Application**: Use cases and business logic orchestration
- **Infrastructure**: External service implementations (LLM API)
- **API**: HTTP endpoints and request/response handling

## Entity Specifications

### KubeCluster Entity Extensions

| Field          | Type   | Required | Constraints | Description                            |
| -------------- | ------ | -------- | ----------- | -------------------------------------- |
| Instructions   | string | No       | -           | LLM analysis instructions/prompts      |
| ResponseFormat | string | No       | -           | Expected response format specification |

_Note: Extending existing KubeCluster entity - no new entities required_

## DTO Specifications

### Request DTOs

**PodAnalysisRequest** (for pod logs):

- Namespace (string, required) - Target namespace
- PodName (string, required) - Target pod
- ContainerName (string, optional) - Specific container
- TailLines (int, optional) - Lines to retrieve
- SinceSeconds (int, optional) - Time filter
- Previous (bool, optional) - Get previous container logs

**DeploymentAnalysisRequest** (for deployment logs):

- Namespace (string, required) - Target namespace
- DeploymentName (string, required) - Target deployment
- Fallback (bool, optional) - Fallback behavior
- IncludeDescription (bool, optional) - Include description

### Response DTO

**LogAnalysisResponse**:

- Data (object, required) - Analysis results in JSON schema format:
  ```typescript
  {
    errors: Array<{
      general_message: string;
      occurrences: string[];
      solutions: Array<{
        description: string;
        steps: Array<{
          title: string;
          explanation: string;
          command: string;
        }>;
      }>;
    }>;
  }
  ```
- Success (bool, required) - Operation success status
- Message (string, optional) - Error details if unsuccessful

## API Endpoint Specifications

| Method | Endpoint                                           | Description                              | Request/Response                                                  |
| ------ | -------------------------------------------------- | ---------------------------------------- | ----------------------------------------------------------------- |
| POST   | `/api/v1/clusters/{clusterId}/analyze/pods`        | Analyze logs from specific pod using LLM | Request: PodAnalysisRequest, Response: LogAnalysisResponse        |
| POST   | `/api/v1/clusters/{clusterId}/analyze/deployments` | Analyze logs from deployment using LLM   | Request: DeploymentAnalysisRequest, Response: LogAnalysisResponse |

### API Constraints

- **Authentication:** All endpoints require authorization
- **Error Handling:** Comprehensive error responses with ProblemDetails (404 for not found, 400 for bad requests, 502 for Kubernetes errors)
- **Response Types:** Proper HTTP status codes and response type annotations
- **Routing:** Follows existing cluster-nested pattern for analysis endpoints
- **Validation:** Request validation using DataAnnotations

## File Structure & Implementation

### Files to Create

1. **`src/PodMD.Application/Analysis/ILlmClient.cs`** - Interface for LLM communication
2. **`src/PodMD.Infrastructure/Analysis/LlmClient.cs`** - OpenAI-compatible API implementation
3. **`src/PodMD.Application/Analysis/IAnalysisService.cs`** - Interface for log analysis orchestration
4. **`src/PodMD.Application/Analysis/AnalysisService.cs`** - Coordinates log fetching and LLM analysis
5. **`src/PodMD.Api/Controllers/AnalysisController.cs`** - POST endpoints for analysis
6. **`src/PodMD.Api/DTOs/PodAnalysisRequestDto.cs`** - Pod-specific analysis requests
7. **`src/PodMD.Api/DTOs/DeploymentAnalysisRequestDto.cs`** - Deployment-specific analysis requests
8. **`src/PodMD.Api/DTOs/AnalysisResponseDto.cs`** - Structured analysis responses

### Files to Modify

1. **`src/PodMD.Api/appsettings.json`** - Add LLM configuration defaults

## Implementation Guidance

### Development Phases

#### Phase 1: Core Infrastructure

1. **LLM Client Implementation**

   - Implement `ILlmClient` interface in Application layer
   - Create `LlmClient` in Infrastructure layer with OpenAI-compatible API calls
   - Handle authentication, error handling, and response parsing

2. **Analysis Service**
   - Implement `IAnalysisService` interface
   - Create `AnalysisService` that orchestrates log fetching and LLM analysis
   - Integrate with existing `IKubeLogService` for log retrieval
   - Implement log truncation and preprocessing

#### Phase 2: API Layer

3. **DTOs Creation**

   - Create request/response DTOs matching existing patterns
   - Implement proper validation using DataAnnotations
   - Ensure JSON schema compliance for responses

4. **Controller Implementation**
   - Create `AnalysisController` following existing patterns
   - Implement POST endpoints for pod and deployment analysis
   - Add comprehensive error handling and response formatting

#### Phase 3: Configuration & Deployment

5. **Configuration**

   - Add LLM settings to appsettings.json
   - Implement environment variable configuration
   - Add health checks for LLM service availability

6. **Dependency Injection**
   - Register new services in Program.cs
   - Ensure proper service lifetimes (scoped for HTTP clients)

### Technical Requirements

#### LLM Integration

- **API Compatibility:** OpenAI-compatible endpoints and authentication
- **Error Handling:** Graceful degradation when LLM service unavailable
- **Rate Limiting:** Respect API rate limits and implement retry logic
- **Cost Optimization:** Log truncation and filtering to minimize token usage

#### Log Processing

- **Normalization:** Implement value normalization (timestamps, UUIDs, IPs, etc.)
- **Deduplication:** Remove duplicate error patterns
- **Language Detection:** Identify programming language for targeted heuristics
- **Severity Filtering:** Focus only on error-level messages

#### Response Formatting

- **Schema Compliance:** Ensure responses match specified JSON schema
- **Solution Structure:** Provide actionable steps with kubectl/helm commands
- **Error Grouping:** Group similar errors with occurrence tracking

### Testing Strategy

1. **Unit Tests:** Service layer logic and DTOs
2. **Integration Tests:** LLM API communication and log processing
3. **API Tests:** Endpoint functionality and error handling
4. **E2E Tests:** Full workflow from log retrieval to analysis response

### Security Considerations

- **API Keys:** Secure storage and rotation of LLM API credentials
- **Input Validation:** Sanitize log content before sending to LLM
- **Output Validation:** Validate LLM responses before returning to clients
- **Rate Limiting:** Implement API rate limiting to prevent abuse

### Performance Considerations

- **Async Processing:** All operations should be asynchronous
- **Caching:** Consider caching analysis results for repeated requests
- **Streaming:** Evaluate streaming responses for large log analysis
- **Background Processing:** Option for asynchronous analysis of large log volumes

## Success Criteria

- ✅ Structured error identification with root-cause analysis
- ✅ Actionable solutions with executable commands
- ✅ Multi-language error pattern recognition
- ✅ Log deduplication and normalization
- ✅ OpenAI-compatible LLM integration
- ✅ Clean Architecture compliance
- ✅ Comprehensive error handling
- ✅ API documentation and testing

## Assumptions & Dependencies

- Existing `IKubeLogService` integration available
- Kubernetes cluster connectivity functional
- OpenAI-compatible LLM service accessible
- Proper authentication and authorization configured
- Clean Architecture patterns followed throughout implementation

This task file serves as the comprehensive specification for implementing LLM-powered Kubernetes log analysis in the PodMD application.
