# Quickstart – AI Persona Prompt Management System

## Prerequisites
- Configure `ArtSparkAgent:Prompts` section in `WebSpark.ArtSpark.Demo/appsettings.Development.json` with `DataPath: "./prompts/agents"`.
- Ensure prompt markdown files exist: `prompts/agents/artspark.{persona}.prompt.md` with required sections and allowed tokens.
- Set OpenAI API key via `dotnet user-secrets` if not already configured.

## Local Setup
1. Create the prompt directory:
   ```powershell
   mkdir -p WebSpark.ArtSpark.Demo/prompts/agents
   ```
2. Copy initial prompt templates from `/WebSpark.ArtSpark.Agent/Personas/Templates` (to be added) into the Demo directory.
3. Confirm `ArtSparkAgent:Prompts:DefaultMetadata` in `appsettings.Development.json` includes values such as `ModelId`, `Temperature`, and optional knobs (`TopP`, `MaxOutputTokens`).
4. Run migrations if necessary:
   ```powershell
   dotnet ef database update --project WebSpark.ArtSpark.Demo
   ```
5. Launch the Demo with hot reload enabled for prompts:
   ```powershell
   dotnet watch run --project WebSpark.ArtSpark.Demo --no-launch-profile
   ```

## Editing Prompts
- Begin each persona file with YAML front matter to override metadata when needed:
   ```markdown
   ---
   model: gpt-4o
   temperature: 0.6
   top_p: 0.9
   max_output_tokens: 700
   ---
   ```
- Update the markdown body (e.g., `prompts/agents/artspark.artwork.prompt.md`).
- Preserve sections: `## CULTURAL SENSITIVITY`, `## CONVERSATION GUIDELINES`, and any token placeholders from the whitelist.
- On save, hot reload (if enabled) reloads both content and metadata for the next chat request and emits a `ConfigurationReloaded` log; otherwise restart the Demo app.
- If validation fails (missing headings, unapproved tokens, malformed metadata), the system falls back to the hardcoded prompt and logs `PromptLoadFailed` or `PromptTokenValidationFailed`.

## Verification Steps
1. Use the Demo UI `/Artwork/Details/{id}` to initiate a chat and confirm persona tone matches changes.
2. Inspect logs for `PromptLoaded` entries showing file hash, size, and metadata overrides; metadata reloads should log `ConfigurationReloaded`.
3. Temporarily rename a prompt file to confirm fallback behavior logs `PromptFallbackUsed` and responses continue using default persona text.
4. Introduce an invalid token (e.g., `{artwork.SecretField}`) to verify the system logs `PromptTokenValidationFailed` and falls back to the default prompt.
5. Run automated tests:
   ```powershell
   dotnet test WebSpark.ArtSpark.Tests --filter "Prompt"
   ```
