---

description: "Implementation tasks for AI Persona Prompt Management System"
---

# Tasks: AI Persona Prompt Management System

**Input**: Design documents from `/specs/003-prompt-management/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare repository structure and configuration required for prompt file loading.

- [ ] T001 Restore NuGet packages for `WebSpark.ArtSpark.sln`
- [ ] T002 Create prompt directory scaffold `WebSpark.ArtSpark.Demo/prompts/agents/`
- [ ] T003 Verify `ArtSparkAgent:Prompts` section in `WebSpark.ArtSpark.Demo/appsettings.Development.json` includes `DataPath`, `EnableHotReload`, and `FallbackToDefault`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish core configuration, options, and DI hooks that all user stories depend on.

- [ ] T004 Implement `PromptOptions` with configuration validation in `WebSpark.ArtSpark.Agent/Configuration/PromptOptions.cs`
- [ ] T005 [P] Define persona token whitelist map in `WebSpark.ArtSpark.Agent/Configuration/PersonaPromptConfiguration.cs`
- [ ] T006 Update `WebSpark.ArtSpark.Agent/Extensions/ServiceCollectionExtensions.cs` to expose `AddPromptManagement` registration
- [ ] T007 Configure prompt options binding and fallback toggles inside `WebSpark.ArtSpark.Demo/Program.cs`
- [ ] T008 [P] Add configuration validation unit tests in `WebSpark.ArtSpark.Agent.Tests/Configuration/PromptOptionsValidationTests.cs`

**Checkpoint**: Prompt configuration and DI scaffolding are in place for all personas.

---

## Phase 3: User Story 1 - Content Authors Update Persona Prompts Without Code Changes (Priority: P1) 🎯 MVP

**Goal**: Load persona prompts from markdown files so content editors can change responses without redeploying.

**Independent Test**: Edit `prompts/agents/artspark.artwork.prompt.md`, restart Demo, and confirm AI chat uses updated language.

### Tests for User Story 1

- [ ] T009 [P] [US1] Add `PromptLoaderTests` covering load, fallback, and token substitution in `WebSpark.ArtSpark.Agent.Tests/Services/PromptLoaderTests.cs`
- [ ] T010 [P] [US1] Extend `PersonaFactoryTests` to validate file-backed handlers in `WebSpark.ArtSpark.Agent.Tests/Personas/PersonaFactoryTests.cs`

### Implementation for User Story 1

- [ ] T011 [US1] Implement file-backed `PromptLoader` service in `WebSpark.ArtSpark.Agent/Services/PromptLoader.cs`
- [ ] T012 [P] [US1] Create `FileBackedPersonaHandler` decorator in `WebSpark.ArtSpark.Agent/Personas/FileBackedPersonaHandler.cs`
- [ ] T013 [P] [US1] Update `WebSpark.ArtSpark.Agent/Personas/PersonaFactory.cs` to resolve decorators via `IPromptLoader`
- [ ] T014 [P] [US1] Expose fallback prompt accessors in `WebSpark.ArtSpark.Agent/Personas/ArtworkPersona.cs`, `ArtistPersona.cs`, `CuratorPersona.cs`, and `HistorianPersona.cs`
- [ ] T015 [US1] Seed markdown prompt files in `WebSpark.ArtSpark.Demo/prompts/agents/artspark.{artwork|artist|curator|historian}.prompt.md` with required sections
- [ ] T016 [US1] Register prompt loader integration inside `WebSpark.ArtSpark.Demo/Program.cs` via `AddArtSparkAgent()` pipeline

**Checkpoint**: Persona prompts load from files with safe fallback, enabling non-developer updates.

---

## Phase 4: User Story 2 - Developers Test Persona Variations Locally (Priority: P2)

**Goal**: Support hot reload and variant selection so engineers can iterate on prompts without rebuilding.

**Independent Test**: Modify `artspark.curator.prompt.md` while Console harness runs and confirm subsequent responses reflect changes when hot reload enabled.

### Tests for User Story 2

- [ ] T017 [P] [US2] Add hot reload regression tests in `WebSpark.ArtSpark.Agent.Tests/Services/PromptLoaderHotReloadTests.cs`

### Implementation for User Story 2

- [ ] T018 [US2] Integrate `PhysicalFileProvider`/`IChangeToken` hot reload logic in `WebSpark.ArtSpark.Agent/Services/PromptLoader.cs`
- [ ] T019 [P] [US2] Support persona variant filenames in `WebSpark.ArtSpark.Agent/Configuration/PromptOptions.cs`
- [ ] T020 [US2] Update Console harness configuration in `WebSpark.ArtSpark.Console/Program.cs` to honor `ArtSparkAgent:Prompts` options
- [ ] T021 [P] [US2] Set development defaults (hot reload true, variants path) in `WebSpark.ArtSpark.Demo/appsettings.Development.json`

**Checkpoint**: Local developers can iterate on prompts live without rebuilding libraries.

---

## Phase 5: User Story 3 - Operators Monitor Prompt Versions in Production (Priority: P3)

**Goal**: Provide operational insight into active prompt files, including hashes and fallback usage.

**Independent Test**: Review startup logs and Demo footer metadata to confirm prompt file paths, hashes, and status are reported.

### Tests for User Story 3

- [ ] T022 [P] [US3] Add audit logging tests in `WebSpark.ArtSpark.Agent.Tests/Services/PromptAuditLoggingTests.cs`

### Implementation for User Story 3

- [ ] T023 [US3] Emit structured Serilog events with path, size, and hash in `WebSpark.ArtSpark.Agent/Services/PromptLoader.cs`
- [ ] T024 [P] [US3] Extend `WebSpark.ArtSpark.Demo/Services/BuildInfoService.cs` to expose prompt metadata
- [ ] T025 [US3] Render prompt versions in `WebSpark.ArtSpark.Demo/Views/Shared/Components/Footer/Default.cshtml`
- [ ] T026 [P] [US3] Add fallback warning handling to `WebSpark.ArtSpark.Agent/Services/PromptLoader.cs` to trigger `PromptFallbackUsed`

**Checkpoint**: Operators can audit prompt versions and detect fallback conditions in production.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Finalize documentation, validation, and regression coverage across the feature.

- [ ] T027 Update authoring guidance in `docs/copilot/prompt-authoring-guide.md`
- [ ] T028 [P] Document prompt management usage in `WebSpark.ArtSpark.Agent/README.md`
- [ ] T029 [P] Refresh configuration instructions in `README.md`
- [ ] T030 Run full test suite `dotnet test WebSpark.ArtSpark.Tests`
- [ ] T031 [P] Verify quickstart steps and hot reload workflow in `specs/003-prompt-management/quickstart.md`

---

## Dependencies & Execution Order

- **Phase 1 → Phase 2**: Setup must complete before foundational work (directory and config assumptions).
- **Phase 2 → Phases 3-5**: Prompt options and DI registration are prerequisites for every user story.
- **User Story Sequence**: US1 (P1) delivers MVP; US2 and US3 may start after Phase 2 but should not block US1 completion.
- **Phase 6**: Runs after targeted user stories reach completion.

## Parallel Execution Opportunities

- Tasks marked `[P]` operate on distinct files and can run concurrently once their phase is unlocked.
- After Phase 2, teams can tackle US1, US2, and US3 in parallel with clear ownership of Agent vs. Demo vs. Console changes.
- Testing tasks marked `[P]` can execute simultaneously across different test classes to speed validation.

## Implementation Strategy

1. Complete Setup and Foundational phases to establish configuration, DI, and token whitelists.
2. Deliver User Story 1 as MVP, ensuring prompt files load from disk with safe fallbacks and passing tests.
3. Layer in User Story 2 for hot reload and variant support, followed by User Story 3 for operational logging and metadata surfacing.
4. Finish with cross-cutting polish, documentation, and full regression testing before deployment.
