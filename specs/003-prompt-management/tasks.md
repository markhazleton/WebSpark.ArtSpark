---

description: "Implementation tasks for AI Persona Prompt Management System"
---

# Tasks: AI Persona Prompt Management System

**Input**: Design documents from `/specs/003-prompt-management/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare repository structure and baseline configuration for persona prompt files.

- [X] T001 Restore NuGet packages for `WebSpark.ArtSpark.sln`
- [X] T002 Create prompt directory scaffold `WebSpark.ArtSpark.Demo/prompts/agents/`
- [X] T003 Add `ArtSparkAgent:Prompts` defaults (DataPath, FallbackToDefault, DefaultMetadata) to `WebSpark.ArtSpark.Demo/appsettings.Development.json`
- [X] T004 Mirror production-safe prompt settings in `WebSpark.ArtSpark.Demo/appsettings.json`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish core configuration, options, and DI hooks that all user stories depend on.

- [X] T005 Implement prompt options model with metadata defaults in `WebSpark.ArtSpark.Agent/Configuration/PromptOptions.cs`
- [X] T006 [P] Define persona prompt configuration map with token whitelist and default metadata in `WebSpark.ArtSpark.Agent/Configuration/PersonaPromptConfiguration.cs`
- [X] T007 Update `WebSpark.ArtSpark.Agent/Extensions/ServiceCollectionExtensions.cs` to register prompt options, loader, and decorator services
- [X] T008 Configure prompt options binding and fallback toggles in `WebSpark.ArtSpark.Demo/Program.cs`
- [X] T009 [P] Add `PromptOptionsValidationTests` covering defaults, key merging, file name rules, token whitelist enforcement, and invalid path handling in `WebSpark.ArtSpark.Tests/Agent/Configuration/PromptOptionsValidationTests.cs`
- [X] T010a [P] Implement prompt data path validation at startup with warning logs when directory is missing or unreadable
- [X] T010b [P] Add prompt path validation coverage in `PromptOptionsValidationTests.cs`
- [X] T010 [P] Document configuration schema in `specs/003-prompt-management/contracts/appsettings.ArtSparkAgent.Prompts.schema.json`

**Checkpoint**: Prompt configuration, startup path validation, and DI scaffolding are in place for all personas.

---

## Phase 3: User Story 1 - Content Authors Update Persona Prompts Without Code Changes (Priority: P1) 🎯 MVP

**Goal**: Load persona prompts from markdown files so content editors can change responses without redeploying.

**Independent Test**: Edit `prompts/agents/artspark.artwork.prompt.md`, restart Demo, and confirm AI chat uses updated language.

### Tests for User Story 1

- [X] T011 [P] [US1] Add `PromptLoaderTests` covering markdown load, front matter parsing, and strict fallback in `WebSpark.ArtSpark.Tests/Agent/Services/PromptLoaderTests.cs`
- [X] T012 [P] [US1] Add metadata merge tests in `WebSpark.ArtSpark.Tests/Agent/Services/PromptLoaderMetadataTests.cs`
- [X] T013 [P] [US1] Add token validation tests in `WebSpark.ArtSpark.Tests/Agent/Services/PromptTemplateTokenTests.cs`
- [X] T014 [P] [US1] Extend `PersonaFactoryTests` for file-backed handler resolution in `WebSpark.ArtSpark.Tests/Agent/Personas/PersonaFactoryTests.cs`

### Implementation for User Story 1

- [X] T015 [US1] Implement `PromptMetadataParser` for YAML front matter in `WebSpark.ArtSpark.Agent/Services/PromptMetadataParser.cs`
- [X] T016 [P] [US1] Update `WebSpark.ArtSpark.Agent/Models/PromptTemplate.cs` to store metadata overrides and validation state
- [X] T017 [US1] Implement file-backed `PromptLoader` to parse markdown, enforce sections, validate tokens, and merge metadata in `WebSpark.ArtSpark.Agent/Services/PromptLoader.cs`
- [X] T018 [P] [US1] Create `FileBackedPersonaHandler` decorator in `WebSpark.ArtSpark.Agent/Personas/FileBackedPersonaHandler.cs`
- [X] T019 [P] [US1] Expose fallback prompt accessors in persona classes within `WebSpark.ArtSpark.Agent/Personas/`
- [X] T020 [US1] Update `WebSpark.ArtSpark.Agent/Personas/PersonaFactory.cs` to resolve decorators via `IPromptLoader`
- [X] T021 [US1] Seed markdown prompt files with YAML front matter in `WebSpark.ArtSpark.Demo/prompts/agents/`
- [X] T022 [US1] Register prompt loader pipeline inside `WebSpark.ArtSpark.Demo/Program.cs`

**Checkpoint**: Persona prompts load from files with safe fallback, enabling non-developer updates.

---

## Phase 4: User Story 2 - Developers Test Persona Variations Locally (Priority: P2)

**Goal**: Support hot reload and variant selection so engineers can iterate on prompts without rebuilding.

**Independent Test**: Modify `artspark.curator.prompt.md` while Console harness runs and confirm subsequent responses reflect changes when hot reload enabled.

### Tests for User Story 2

- [X] T023 [P] [US2] Add hot reload regression tests covering body and metadata reload in `WebSpark.ArtSpark.Tests/Agent/Services/PromptLoaderHotReloadTests.cs`
- [X] T024 [P] [US2] Add console harness tests for prompt variants in `WebSpark.ArtSpark.Tests/Console/PromptVariantTests.cs`

### Implementation for User Story 2

- [X] T025 [US2] Integrate `PhysicalFileProvider` change tokens into `WebSpark.ArtSpark.Agent/Services/PromptLoader.cs`
- [X] T026 [P] [US2] Emit `ConfigurationReloaded` log events when metadata overrides change in `WebSpark.ArtSpark.Agent/Services/PromptLoader.cs`
- [X] T027 [P] [US2] Extend prompt options to support variants path in `WebSpark.ArtSpark.Agent/Configuration/PromptOptions.cs`
- [X] T028 [US2] Update Console harness configuration in `WebSpark.ArtSpark.Console/Program.cs` to consume prompt options and variant selection
- [X] T029 [P] [US2] Set development defaults (`EnableHotReload`, `VariantsPath`) in `WebSpark.ArtSpark.Demo/appsettings.Development.json`

**Checkpoint**: Local developers can iterate on prompts live without rebuilding libraries.

---

## Phase 5: User Story 3 - Operators Monitor Prompt Versions in Production (Priority: P3)

**Goal**: Provide operational insight into active prompt files, including hashes and fallback usage.

**Independent Test**: Review startup logs and Demo footer metadata to confirm prompt file paths, hashes, and status are reported.

### Tests for User Story 3

- [X] T030 [P] [US3] Add audit logging tests in `WebSpark.ArtSpark.Tests/Agent/Services/PromptAuditLoggingTests.cs`
- [X] T031 [US3] Add Demo integration test verifying file-based, metadata overrides, and fallback prompts in `WebSpark.ArtSpark.Tests/Demo/PromptFallbackTests.cs`

### Implementation for User Story 3

- [X] T032 [US3] Emit structured Serilog events (`PromptLoaded`, `PromptLoadFailed`, `PromptFallbackUsed`, `PromptTokenValidationFailed`, `ConfigurationReloaded`) with metadata in `WebSpark.ArtSpark.Agent/Services/PromptLoader.cs`
- [X] T033 [P] [US3] Add fallback warning handling that increments audit counters in `WebSpark.ArtSpark.Agent/Services/PromptLoader.cs`
- [X] T034 [P] [US3] Extend `WebSpark.ArtSpark.Demo/Services/BuildInfoService.cs` to surface prompt file hashes, metadata overrides, and fallback status
- [X] T035 [US3] Render prompt metadata in `WebSpark.ArtSpark.Demo/Views/Shared/Components/Footer/Default.cshtml`
- [X] T036 [P] [US3] Add operator-facing log enrichment in `WebSpark.ArtSpark.Agent/Services/PromptLoader.cs` to include persona type and configuration source

**Checkpoint**: Operators can audit prompt versions and detect fallback conditions in production.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Finalize documentation, validation, and regression coverage across the feature.

- [X] T037 Update authoring guidance in `docs/copilot/prompt-authoring-guide.md`
- [X] T038 [P] Coordinate review of `docs/copilot/prompt-authoring-guide.md` with content stakeholders
- [X] T039 [P] Document prompt management usage in `WebSpark.ArtSpark.Agent/README.md`
- [X] T040 [P] Refresh configuration instructions in `README.md`
- [X] T041 Update `docs/AI-Chat-Personas-Implementation.md` with prompt management architecture changes
- [X] T042 Run full test suite `dotnet test WebSpark.ArtSpark.Tests`
- [X] T043 [P] Verify quickstart steps and hot reload workflow in `specs/003-prompt-management/quickstart.md`
- [X] T044 Capture prompt load performance metrics (<50ms) via Serilog log review or benchmark script
- [X] T045 Draft release-note entry summarizing externalized prompts, metadata overrides, and logging enhancements

---

## Dependencies & Execution Order

- **Phase 1 → Phase 2**: Setup must complete before foundational work (directory and config assumptions).
- **Phase 2 → Phases 3-5**: Prompt options and DI registration are prerequisites for every user story.
- **User Story Sequence**: US1 (P1) delivers MVP; US2 and US3 may start after Phase 2 but should not block US1 completion.
- **Phase 6**: Runs after targeted user stories reach completion.

## Parallel Execution Opportunities

- `[P]` tasks operate on different files or test suites; run concurrently once dependencies are satisfied.
- After Phase 2 completes, parallelize user stories by team: US1 (Agent + Demo), US2 (Agent + Console), US3 (Agent + Demo UI/Services).
- Documentation tasks `[P]` in Phase 6 can progress alongside final verification once implementation stabilizes.

## Implementation Strategy

1. Complete Setup and Foundational phases to establish configuration, DI, and token whitelists.
2. Deliver User Story 1 as MVP, ensuring prompt files load from disk with safe fallbacks and passing tests.
3. Layer in User Story 2 for hot reload and variant support, followed by User Story 3 for operational logging and metadata surfacing.
4. Finish with cross-cutting polish, documentation, and full regression testing before deployment.
