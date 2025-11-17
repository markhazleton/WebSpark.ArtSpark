# Contract – Prompt Loading Services

## Service Registration

```csharp
public static IServiceCollection AddPromptManagement(this IServiceCollection services, IConfiguration configuration);
```

- Registers `IPromptLoader` (singleton) and `IFileBackedPersonaHandlerFactory` (singleton).
- Binds configuration section `ArtSparkAgent:Prompts` to `PromptOptions` with validation on build.
- Adds optional hot-reload change token subscription when `EnableHotReload` is true.

## `IPromptLoader`

```csharp
public interface IPromptLoader
{
    ValueTask<PromptTemplate> GetPromptAsync(PersonaType personaType, CancellationToken cancellationToken = default);
}
```

- Returns cached `PromptTemplate` if already loaded; otherwise reads from `prompts/agents/` using configured `DataPath`.
- Throws `PromptNotFoundException` only when `FallbackToDefault` is false; otherwise returns fallback template with `IsFallback = true`.
- Emits Serilog events `PromptLoaded` (Information) or `PromptLoadFailed` (Warning/Error) with properties: `PersonaType`, `FilePath`, `ContentHash`, `FileSizeBytes`, `ElapsedMs`.

## `IFileBackedPersonaHandler`

```csharp
public interface IFileBackedPersonaHandler : IPersonaHandler
{
    PersonaType PersonaType { get; }
}
```

- Wraps existing persona handlers, delegating chat orchestration while substituting `PromptTemplate.Content` for `GenerateSystemPrompt()` results.
- Falls back to inner handler when prompt retrieval returns `IsFallback = true`.
- Logs `PromptFallbackUsed` when fallback path invoked.

## Configuration Schema (`ArtSparkAgent:Prompts`)

```json
{
  "DataPath": "./prompts/agents",
  "EnableHotReload": true,
  "FallbackToDefault": true,
  "Personas": {
    "Artwork": {
      "FileName": "artspark.artwork.prompt.md",
      "AllowedTokens": ["artwork.Title", "artwork.ArtistDisplay", "artwork.DateDisplay"],
      "EnableHotReload": true
    }
  }
}
```

- `Personas` entry optional; defaults derived from `PersonaPromptConfiguration` if omitted.
- Invalid tokens trigger configuration validation failure with descriptive message.
