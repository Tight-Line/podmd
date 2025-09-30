# Code Standards - Overview

## Purpose

This document provides an overview of all code standards for the project. Standards are organized by technology and concern for better discoverability and maintenance.

## Standards Organization

### Technology-Specific Standards

- **[Backend Standards](codeStandards-backend.md)**: C#/.NET Web API development

  - ASP.NET Core, EF Core, Identity, Clean Architecture
  - Database design, API patterns, security

- **[Frontend Standards](codeStandards-frontend.md)**: Vue.js/TypeScript development
  - Component architecture, state management, API integration
  - TypeScript patterns, testing, performance

### Shared Standards

- **[Shared Standards](codeStandards-shared.md)**: Cross-cutting concerns
  - Security, version control, documentation, processes
  - Accessibility, compliance, communication

## Quick Reference

### For Backend Developers

- Use Clean Architecture with layered separation
- Implement EF Core with code-first migrations
- Use ASP.NET Core Identity for authentication
- Follow C# coding conventions and nullable reference types

### For Frontend Developers

- Use Vue.js Composition API with TypeScript
- Implement Pinia for state management
- Generate API clients from OpenAPI specifications
- Follow atomic design principles for components

### For All Developers

- Never leak sensitive information in logs/errors
- Use proper version control practices
- Write comprehensive documentation
- Implement security best practices

## Standards Maintenance

- Each standards file is owned by the relevant technology team
- Regular reviews ensure standards stay current
- New patterns and technologies are evaluated for inclusion
- Breaking changes require team consensus

## Related Documentation

- [System Patterns](systemPatterns-optimal.md): Architectural patterns and decisions
- [Workflows](../.clinerules/workflows/): Project initialization and development processes
- [Memory Bank](../memory-bank/): Project context and technical decisions
