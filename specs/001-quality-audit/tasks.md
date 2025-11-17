# Tasks: Quality Audit & Package Currency

**Input**: Design documents from `/specs/001-quality-audit/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish a clean baseline so audit diagnostics reflect intentional changes.

- [X] T001 Run baseline restore for `WebSpark.ArtSpark.sln` to ensure toolchain readiness.
- [X] T002 Run baseline build for `WebSpark.ArtSpark.sln` and capture existing warnings for comparison.
- [X] T003 Run baseline tests for `WebSpark.ArtSpark.sln` to confirm current solution health.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create the reusable audit execution surface required by all user stories.

- [X] T004 Create runner scaffold in `scripts/audit/run-quality-audit.ps1` with parameters for output directory, severity filters, and orchestration placeholders.
- [X] T005 Implement shared context helpers in `scripts/audit/QualityAudit.psm1` to create audit run metadata and normalize report models.
- [X] T006 Add Pester harness skeleton in `scripts/audit/tests/QualityAudit.Tests.ps1` to host future audit script assertions.

**Checkpoint**: Foundation ready — user stories can now build on the audit runner.

---

## Phase 3: User Story 1 - Maintain Build Health Dashboard (Priority: P1) 🎯 MVP

**Goal**: Generate a Markdown report aggregating build diagnostics and dependency currency for Demo maintainers.

**Independent Test**: Execute `scripts/audit/run-quality-audit.ps1` and verify the generated `docs/copilot/YYYY-MM-DD/quality-audit.md` contains "Build Diagnostics" and "Dependency Currency" sections with ranked findings.

### Implementation for User Story 1

- [X] T007 [P] [US1] Implement `Invoke-BuildDiagnostics` in `scripts/audit/modules/BuildDiagnostics.ps1` to run `dotnet build`/`dotnet test` on `WebSpark.ArtSpark.sln` and emit `BuildHealthFinding` objects.
- [X] T008 [P] [US1] Implement `Get-NuGetCurrency` in `scripts/audit/modules/NuGetCurrency.ps1` to run `dotnet list` across solution `.csproj` files and capture outdated packages.
- [X] T009 [P] [US1] Implement `Get-NpmCurrency` in `scripts/audit/modules/NpmCurrency.ps1` to detect `package.json` manifests and collect `npm outdated`/`npm audit` results.
- [X] T010 [US1] Integrate diagnostics and dependency collectors in `scripts/audit/QualityAudit.psm1`, mapping results to `BuildHealthFinding` and `PackageCurrencyEntry` models.
- [X] T011 [US1] Render "Build Diagnostics", "Dependency Currency", and an "Alerting Notes" callout within `scripts/audit/run-quality-audit.ps1`, writing Markdown to `docs/copilot/YYYY-MM-DD/quality-audit.md` and flagging threshold breaches.
- [X] T012 [US1] Extend `scripts/audit/tests/QualityAudit.Tests.ps1` with Pester assertions ensuring diagnostics and dependency outputs contain severity, source project, and recommended actions.

**Checkpoint**: Audit report surfaces build health and package drift data for Demo review.

---

## Phase 4: User Story 2 - Plan Remediation Iterations (Priority: P2)

**Goal**: Provide filtered remediation backlogs that stakeholders can schedule independently of report generation.

**Independent Test**: Run `scripts/audit/run-quality-audit.ps1 -Severity Warning -MaxItems 10` and confirm `docs/copilot/YYYY-MM-DD/quality-audit-backlog.md` lists the selected items with owners and target release windows.

### Implementation for User Story 2

- [X] T013 [US2] Add backlog scoring and filtering logic to `scripts/audit/QualityAudit.psm1`, ordering findings by severity, risk, and effort metadata.
- [X] T014 [US2] Update `scripts/audit/run-quality-audit.ps1` to accept backlog filter parameters and emit `docs/copilot/YYYY-MM-DD/quality-audit-backlog.md` with export-ready content.
- [X] T015 [US2] Update `specs/001-quality-audit/quickstart.md` to document backlog parameters, export workflow, and sharing guidance.
- [X] T016 [US2] Add Pester coverage in `scripts/audit/tests/QualityAudit.Tests.ps1` verifying backlog filters respect severity and item limits.

**Checkpoint**: Maintainers can curate prioritized remediation batches directly from audit output.

---

## Phase 5: User Story 3 - Verify AI & Content Safeguards (Priority: P3)

**Goal**: Validate AI persona, moderation, and cultural safeguards with documented evidence and follow-up actions.

**Independent Test**: Execute `scripts/audit/run-quality-audit.ps1` and confirm the "Safeguards" section lists each control with pass/fail status, evidence links, and follow-up tasks when needed.

### Implementation for User Story 3

- [X] T017 [US3] Implement `Invoke-SafeguardAudit` in `scripts/audit/modules/Safeguards.ps1` to review `WebSpark.ArtSpark.Agent/Personas/` definitions and Demo moderation hooks for compliance evidence.
- [X] T018 [US3] Map safeguard results to `SafeguardControlCheck` objects within `scripts/audit/QualityAudit.psm1`, including outcomes and follow-up actions.
- [X] T019 [US3] Update `scripts/audit/run-quality-audit.ps1` to append the "Safeguards" section and merge failing controls into backlog exports.
- [X] T020 [US3] Expand `scripts/audit/tests/QualityAudit.Tests.ps1` with Pester checks that safeguard audits output control names, evidence references, and outcomes.

**Checkpoint**: Compliance reviewers receive actionable safeguard assessments in each audit run.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Document, validate, and finalize audit delivery across the solution.

- [X] T021 Record the audit workflow, diagnostic threshold policy, and latest findings in `docs/Documentation-Update-Summary.md`.
- [X] T022 Update `README.md` with a "Quality Audit" subsection linking to `scripts/audit/run-quality-audit.ps1` usage.
- [X] T023 Execute `scripts/audit/run-quality-audit.ps1` to generate the initial `docs/copilot/YYYY-MM-DD/quality-audit.md` and backlog artifacts for archives.
- [X] T024 Review the generated `docs/copilot/YYYY-MM-DD/quality-audit.md` to confirm required sections (Diagnostics, Dependencies, Safeguards, Backlog) render correctly.
- [X] T025 Run `dotnet test WebSpark.ArtSpark.sln` after audit integration to ensure solution health remains green.

---

## Dependencies & Execution Order

- **Phase 1 → Phase 2**: Baseline restore/build/test ensures the runner work starts from a known state.
- **Phase 2 → Phase 3-5**: User stories rely on the audit scaffold and shared context helpers.
- **User Stories**: Execute in priority order (US1 → US2 → US3) or in parallel once foundational tasks are complete.
- **Phase 6**: Only begin after desired user stories have shipped to avoid stale documentation.

## Parallel Opportunities

- **US1**: Tasks T007, T008, and T009 can proceed concurrently because they implement separate modules.
- **US2**: Tasks T013 and T015 may run in parallel once T014 is stubbed, covering logic vs. documentation.
- **US3**: Tasks T017 and T020 can be split, with safeguard collection and test coverage handled by different contributors.

## Implementation Strategy

1. Complete Phases 1-2 to establish the audit runner and shared helpers.
2. Deliver User Story 1 as the MVP, producing the combined diagnostics/dependency report.
3. Layer User Story 2 backlog exports so stakeholders can plan remediation without waiting for safeguard work.
4. Add User Story 3 safeguard audits to finalize compliance coverage.
5. Finish with Phase 6 polish: documentation, initial report capture, and regression tests.
