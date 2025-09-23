# Product Context - PodMD

## Why This Project Exists

PodMD addresses the critical need for intelligent, automated troubleshooting in modern DevOps environments where Kubernetes and CI/CD failures create operational chaos and slow resolution times.

## Problems Solved

- **Fragmented triage**: Logs scattered across Kubernetes, Jenkins, and GitLab with no unified analysis
- **Slow manual debugging**: Engineers waste hours sifting through verbose, unstructured logs
- **Generic AI responses**: Standard LLM analysis lacks domain-specific context and organizational knowledge
- **Insecure credential management**: API tokens and credentials stored insecurely or hardcoded

## How It Works

1. **Source Integration**: Connect Kubernetes clusters, Jenkins pipelines, and GitLab projects
2. **Log Ingestion**: Automatically fetch logs from failed pods, deployments, or CI/CD jobs
3. **AI Analysis**: LLM processes logs enriched with RAG from knowledge bases
4. **Structured Output**: Returns categorized errors, actionable recommendations, and evidence citations
5. **Secure Access**: RBAC-controlled API and web UI for credential and analysis management

## User Experience Goals

- **Reduce MTTR by 50%** through instant, actionable insights
- **Enable self-service troubleshooting** for all engineering roles
- **Provide confidence-scored recommendations** with clear evidence
- **Ensure security-first access** with minimal friction
- **Support API-first integration** while offering intuitive web management

## Key Product Features

- Multi-source log ingestion (Kubernetes, Jenkins, GitLab, raw logs)
- AI-powered analysis with RAG-enhanced recommendations
- Secure credential management with encryption and RBAC
- REST API with OpenAPI documentation and async processing
- Simple web UI for source management and analysis
