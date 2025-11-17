# Data Model – AI Persona Prompt Management System

## Entities

### `PromptTemplate`
- **Fields**:
  - `PersonaType` (enum `PersonaType` – Artwork, Artist, Curator, Historian)
  - `FilePath` (string, absolute path resolved from `ArtSparkAgent:Prompts:DataPath`)
  - `FileName` (string, e.g., `artspark.artwork.prompt.md`)
  - `Content` (string, raw markdown)
  - `ContentHash` (string, MD5 hex)
  - `LastModifiedUtc` (DateTime)
  - `FileSizeBytes` (long)
  - `IsFallback` (bool)
  - `MetadataOverrides` (`PromptMetadata`, optional – parsed from YAML front matter)
- **Validation**:
  - Content MUST include headings `## CULTURAL SENSITIVITY` and `## CONVERSATION GUIDELINES`.
  - File size MUST be >0 and < 100 KB.
  - When `IsFallback == false`, file MUST exist on disk, pass token whitelist validation, and contain well-formed metadata overrides; failures trigger fallback to hardcoded prompt.

### `PromptMetadata`
- **Fields**:
  - `ModelId` (string)
  - `Temperature` (double)
  - `TopP` (double, optional)
  - `MaxOutputTokens` (int?, optional)
  - `FrequencyPenalty` (double?, optional)
  - `PresencePenalty` (double?, optional)
- **Validation**:
  - Values MUST fall within OpenAI configuration bounds (e.g., `Temperature` ∈ [0,1]).
  - Missing fields fall back to persona defaults defined in configuration.

### `PersonaPromptConfiguration`
- **Fields**:
  - `PersonaType` (enum)
  - `PromptFileName` (string, default `artspark.{persona}.prompt.md`)
  - `FallbackPrompt` (string – existing hardcoded prompt)
  - `AllowedTokens` (string collection – e.g., `artwork.Title`)
  - `EnableHotReload` (bool)
  - `DefaultMetadata` (`PromptMetadata` – baseline values applied when overrides missing)
- **Relationships**:
  - One-to-one with `PromptTemplate` at runtime (loaded prompt bound to configuration entry).

### `TokenReplacementContext`
- **Fields**:
  - `Artwork` (`ArtworkDetails` DTO from Client library)
  - `RequestMetadata` (dictionary – conversation context, optional)
- **Validation**:
  - Only tokens explicitly listed in `AllowedTokens` may map to context values.
  - Missing token values resolve to empty string and emit `PromptTokenValidationFailed` log; presence of disallowed tokens causes prompt rejection and fallback.

## Interactions
- `PromptLoader` materializes `PromptTemplate` per persona using `PersonaPromptConfiguration` and the configured data path.
- `PromptLoader` merges `MetadataOverrides` with `DefaultMetadata` and applies strict validation; failures produce `PromptLoadFailed` or `PromptTokenValidationFailed` events and trigger fallback templates.
- `FileBackedPersonaHandler` composes existing persona handlers; injects `PromptTemplate` content and resolved metadata into Semantic Kernel prompt request in place of static `GenerateSystemPrompt` output.
- Hot reload (when enabled) attaches an `IChangeToken` to `PromptTemplate`, refreshes metadata and content, and emits `ConfigurationReloaded` events when overrides change.
