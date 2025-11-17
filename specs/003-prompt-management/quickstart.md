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
3. Run migrations if necessary:
   ```powershell
   dotnet ef database update --project WebSpark.ArtSpark.Demo
   ```
4. Launch the Demo with hot reload enabled for prompts:
   ```powershell
   dotnet watch run --project WebSpark.ArtSpark.Demo --no-launch-profile
   ```

## Editing Prompts
- Update the markdown file (e.g., `prompts/agents/artspark.artwork.prompt.md`).
- Preserve sections: `## CULTURAL SENSITIVITY`, `## CONVERSATION GUIDELINES`, and any token placeholders from the whitelist.
- On save, hot reload (if enabled) reloads the prompt for the next chat request; otherwise restart the Demo app.

## Verification Steps
1. Use the Demo UI `/Artwork/Details/{id}` to initiate a chat and confirm persona tone matches changes.
2. Inspect logs for `PromptLoaded` entries showing file hash and size.
3. Temporarily rename a prompt file to confirm fallback behavior logs `PromptFallbackUsed` and responses continue using default persona text.
4. Run automated tests:
   ```powershell
   dotnet test WebSpark.ArtSpark.Tests --filter "Prompt"
   ```
