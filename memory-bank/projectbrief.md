# Project Brief – PodMD

## Core Purpose

PodMD is a **Web API service** that automates troubleshooting for failed Kubernetes pods/deployments and CI builds (Jenkins, GitLab). It fetches logs, enriches them with knowledge via Retrieval-Augmented Generation (RAG), and uses Large Language Models (LLMs) to produce structured, actionable "how to fix" recommendations.

## Target Users

- **Primary**: SREs, DevOps, Platform Engineers
- **Secondary**: Developers, QA/Release Engineers

## Project Scope

### In Scope (Initial Release)

- Web API service with Kubernetes/Jenkins/GitLab integrations
- Simple web UI for source management (clusters, CI/CD connections)
- Credential storage, RBAC, and audit logging
- Knowledge bases with RAG integration
- Structured analysis responses and async orchestration

### Out of Scope (Initial Release)

- Rich web UI features (real-time streaming, advanced dashboards)
- Long-term log archival or automatic remediation
- Additional CI/CD providers beyond Jenkins/GitLab
- Advanced observability or cost controls
