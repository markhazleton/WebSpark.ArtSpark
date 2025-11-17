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
- **Validation**:
  - Content MUST include headings `## CULTURAL SENSITIVITY` and `## CONVERSATION GUIDELINES`.
  - File size MUST be >0 and < 100 KB.
  - When `IsFallback == false`, file MUST exist on disk and pass token whitelist validation.

### `PersonaPromptConfiguration`
- **Fields**:
  - `PersonaType` (enum)
  - `PromptFileName` (string, default `artspark.{persona}.prompt.md`)
  - `FallbackPrompt` (string – existing hardcoded prompt)
  - `AllowedTokens` (string collection – e.g., `artwork.Title`)
  - `EnableHotReload` (bool)
- **Relationships**:
  - One-to-one with `PromptTemplate` at runtime (loaded prompt bound to configuration entry).

### `TokenReplacementContext`
- **Fields**:
  - `Artwork` (`ArtworkDetails` DTO from Client library)
  - `RequestMetadata` (dictionary – conversation context, optional)
- **Validation**:
  - Only tokens explicitly listed in `AllowedTokens` may map to context values.
  - Missing token values resolve to empty string and emit `PromptTokenValidationFailed` log.

## Interactions
- `PromptLoader` materializes `PromptTemplate` per persona using `PersonaPromptConfiguration` and the configured data path.
- `FileBackedPersonaHandler` composes existing persona handlers; injects `PromptTemplate` content into Semantic Kernel prompt request in place of static `GenerateSystemPrompt` output.
- Hot reload (when enabled) attaches an `IChangeToken` to `PromptTemplate` and invalidates cached content on disk updates.
