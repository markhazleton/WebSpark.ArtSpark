# Research Log – AI Persona Prompt Management System

## Findings

### Decision: Use `PhysicalFileProvider` + `IChangeToken` for prompt hot reload
- **Rationale**: Aligns with ASP.NET Core best practices for monitoring file changes, integrates cleanly with dependency injection, and avoids manual thread management. Provides debounced change notifications and works cross-platform.
- **Alternatives considered**: Direct `FileSystemWatcher` (more boilerplate, risk of duplicate events); periodic polling (adds latency, wastes IO when no edits occur).

### Decision: Keep prompt parsing in-process with regex-based token replacement
- **Rationale**: Persona templates only require `{entity.Property}` substitution; a scoped regex with persona-specific whitelists minimizes dependencies and supports validation logging.
- **Alternatives considered**: Introduce templating engine (Scriban/Razor) – heavier dependency surface; dynamic C# interpolation – security risk; Semantic Kernel prompt templates – unnecessary coupling.

### Decision: Hash prompt content with `MD5.HashData` for logging
- **Rationale**: MD5 is fast for non-cryptographic fingerprints, enabling operators to confirm prompt versions without performance penalty; matches existing Serilog operational patterns.
- **Alternatives considered**: SHA-256 (stronger but slower with no auditing benefit); omit hashing (would hinder production version traceability).

### Decision: Store production prompts on web server with role-based access
- **Rationale**: Satisfies Live Demo First agility by enabling content updates without redeployments; mirrors content author workflow and keeps files close to Demo app for low-latency reads.
- **Alternatives considered**: Bundle prompts in deployment artifact (slows iteration); external blob storage (adds latency, new secrets management, more moving parts).

### Decision: Extend Agent DI with decorator pattern for persona handlers
- **Rationale**: Preserves existing `IPersonaHandler` contracts while substituting file-backed prompts; aligns with existing HttpClient decorator patterns and keeps fallbacks intact.
- **Alternatives considered**: Replace persona classes entirely (risk of regressions); add file reads inside each persona class (duplication, harder to test).

### Decision: Hybrid prompt metadata configuration
- **Rationale**: Stores global defaults (e.g., `model`, `temperature`) in configuration while allowing persona prompt files to override via YAML front matter, balancing operational governance with per-persona tuning.
- **Alternatives considered**: Configuration-only metadata (reduced flexibility); file-only metadata (harder to enforce environment-wide defaults).

### Decision: Strict failure on invalid tokens or metadata
- **Rationale**: Rejects prompts containing unapproved tokens or malformed metadata, falls back to hardcoded defaults, and logs the failure to keep AI safeguards intact.
- **Alternatives considered**: Lenient stripping of tokens (silent degradation risk); ignoring invalid sections (operators may miss issues).

### Decision: Hot reload emits configuration audit events
- **Rationale**: When hot reload updates prompt metadata, the system refreshes both content and front matter and emits a `ConfigurationReloaded` event so operators can audit runtime changes.
- **Alternatives considered**: Body-only reload (developers cannot iterate on metadata); reload without logging (no audit trail for dynamic updates).
