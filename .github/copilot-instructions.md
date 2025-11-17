# Copilot Instructions for WebSpark.ArtSpark

## Project Mission
- WebSpark.ArtSpark is a multi-project .NET 9 solution centered on the public web experience at https://artspark.markhazleton.com (`WebSpark.ArtSpark.Demo`).
- `WebSpark.ArtSpark.Client`, `WebSpark.ArtSpark.Agent`, and `WebSpark.ArtSpark.Console` exist to serve the Demo. Any change must articulate its Demo impact before touching supporting libraries.

## Core Delivery Principles
1. **Live Demo First**: Prioritize features, fixes, and refactors that improve or safeguard the Demo. When edits begin in libraries, include the Demo integration path in the same change set.
2. **Contract-Stable Shared Libraries**: Preserve public contracts in Client/Agent/Console unless coordinated Demo updates land simultaneously. Flag breaking changes, update release notes, and add Demo coverage.
3. **Production Reliability & Observability**: Add or update automated tests (unit, integration, UI) for every change. Keep Serilog logging, build metadata, Application Insights hooks, and EF Core migrations consistent.
4. **Responsible AI & Cultural Respect**: Enforce persona guardrails, content moderation, and data-handling rules before exposing AI responses. Reference the guidance in `docs/` (e.g., *AI-Chat-Personas-Implementation.md*).
5. **Transparent Documentation & Deployment Records**: Update relevant README and `docs/` pages, plus operational notes such as `docs/Documentation-Update-Summary.md`, whenever behavior, configuration, or release steps change.

## Coding Standards
- Target **.NET 9 / C# 13**; prefer async/await, minimal allocations, and idiomatic ASP.NET Core patterns.
- Keep files ASCII unless an existing file already uses Unicode. Add succinct comments only for non-obvious logic.
- Maintain dependency direction: Demo → Agent/Client; Agent → Client; Console → Client. Prohibit reverse references without governance approval.
- Apply consistent naming (PascalCase for types, camelCase for locals/parameters). Use `readonly`/`sealed` where it improves clarity.
- When introducing configuration, add strongly typed options classes and validate via `IOptions<T>`. Document secrets or environment keys in `README.md` or docs.

## Testing & Quality Gates
- Run `dotnet test WebSpark.ArtSpark.sln` (or targeted projects) before merge; capture notable results in PRs.
- Extend existing test suites instead of duplicating coverage. Prefer deterministic tests; mock external APIs when appropriate.
- For EF Core changes, create migrations under `WebSpark.ArtSpark.Demo/Migrations/` and ensure `ApplicationDbContext` updates are reflected in seed/config code.
- Validate logging by ensuring new code paths emit structured Serilog events with relevant context (e.g., correlation IDs, user interactions).

## AI Feature Guidance
- Review persona prompts and response handling in `WebSpark.ArtSpark.Agent` before modifications.
- Implement safety layers: prompt sanitization, OpenAI moderation fallback, and culturally aware messaging.
- Avoid storing personal data; clarify retention or anonymization in specs and documentation.

## Documentation & Release Notes
- Update:
  - `README.md` (root and project-specific) for new capabilities or setup steps.
  - `docs/` implementation guides when workflows change.
  - Deployment logs/build metadata displayed in the Demo footer or related view components.
- Surface SemVer bumps for shared libraries and record Demo release info in operational docs.

## Documentation Standards

### File Organization

**Persistent Reference Documentation:**
- Lives in `docs/copilot` for material that must persist across sessions (architecture guides, API patterns, template authoring guides, business processes).
- Keep these references updated as the solution evolves.

**Session Documentation:**
- All Copilot-created session `.md` files MUST be placed in `docs/copilot/YYYY-MM-DD/` (ISO date) folders.
- Session docs include implementation summaries, reviews, analyses, planning artifacts, and similar temporary outputs.
- Review session folders periodically—promote enduring insights into `docs/copilot`, remove stale notes.

**Exceptions:**
- Only the root `README.md` and `.github/copilot-instructions.md` may live outside the structured `docs/copilot` hierarchy.
- Use descriptive filenames (e.g., `care-type-implementation-summary.md`, `template-validation-analysis.md`).

## Pull Request Expectations
- Summarize Demo impact first, then supporting library changes.
- List updated tests (`dotnet test` output) and manual verification (e.g., pages exercised, accessibility checks).
- Highlight any AI persona updates, new guardrails, or documentation links.
- Reference applicable specification/plan/tasks artifacts generated via SpecKit commands.

Adhering to these instructions keeps Copilot suggestions aligned with the WebSpark.ArtSpark constitution and maintains the live experience users rely on.