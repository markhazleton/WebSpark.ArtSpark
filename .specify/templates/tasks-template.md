---

description: "Task list template for feature implementation"
---

# Tasks: [FEATURE NAME]

**Input**: Design documents from `/specs/[###-feature-name]/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story. Always describe how the work surfaces in `WebSpark.ArtSpark.Demo` to honor the Live Demo First principle.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Demo Web App**: `WebSpark.ArtSpark.Demo/Controllers/`, `.../Services/`, `.../Views/`, `.../wwwroot/`
- **Agent Library**: `WebSpark.ArtSpark.Agent/Services/`, `.../Personas/`
- **Client Library**: `WebSpark.ArtSpark.Client/Clients/`, `.../Models/`
- **Shared Tests**: `WebSpark.ArtSpark.Tests/` (or `WebSpark.ArtSpark.Demo/Tests/` when scoped)
- Always reference the exact `.cs` file or migration path affected

<!-- 
  ============================================================================
  IMPORTANT: The tasks below are SAMPLE TASKS for illustration purposes only.
  
  The /speckit.tasks command MUST replace these with actual tasks based on:
  - User stories from spec.md (with their priorities P1, P2, P3...)
  - Feature requirements from plan.md
  - Entities from data-model.md
  - Endpoints from contracts/
  
  Tasks MUST be organized by user story so each story can be:
  - Implemented independently
  - Tested independently
  - Delivered as an MVP increment
  
  DO NOT keep these sample tasks in the generated tasks.md file.
  ============================================================================
-->

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Ensure solution dependencies and tooling support the upcoming Demo-focused work.

- [ ] T001 Restore solution packages (`dotnet restore WebSpark.ArtSpark.sln`) if new dependencies are required
- [ ] T002 Confirm EF Core tooling and Serilog configuration align with plan assumptions
- [ ] T003 [P] Update build/test automation scripts if new projects or pipelines are introduced

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

Examples of foundational tasks (adjust based on your project):

- [ ] T004 Create/Update EF Core migration in `WebSpark.ArtSpark.Demo/Migrations/`
- [ ] T005 [P] Extend shared services in `WebSpark.ArtSpark.Demo/Services/` needed by multiple stories
- [ ] T006 [P] Update `WebSpark.ArtSpark.Agent` persona contracts consumed by multiple stories
- [ ] T007 Align `WebSpark.ArtSpark.Client` models with new API fields required by the Demo
- [ ] T008 Expand Serilog configuration or health checks to cover the new surface area
- [ ] T009 Document configuration secrets and add placeholders to `appsettings.json` / deployment settings

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - [Title] (Priority: P1) 🎯 MVP

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 1 (OPTIONAL - only if tests requested) ⚠️

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T010 [P] [US1] Add integration test in `WebSpark.ArtSpark.Tests/Integration/[Feature]Tests.cs`
- [ ] T011 [P] [US1] Add UI test in `WebSpark.ArtSpark.Demo/Tests/Pages/[Page]Tests.cs`

### Implementation for User Story 1

- [ ] T012 [P] [US1] Update Demo controller in `WebSpark.ArtSpark.Demo/Controllers/[Controller]Controller.cs`
- [ ] T013 [P] [US1] Adjust view or component in `WebSpark.ArtSpark.Demo/Views/[Area]/[View].cshtml`
- [ ] T014 [US1] Add supporting service in `WebSpark.ArtSpark.Demo/Services/[Service].cs`
- [ ] T015 [US1] Update client/agent contract if required in `WebSpark.ArtSpark.Client/...` or `WebSpark.ArtSpark.Agent/...`
- [ ] T016 [US1] Ensure validation, authorization, and guardrails are enforced
- [ ] T017 [US1] Add Serilog logging and telemetry for new flow

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - [Title] (Priority: P2)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 2 (OPTIONAL - only if tests requested) ⚠️

- [ ] T018 [P] [US2] Add contract test in `WebSpark.ArtSpark.Tests/Contracts/[Client]ContractTests.cs`
- [ ] T019 [P] [US2] Add persona regression test in `WebSpark.ArtSpark.Agent/Tests/[Persona]Tests.cs`

### Implementation for User Story 2

- [ ] T020 [P] [US2] Extend Demo view models in `WebSpark.ArtSpark.Demo/Models/[Model].cs`
- [ ] T021 [US2] Update agent pipeline in `WebSpark.ArtSpark.Agent/Services/[Service].cs`
- [ ] T022 [US2] Modify client request builder in `WebSpark.ArtSpark.Client/Clients/[Client].cs`
- [ ] T023 [US2] Wire Demo UI to the new service while keeping US1 behavior intact

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - [Title] (Priority: P3)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 3 (OPTIONAL - only if tests requested) ⚠️

- [ ] T024 [P] [US3] Add API regression test in `WebSpark.ArtSpark.Tests/Api/[Endpoint]Tests.cs`
- [ ] T025 [P] [US3] Add accessibility snapshot test for Demo page (if applicable)

### Implementation for User Story 3

- [ ] T026 [P] [US3] Extend supporting model in `WebSpark.ArtSpark.Demo/Models/[Model].cs`
- [ ] T027 [US3] Update Demo Razor component or partial view
- [ ] T028 [US3] Add configuration or migration updates if data model shifts

**Checkpoint**: All user stories should now be independently functional

---

[Add more user story phases as needed, following the same pattern]

---

## Phase N: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] TXXX [P] Documentation updates in `docs/` and relevant README files
- [ ] TXXX Code cleanup and refactoring across touched projects
- [ ] TXXX Performance optimization and load verification for affected endpoints
- [ ] TXXX [P] Additional unit/integration/UI tests in appropriate `.Tests` projects
- [ ] TXXX Security and privacy review (AI safeguards, data handling)
- [ ] TXXX Validate quickstart/demo instructions still succeed end-to-end

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - May integrate with US1 but should be independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - May integrate with US1/US2 but should be independently testable

### Within Each User Story

- Tests (if included) MUST be written and FAIL before implementation
- Models before services
- Services before endpoints
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- All tests for a user story marked [P] can run in parallel
- Models within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together (if tests requested):
Task: "Contract test for [endpoint] in WebSpark.ArtSpark.Tests/Contracts/[Client]ContractTests.cs"
Task: "Integration test for [user journey] in WebSpark.ArtSpark.Tests/Integration/[Feature]Tests.cs"

# Launch all models for User Story 1 together:
Task: "Update controller in WebSpark.ArtSpark.Demo/Controllers/[Controller]Controller.cs"
Task: "Adjust view in WebSpark.ArtSpark.Demo/Views/[Area]/[View].cshtml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
