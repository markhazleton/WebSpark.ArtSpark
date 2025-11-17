# Implementation Plan: Quality Audit & Package Currency

**Branch**: `[001-quality-audit]` | **Date**: 2025-11-16 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-quality-audit/spec.md`
**Live Demo Focus**: Preserve Demo reliability by institutionalizing a repeatable audit that flags build diagnostics and stale dependencies before they impact visitors.

## Summary

Deliver a repository-wide quality audit workflow that inventories build errors/warnings, aggregates analyzer output, and reports NuGet/NPM currency in a Demo-first Markdown report. Implementation will script the audit recipe, normalize outputs, and document the remediation backlog alongside safeguard checks so maintainers can schedule fixes without blocking the initial audit delivery.

## Technical Context

**Language/Version**: C# / .NET 9.0 (consistent with solution)  
**Primary Dependencies**: .NET SDK CLI, NuGet CLI APIs, npm CLI, Serilog instrumentation hooks, existing WebSpark.ArtSpark solution build pipelines  
**Storage**: No persistent storage changes; audit artifacts stored as Markdown under `docs/copilot/YYYY-MM-DD/`  
**Testing**: `dotnet test` for impacted projects plus validation scripts to ensure audit command exits cleanly; targeted lint/test runs for npm workspaces if present  
**Target Platform**: ASP.NET Core Demo deployed to artspark.markhazleton.com with local/CI execution of audit recipe  
**Project Type**: Multi-project .NET solution centered on `WebSpark.ArtSpark.Demo`  
**Performance Goals**: Audit execution completes within 15 minutes on CI hardware and under 10 minutes locally, enabling maintainers to iterate during a single session  
**Constraints**: Rely on approved package feeds (NuGet.org, npmjs.org); avoid automated upgrades that break semantic versioning or Demo runtime stability  
**Scale/Scope**: Entire solution codebase (Demo, Agent, Client, Console) with primary remediation directed at Demo-facing diagnostics and dependencies

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*
- [x] **Demo Alignment**: Audit highlights Demo build diagnostics and stale packages, and validation occurs by running the recipe against `WebSpark.ArtSpark.Demo` and confirming the Markdown report sections populate.
- [x] **Shared Library Contracts**: No immediate contract changes; any remediation follow-up will document coordinated Demo updates before touching Client/Agent/Console APIs.
- [x] **Reliability & Observability**: Plan adds automated audit script coverage, enforces clean `dotnet build/test`, and captures Serilog/build metadata findings inside the report.
- [x] **Responsible AI Safeguards**: Audit re-validates existing personas, moderation hooks, and refusal messaging with documented outcomes, without introducing new AI endpoints.
- [x] **Documentation & Release Notes**: Audit output lands in `docs/copilot/YYYY-MM-DD/quality-audit.md`, with references in `docs/Documentation-Update-Summary.md` and Demo release notes.

## Project Structure

### Documentation (this feature)

```text
specs/001-quality-audit/
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
└── Tests/

WebSpark.ArtSpark.Agent/     # AI persona services consumed by the Demo
WebSpark.ArtSpark.Client/    # Art Institute API client consumed by Demo/Agent/Console
WebSpark.ArtSpark.Console/   # Developer tooling bound to Client contracts
docs/                        # Operational and implementation guides
```

**Structure Decision**: Reuse existing solution layout; all audit tooling and documentation live under `specs/001-quality-audit/` and `docs/copilot/`, while code inspections run against existing project directories above.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| *(None)* |  |  |
