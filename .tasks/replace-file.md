# Feature Specification: Knowledge Base File Replace Functionality

## Objective

Add replace file functionality to knowledge base files in the frontend, allowing users to replace existing files with new ones without manual deletion and re-upload. Users can click a replace button next to the delete button, which opens the upload dialog and automatically uploads the new file while deleting the old one from MinIO storage.

## Scope

### Must Include Features

- Frontend UI changes (replace button next to delete, upload dialog behavior)
- Backend API endpoint for file replacement (reuse existing endpoints)
- MinIO file operations (upload new file, delete old file)
- File validation and error handling
- UI state updates and list refresh after replacement

### Excluded Features

- Bulk file operations, file version history, advanced editing features
- Multi-file selection, async processing indicators

### Technical Boundaries

- Single file replacement only (no bulk/multi-select)
- Maintain existing security/auth patterns
- Reuse existing file upload infrastructure where possible
- Keep simple synchronous behavior (no advanced async indicators needed)

## Success Criteria

- Replace button visible and functional next to delete button
- Upload dialog opens on click
- New file uploads successfully and old file deleted from MinIO
- UI updates to show replaced file with new metadata
- Error handling for failed operations
- Seamless user experience without page refresh

## Entity Specifications

Reusing existing KnowledgeFile entity with all necessary fields:

- `Id` (Guid, required) - Primary key for the knowledge file record
- `KnowledgeBaseId` (Guid, required) - Foreign key to the associated knowledge base
- `FileName` (string, required, max 255) - Original filename as uploaded
- `StorageKey` (string, required, max 500) - MinIO storage path/key for the file
- `ContentType` (string, required, max 100) - MIME type of the file
- `FileSize` (long, required) - Size in bytes
- `CreatedAt` (DateTime, required) - When the file record was created
- `UpdatedAt` (DateTime, required) - When the file was last modified (will be updated on replacement)
- `KnowledgeBase` navigation property

## DTO Specifications

### CreateKnowledgeFileDto (for uploading new files)

- `KnowledgeBaseId` (Guid, required) - Target knowledge base
- `FileName` (string, required) - Original filename
- `ContentType` (string, required) - MIME type
- `FileSize` (long, required) - File size in bytes
- `FileStream` (Stream, required) - File content

### UpdateKnowledgeFileDto (for replacing files)

- `FileName` (string, optional) - Will be set to uploaded file's name (replaces existing filename)
- `ContentType` (string, required) - New MIME type
- `FileSize` (long, required) - New file size
- `FileStream` (Stream, required) - New file content

### KnowledgeFileDto (for reading/display)

- `Id`, `KnowledgeBaseId`, `FileName`, `ContentType`, `FileSize`, `CreatedAt`, `UpdatedAt` (all required)
- No sensitive data to exclude in read operations

## API Endpoints

| Method | Path                                  | Description                        | Parameters                    | Response           | Security |
| ------ | ------------------------------------- | ---------------------------------- | ----------------------------- | ------------------ | -------- |
| PUT    | `/api/v1/files/{fileId}/replace`      | Replace existing file with new one | fileId (URL), file (FormData) | KnowledgeFileDto   | JWT      |
| GET    | `/api/v1/knowledgebases/{kbId}/files` | List files for knowledge base      | kbId (URL)                    | KnowledgeFileDto[] | JWT      |
| POST   | `/api/v1/knowledgebases/{kbId}/files` | Upload new files                   | kbId (URL), files (FormData)  | KnowledgeFileDto[] | JWT      |
| DELETE | `/api/v1/files/{fileId}`              | Delete specific file               | fileId (URL)                  | 204 No Content     | JWT      |

## File Structure

### Frontend Files to Modify

- `vue-frontend/src/components/KnowledgeBaseFileUpload.vue` → Add replace button next to delete button, implement modal dialog for file selection, integrate with existing replace API endpoint

### Backend Files (Already Implemented - Verify)

- `dotnet-backend/src/PodMD.Api/Controllers/V1/KnowledgeFileController.cs` → Should have PUT /replace endpoint
- `dotnet-backend/src/PodMD.Application/Dtos/KnowledgeFileDtos.cs` → UpdateKnowledgeFileDto should exist
- `dotnet-backend/src/PodMD.Application/Interfaces/IKnowledgeFileService.cs` → Should have ReplaceFileAsync method
- `dotnet-backend/src/PodMD.Application/Services/KnowledgeFileService.cs` → May need completion of ReplaceFileAsync method
- `dotnet-backend/src/PodMD.Domain/Entities/KnowledgeFile.cs` → Entity exists
- `dotnet-backend/src/PodMD.Infrastructure/Repositories/KnowledgeFileRepository.cs` → Has Update method

## Implementation Plan

### 1. Backend Verification

- [ ] Confirm PUT `/api/v1/files/{fileId}/replace` endpoint is properly implemented
- [ ] Verify UpdateKnowledgeFileDto exists and is used correctly
- [ ] Ensure MinIO integration handles upload new/delete old operations
- [ ] Check error handling and validation

### 2. API Client Updates

- [ ] Regenerate OpenAPI client if replace endpoint not included in `vue-frontend/src/api/Api.ts`
- [ ] Ensure `authenticatedApi` includes the replace method

### 3. Frontend Implementation

- [ ] Add replace button (pi-refresh icon) next to delete button in file list
- [ ] Create/reuse modal dialog for single file selection
- [ ] Implement replace logic: open dialog → select file → call API → update UI
- [ ] Add proper error handling and success notifications
- [ ] Style consistently with existing UI patterns
- [ ] Test component integration and user flow

## Assumptions and Validations

- Backend replace endpoint is implemented (controller code suggests yes)
- MinIO delete/upload operations work correctly with proper error logging for storage IDs
- File validation follows existing patterns (same file replacement is allowed)
- JWT authentication covers the replace endpoint
- UI can be implemented within existing KnowledgeBaseFileUpload component
- Backend automatically updates filename in DB with uploaded file's actual name
- FormData field name for file upload is "file"
- Success messages should be detailed about file replacement
- Error handling should log MinIO storage IDs and handle concurrent replacement conflicts

## Risks and Mitigations

- **MinIO storage quota**: Handle in API error responses
- **Large file uploads**: Use existing timeout/conflict patterns
- **Concurrent access**: Entity Framework handles concurrency
- **UI state issues**: Refresh file list after successful replacement

## Acceptance Criteria

- [ ] Replace button appears next to each file's delete button
- [ ] Clicking replace opens file selection dialog
- [ ] File selection proceeds without page refresh
- [ ] Success updates file metadata in UI instantly
- [ ] Errors show appropriate toast messages
- [ ] No breaking changes to existing functionality
- [ ] Consistent with existing UI/UX patterns
