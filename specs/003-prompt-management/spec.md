# Feature Specification: AI Persona Prompt Management System

**Feature Branch**: `003-prompt-management`  
**Created**: 2025-11-16  
**Status**: Draft  
**Input**: User description: "Create Prompt management system for the various prompt personas. Store the prompts in a /data folder with the prompt name and markdown extension .md load the prompts from the /data folder when needed"  
**Live Demo Impact**: Decouples persona system prompts from code, enabling non-developers to refine AI chat personas, improving user experience quality and cultural sensitivity without deployments.  
**Related Projects**: Agent (primary), Demo (consumer via dependency), Console (test harness)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Content Authors Update Persona Prompts Without Code Changes (Priority: P1)

Museum curators and content specialists need to refine AI persona system prompts to improve cultural sensitivity, accuracy, or tone based on visitor feedback. Currently, prompts are hardcoded in C# classes, requiring developer involvement and deployments for every content change.

**Why this priority**: Enables rapid iteration on AI quality and cultural sensitivity—critical for the live Demo's educational mission. Reduces developer bottleneck for content improvements.

**Independent Test**: Content author updates `artwork-persona.md` with revised cultural sensitivity language, restarts service, and validates new prompt behavior in AI chat without touching code.

**Acceptance Scenarios**:

1. **Given** a persona prompt file exists at `/data/prompts/artwork-persona.md`, **When** content author edits the file, **Then** next AI chat session uses updated prompt without recompilation
2. **Given** prompt file contains token `{artwork.Title}`, **When** system loads prompt for specific artwork, **Then** token is replaced with actual artwork title
3. **Given** prompt file is missing or corrupted, **When** system attempts to load, **Then** system logs error and falls back to default hardcoded prompt

---

### User Story 2 - Developers Test Persona Variations Locally (Priority: P2)

Developers iterating on persona behavior need to experiment with prompt variations without rebuilding the Agent library for each test.

**Why this priority**: Accelerates AI feature development cycle and enables A/B testing of prompt strategies. Supports Console test harness workflows.

**Independent Test**: Developer modifies `curator-persona.md`, runs Console test harness, and observes different AI responses without recompiling Agent project.

**Acceptance Scenarios**:

1. **Given** Developer runs Console with `--persona Curator`, **When** prompt file is modified mid-session, **Then** next chat request loads updated prompt (if reload enabled)
2. **Given** multiple prompt variants exist in `/data/prompts/test/`, **When** Developer specifies variant via config, **Then** system loads alternate prompt file

---

### User Story 3 - Operators Monitor Prompt Versions in Production (Priority: P3)

Site operators need to understand which prompt versions are active in production for debugging and compliance audits.

**Why this priority**: Supports operational excellence and AI governance. Less critical than content authoring but essential for production confidence.

**Independent Test**: Operator views Demo footer build metadata or logs and confirms active prompt file versions and last-modified timestamps.

**Acceptance Scenarios**:

1. **Given** prompt files are loaded at startup, **When** system logs initialization, **Then** log entries include prompt file paths and MD5 hashes
2. **Given** prompt load fails, **When** system falls back to default, **Then** warning logged with fallback indicator

---

### Edge Cases

- **Missing prompt file**: System falls back to hardcoded default prompt with logged warning. Demo continues operating.
- **Malformed prompt file**: System validates basic structure (detects empty files, encoding issues), logs error, uses fallback.
- **File system permissions**: If Demo process cannot read `/data/prompts/`, system logs error and uses defaults. Configuration validates path on startup.
- **Concurrent edits**: File system watches detect changes; system reloads prompts on next request (optional feature for dev/staging).
- **Token injection attacks**: Template engine validates token names against whitelist (e.g., `artwork.*`, `artist.*`). Invalid tokens logged and rendered as empty strings.
- **Upstream service failures**: Prompt loading errors do not prevent AI chat from functioning—fallback ensures graceful degradation.

## Demo Surface & Dependencies *(mandatory)*

- **Affected Pages/Routes**: 
  - `/Artwork/Details/{id}` (AI chat interface) - No UI changes, but improved AI response quality visible to users
  - No new routes added; feature is infrastructure for existing chat

- **Feature Flags / Configuration**: 
  - New `ArtSparkAgent:Prompts:DataPath` setting (default: `./data/prompts`)
  - New `ArtSparkAgent:Prompts:EnableHotReload` setting (default: `false` for production, `true` for dev)
  - New `ArtSparkAgent:Prompts:FallbackToDefault` setting (default: `true`)

- **Shared Library Touchpoints**: 
  - **Agent**: New `IPromptLoader` service and `PromptLoader` implementation. Existing `IPersonaHandler` implementations consume loaded prompts instead of generating inline.
  - **Demo**: No contract changes; updates service registration to configure prompt loader. Existing `AddArtSparkAgent()` extension handles new services.
  - **Console**: Can leverage new prompt loading for testing; no breaking changes.

- **Rollback Plan**: 
  - Remove `/data/prompts/` folder to force fallback to hardcoded defaults
  - Toggle `FallbackToDefault` to `true` in config
  - Revert Agent library to previous version (hardcoded prompts remain in code for fallback)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Agent MUST load persona system prompts from markdown files located at configurable data path (default: `./data/prompts/`)
- **FR-002**: Agent MUST support file naming convention: `{persona-name}-persona.md` (e.g., `artwork-persona.md`, `curator-persona.md`, `artist-persona.md`, `historian-persona.md`)
- **FR-003**: System MUST replace template tokens in prompt files (format: `{entity.property}`) with actual artwork data at runtime (e.g., `{artwork.Title}`, `{artwork.ArtistDisplay}`)
- **FR-004**: System MUST validate template tokens against whitelist of allowed properties to prevent injection attacks
- **FR-005**: System MUST fall back to hardcoded default prompts if file loading fails, with logged warnings including file path and error details
- **FR-006**: System MUST log prompt initialization events at startup, including loaded file paths, file sizes, and content hashes (MD5) for audit trails
- **FR-007**: Prompt loader MUST support optional hot-reload capability for development environments (detect file changes and reload on next request)
- **FR-008**: Demo MUST continue serving AI chat requests even when prompt files are missing or corrupted (graceful degradation via fallbacks)
- **FR-009**: Configuration validation MUST check prompt data path existence at startup and log warnings if path is invalid
- **FR-010**: Agent MUST preserve existing `IPersonaHandler` interface to maintain backward compatibility with Demo and Console consumers

### Key Entities *(include if feature involves data)*

- **PromptTemplate**: Represents loaded prompt content with metadata (file path, last modified timestamp, content hash, raw markdown text)
- **PersonaPromptConfiguration**: Maps persona enum values to file names and fallback prompts
- **TokenReplacementContext**: Holds artwork data and other contextual information for template token substitution

## AI & Cultural Safeguards *(required when using AI personas or content generation)*

- **Prompt & Persona Controls**: 
  - Prompt files MUST include sections for "CULTURAL SENSITIVITY" and "CONVERSATION GUIDELINES" (validated by loader)
  - Content authors receive documentation on inclusive language and cultural respect standards
  - Prompt loader logs warnings if required sections are missing from loaded files

- **Content Filtering**: 
  - Template token validation prevents arbitrary code injection
  - Prompt files stored in controlled repository location (not user-uploaded content)
  - File system access restricted to read-only for service process

- **Data Handling**: 
  - Prompt files do not contain user data or conversation history
  - Only artwork metadata used in token replacement (public Art Institute of Chicago API data)
  - No PII or sensitive data in prompt templates

- **Compliance Links**: 
  - Content authoring guide to be created at `docs/copilot/prompt-authoring-guide.md`
  - Updates to existing `docs/AI-Chat-Personas-Implementation.md` to document new prompt management architecture

## Observability & Testing Plan *(mandatory)*

- **Automated Tests**: 
  - **Agent.Tests**: New `PromptLoaderTests` class covering file loading, fallback behavior, token replacement, validation failures
  - **Agent.Tests**: Updated `PersonaFactoryTests` to verify integration with prompt loader
  - **Agent.Tests**: New `PromptTemplateTokenTests` for injection attack prevention
  - **Demo.Tests**: Integration test verifying AI chat works with both file-based and fallback prompts

- **Logging & Metrics**: 
  - Serilog structured events: `PromptLoaded`, `PromptLoadFailed`, `PromptFallbackUsed`, `PromptTokenValidationFailed`
  - Log properties: `PersonaType`, `FilePath`, `FileSize`, `ContentHash`, `ErrorDetails`
  - Performance metric: Prompt load duration (should be <50ms for file read + parse)

- **Manual Validation**: 
  - Modify each persona prompt file on staging, restart service, verify AI chat reflects changes
  - Delete prompt file, verify fallback behavior and warning logs
  - Test hot-reload in development environment by editing file mid-conversation

## Documentation & Release Updates *(mandatory)*

- **Docs to Update**: 
  - Create `docs/copilot/prompt-authoring-guide.md` with markdown structure guidelines, token reference, cultural sensitivity checklist
  - Update `WebSpark.ArtSpark.Agent/README.md` with prompt management section and configuration examples
  - Update `docs/AI-Chat-Personas-Implementation.md` with new architecture diagram showing prompt loading flow
  - Update root `README.md` configuration section with new `ArtSparkAgent:Prompts` settings

- **Release Notes**: 
  - "AI Persona prompts now externalized to `/data/prompts/` for easier content updates without code changes"
  - "Content authors can refine AI chat personas by editing markdown files"
  - "Added operational logging for prompt versions and fallback behavior"

- **Post-Deployment Checks**: 
  - Verify `/data/prompts/` folder deployed with initial markdown files
  - Check logs for successful prompt initialization on all personas
  - Test one persona prompt edit to confirm reload works (if enabled)
  - Validate footer build metadata includes prompt versioning info

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Content authors successfully update persona prompts and verify changes in Demo AI chat within 5 minutes (no developer involvement required)
- **SC-002**: Prompt loading completes in under 50ms per persona during service initialization (measured via Serilog timing logs)
- **SC-003**: AI chat maintains 100% availability during prompt file failures via fallback mechanism (zero user-facing errors)
- **SC-004**: All four persona prompt files successfully migrate from hardcoded C# to markdown with zero functional regression (validated by existing Agent test suite)
- **SC-005**: Prompt authoring guide receives review from at least two content stakeholders for clarity and completeness before release
