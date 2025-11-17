# Implementation Plan: Enhanced User Registration and Profile Management

**Branch**: `002-user-profile` | **Date**: 2025-11-16 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-user-profile/spec.md`
**Live Demo Focus**: Elevate the public Demo's registration and profile experience with photo uploads, stronger security, and Admin oversight, directly improving user-facing identity features while preserving contract boundaries.

## Summary

Implement profile photo uploads (with resizing and local storage), enhanced registration and profile editing UX, role-based access control with an Admin dashboard, password strength validation, email verification support, operational background jobs for audit-log retention/storage monitoring, and performance instrumentation to satisfy success criteria—all centered on `WebSpark.ArtSpark.Demo` using ASP.NET Core Identity and EF Core migrations.

## Technical Context

<!--
  Replace defaults only when the feature truly deviates from the repository
  standard. Note deviations explicitly so reviewers can validate scope.
-->

**Language/Version**: C# 13 / .NET 10 (Preview)  
**Primary Dependencies**: ASP.NET Core Identity, Entity Framework Core, Serilog, ImageSharp (or equivalent for resizing), WebSpark.ArtSpark shared libraries  
**Storage**: SQLite via EF Core migrations (production-ready for single-node Demo; local file system for profile photos and thumbnails)  
**Testing**: `dotnet test` across `WebSpark.ArtSpark.Tests` focusing on new ProfilePhotoService, RoleManagement, and integration tests for registration/admin flows  
**Target Platform**: ASP.NET Core web app deployed to artspark.markhazleton.com  
**Project Type**: Multi-project .NET solution centered on `WebSpark.ArtSpark.Demo`  
**Performance Goals**: Registration completion < 2 minutes; photo processing < 5 seconds/file up to 5MB; Admin user list p95 load time < 1 second (achieved via AsNoTracking projection-based queries per research.md EF optimization strategy); password feedback < 200ms  
**Constraints**: Profile photos ≤ 5MB, JPEG/PNG/WebP only; bios ≤ 500 characters; thumbnails stored as disk files referenced via relative paths; audit logs retained 1 year via hosted cleanup job; email verification tokens expire in 24 hours; disk usage monitoring alerts when profile storage exceeds configured thresholds  
**Scale/Scope**: Applies to all Demo users (existing + new); Admin tooling targets thousands of user records with pagination

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Demo Alignment**: Scope confined to `WebSpark.ArtSpark.Demo`; validation via end-to-end registration, profile editing, and Admin dashboard manual tests plus UI verification for photo display.
- [x] **Shared Library Contracts**: No contract changes to Client/Agent/Console; ensure any helper additions stay internal to Demo keeping libraries stable.
- [x] **Reliability & Observability**: Add unit/integration tests for photo handling, role enforcement, and admin flows; include Serilog events for uploads, role changes, and audit logging; create EF migration for schema updates.
- [x] **Responsible AI Safeguards**: N/A—feature does not engage AI personas; confirm guardrail documentation unchanged.
- [x] **Documentation & Release Notes**: Update root and Demo README, add new docs on profile management and RBAC, refresh live testing checklist, and capture release notes per spec.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
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

**Structure Decision**: Retain existing multi-project layout; all implementation occurs inside `WebSpark.ArtSpark.Demo` plus new documentation under `docs/`, with planning artifacts in `specs/002-user-profile/`.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | - | - |
