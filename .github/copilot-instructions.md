# Copilot Instructions for WebSpark.ArtSpark

## Project Mission
WebSpark.ArtSpark is a **.NET 10 (Preview)** multi-project solution centered on the public web experience at https://artspark.markhazleton.com (`WebSpark.ArtSpark.Demo`). The solution provides a complete .NET ecosystem for the Art Institute of Chicago API with revolutionary AI chat capabilities featuring four distinct personas.

**Project Hierarchy**: Demo → Agent/Client; Agent → Client; Console → Client. All supporting libraries exist to serve the Demo—articulate Demo impact before touching library contracts.

## Core Delivery Principles
1. **Live Demo First**: Prioritize features that improve the live public experience. Changes to Client/Agent/Console require corresponding Demo integration in the same changeset.
2. **Contract-Stable Libraries**: Preserve public contracts in shared libraries. Breaking changes require coordinated updates + explicit Demo coverage + release notes.
3. **Production Reliability**: Update automated tests for every change. Maintain Serilog logging, build metadata display, and EF Core migrations under `WebSpark.ArtSpark.Demo/Migrations/`.
4. **Responsible AI**: Enforce persona guardrails (`WebSpark.ArtSpark.Agent/Personas/`), input validation, and cultural respect. Reference `docs/AI-Chat-Personas-Implementation.md`.
5. **Documentation Discipline**: Update `README.md` files and `docs/` guides when behavior/configuration changes. Reflect version bumps in footer build metadata.

## Architecture & Patterns

### Solution Structure
```
Demo (ASP.NET Core MVC + Identity)
├── Agent (AI chat with Microsoft Semantic Kernel + OpenAI)
│   └── Client (Art Institute of Chicago API wrapper)
└── Console (CLI test harness)
    └── Client
```

### Key Integration Patterns
- **DI Registration in Program.cs**: Use extension methods (`AddArtSparkAgent()`, `AddBootswatchThemeSwitcher()`) for clean service registration. See `WebSpark.ArtSpark.Demo/Program.cs` lines 75-105.
- **Decorator Pattern**: HttpClient utilities use layered decorators (base → Polly resilience → telemetry → cache). See `RegisterHttpClientUtilities()` in `Program.cs`.
- **ViewComponents**: UI modules like `FooterViewComponent` inject services (`IBuildInfoService`) and render via `Views/Shared/Components/{Name}/Default.cshtml`.
- **AI Personas**: Factory pattern creates persona handlers (`IPersonaHandler`) from enum values. Each persona (`ArtworkPersona`, `ArtistPersona`, `CuratorPersona`, `HistorianPersona`) generates unique system prompts for OpenAI.
- **EF Core Identity**: `ArtSparkDbContext : IdentityDbContext<ApplicationUser>` with SQLite. Migrations in `WebSpark.ArtSpark.Demo/Migrations/`.

## Coding Standards
- **Target .NET 10 (Preview)** / C# 13. Use async/await, minimal allocations, top-level statements, and primary constructors where appropriate.
- **Naming**: PascalCase for types/properties, camelCase for locals/parameters. Use `readonly`/`sealed` to signal immutability.
- **Configuration**: Strongly typed options with `IOptions<T>` validation. Store secrets in user-secrets (dev) or Azure Key Vault (prod). Document in `appsettings.json` + README.
- **ASCII by default**: Only use Unicode when existing files require it. Add comments sparingly for non-obvious logic.

## Developer Workflows

### Build & Run
```powershell
# Restore dependencies
dotnet restore WebSpark.ArtSpark.sln

# Build solution (or use VS Code tasks: build-solution, build-demo)
dotnet build WebSpark.ArtSpark.sln

# Run Demo locally
dotnet run --project WebSpark.ArtSpark.Demo

# Watch mode for hot reload
dotnet watch run --project WebSpark.ArtSpark.Demo
```

### Testing
```powershell
# Run all tests
dotnet test WebSpark.ArtSpark.Tests

# Run specific test project or class
dotnet test WebSpark.ArtSpark.Tests --filter "FullyQualifiedName~ArtistSearchTest"
```

### Quality Audit
```powershell
# Run full audit (build diagnostics + dependency currency + safeguards)
pwsh -File scripts/audit/run-quality-audit.ps1

# Generate filtered backlog for high-priority items
pwsh -File scripts/audit/run-quality-audit.ps1 -Severity Warning -MaxItems 10
```
Output: `docs/copilot/YYYY-MM-DD/quality-audit.md` with prioritized backlog. See `specs/001-quality-audit/quickstart.md`.

### Database Migrations
```powershell
# Add new migration
dotnet ef migrations add MigrationName --project WebSpark.ArtSpark.Demo

# Update database
dotnet ef database update --project WebSpark.ArtSpark.Demo

# Generate SQL script
dotnet ef migrations script --project WebSpark.ArtSpark.Demo
```
Migrations live in `WebSpark.ArtSpark.Demo/Migrations/`. Use descriptive names and test both up/down paths.

### Serilog Logs
- Log path configured via `WebSpark:LogFilePath` in `appsettings.json` (default: `c:\temp\WebSpark\Logs\artspark-.txt`).
- Daily rolling with 30-day retention. EF Core queries filtered to Warning+.
- See `WebSpark.ArtSpark.Demo/Utilities/LoggingUtility.cs` and `SERILOG-IMPLEMENTATION.md`.

### AI Configuration
```bash
# Set OpenAI API key (development)
cd WebSpark.ArtSpark.Demo
dotnet user-secrets set "ArtSparkAgent:OpenAI:ApiKey" "sk-..."
```
Config structure in `appsettings.json`:
```json
{
  "ArtSparkAgent": {
    "OpenAI": { "ModelId": "gpt-4o", "Temperature": 0.7 },
    "Cache": { "Enabled": true }
  }
}
```

## Testing & Quality Gates
- Run `dotnet test` before merge. Extend existing test suites instead of duplicating coverage.
- For EF changes, create migrations and verify seed/config updates in `ArtSparkDbContext`.
- Validate logging: ensure new code emits structured Serilog events with correlation IDs.
- Mock external APIs (Art Institute, OpenAI) in tests for determinism.

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