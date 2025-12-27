# Implementation Plan: AI Persona Prompt Management System

**Branch**: `003-prompt-management` | **Date**: 2025-11-17 | **Spec**: [`spec.md`](./spec.md)
**Input**: Feature specification from `/specs/003-prompt-management/spec.md`
**Live Demo Focus**: Externalize persona system prompts so Demo operators and content authors can adjust AI behavior without redeploying, preserving rapid iteration and cultural safeguards on the public site.

## Summary

Load each persona’s system prompt from markdown files located under `prompts/agents/` using a new `IPromptLoader` and file-backed `IPersonaHandler` decorator in the Agent library. Prompts leverage persona-specific token whitelists, emit audit logging (path, size, hash), surface metadata overrides (model, temperature), support optional hot reload with change logging, and fall back to existing hardcoded prompts if files are missing or invalid. Demo configuration wires the loader, ensures path validation, and surfaces prompt metadata for operators.

## Technical Context

**Language/Version**: C# / .NET 10.0 (Preview)  
**Primary Dependencies**: ASP.NET Core, Microsoft Semantic Kernel, Serilog, Polly, WebSpark.ArtSpark.Agent, WebSpark.ArtSpark.Client  
**Storage**: SQLite via EF Core migrations (no schema changes; prompt files stored on web server file system)  
**Testing**: `dotnet test WebSpark.ArtSpark.Tests`; targeted Agent unit tests (`PromptLoaderTests`, `PersonaFactoryTests`, `PromptTemplateTokenTests`, `PromptOptionsValidationTests`, `PromptLoaderHotReloadTests`, `PromptAuditLoggingTests`) and Demo integration tests for prompt fallback  
**Target Platform**: ASP.NET Core web app deployed to https://artspark.markhazleton.com  
**Project Type**: Multi-project .NET solution centered on `WebSpark.ArtSpark.Demo`  
**Performance Goals**: Prompt load duration < 50 ms per persona during initialization; no increase to chat request latency beyond existing budgets  
**Constraints**: Production prompt files editable on Demo web server (`prompts/agents/`); strict token validation triggers fallback; hot reload disabled in production by default; metadata overrides must map to OpenAI configuration surface  
**Scale/Scope**: Impacts all four AI personas (Artwork, Artist, Curator, Historian) across Demo and Console; prompt files expected to remain < 50 KB each

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Demo Alignment**: Demo consumes file-backed prompts via updated `AddArtSparkAgent()` configuration; validation through staging edits to `artspark.artwork.prompt.md` and live regression of `/Artwork/Details/{id}` chat surface.
- [x] **Shared Library Contracts**: Agent library gains `IPromptLoader` and decorator without breaking existing `IPersonaHandler` signatures; Demo integration ships simultaneously to satisfy contract stability.
- [x] **Reliability & Observability**: Add Agent unit tests for file loading, fallback, token validation, hot reload, and audit logging; add Demo integration tests; Serilog emits `PromptLoaded`, `PromptLoadFailed`, `PromptFallbackUsed`, `PromptTokenValidationFailed`, and `ConfigurationReloaded` with hash/path metadata.
- [x] **Responsible AI Safeguards**: Persona prompts enforce required sections (CULTURAL SENSITIVITY, CONVERSATION GUIDELINES); strict whitelist validation prevents injection; fallback retains reviewed hardcoded prompts; documentation instructs culturally respectful updates.
- [x] **Documentation & Release Notes**: Update `docs/copilot/prompt-authoring-guide.md`, `WebSpark.ArtSpark.Agent/README.md`, `docs/AI-Chat-Personas-Implementation.md`, and root `README.md`; release notes highlight externalized prompts, metadata overrides, and logging; ensure deployment logs capture prompt hash metadata.

## Project Structure

### Documentation (this feature)

```text
specs/003-prompt-management/
├── plan.md              # This file (/speckit.plan output)
├── research.md          # Phase 0 output (/speckit.plan)
├── data-model.md        # Phase 1 output (/speckit.plan)
├── quickstart.md        # Phase 1 output (/speckit.plan)
├── contracts/           # Phase 1 output (/speckit.plan)
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
WebSpark.ArtSpark.Demo/      # ASP.NET Core MVC web app (primary delivery target)
├── Controllers/
├── Services/
├── Views/
├── wwwroot/
├── prompts/                # New persona prompt markdown files (added by this feature)
└── Tests/

WebSpark.ArtSpark.Agent/     # AI persona services consumed by the Demo
WebSpark.ArtSpark.Client/    # Art Institute API client consumed by Demo/Agent/Console
WebSpark.ArtSpark.Console/   # Developer tooling bound to Client contracts
docs/                        # Operational and implementation guides
```

**Structure Decision**: Reuse the existing multi-project layout; store editable persona prompts under `WebSpark.ArtSpark.Demo/prompts/agents/`, while `WebSpark.ArtSpark.Agent` exposes the file-backed loader consumed through `AddArtSparkAgent()` so Demo remains the integration hub.

## Complexity Tracking

No Constitution violations identified; table remains empty.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
