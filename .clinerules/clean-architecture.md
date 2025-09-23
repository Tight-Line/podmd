## Brief overview

This set of guidelines is project-specific for the PodMD project, focusing on Clean Architecture principles and project structure.

## Architecture layering

- Domain layer must not depend on Application or Infrastructure layers
- Application layer must not depend on Infrastructure or Api layers
- Infrastructure layer must not depend on Api layer
- Maintain clear separation of concerns across layers

## File placement

- Controllers belong in PodMD.Api/Controllers
- Entities belong in PodMD.Domain/Entities
- Repositories belong in PodMD.Infrastructure/Repositories
- Use Cases (commands/queries) belong in PodMD.Application/UseCases/
- Services belong in appropriate layer directories (Application/Services, Infrastructure/Services)

## Interface placement

- Service interfaces belong in PodMD.Application/Interfaces
- Repository interfaces belong in PodMD.Domain/Interfaces
- Domain events and specifications belong in PodMD.Domain/Interfaces
- Infrastructure implements interfaces from Application and Domain layers
- Avoid circular dependencies by keeping interfaces in higher layers

## API design

- Use Minimal APIs for simple endpoints
- Use Controllers when endpoint complexity exceeds 3+ routes
- DTOs for request/response data transfer

## Build and deployment

- Ensure application builds successfully after changes
- Verify application starts correctly
- Confirm all required services (databases, caches) are reachable
- Health checks must respond as expected
- Use environment variables for credentials; avoid hard-coded secrets
- Document any build or startup failures in progress tracking
