# Prompt Authoring Guide for AI Personas

**Audience**: Content editors, AI prompt engineers, and developers working with ArtSpark AI chat personas.

**Last Updated**: November 17, 2025

---

## Overview

ArtSpark AI personas use markdown-based prompt files with YAML front matter for configuration. This guide explains how to author, validate, and deploy persona prompts without code changes.

---

## File Structure

### Location
All prompt files live in `WebSpark.ArtSpark.Demo/prompts/agents/`:

```
prompts/agents/
├── artspark.artwork.prompt.md     # Artwork persona
├── artspark.artist.prompt.md      # Artist persona
├── artspark.curator.prompt.md     # Curator persona
└── artspark.historian.prompt.md   # Historian persona
```

### Naming Convention
Format: `artspark.{persona-type}.prompt.md`

| Persona | File Name | Purpose |
|---------|-----------|---------|
| Artwork | `artspark.artwork.prompt.md` | Artwork-focused conversations with specific piece details |
| Artist | `artspark.artist.prompt.md` | Artist biography and style discussions |
| Curator | `artspark.curator.prompt.md` | Museum context and exhibition insights |
| Historian | `artspark.historian.prompt.md` | Historical period and cultural context |

---

## Prompt File Format

### Basic Structure

```markdown
---
model: gpt-4o
temperature: 0.7
top_p: 0.9
max_output_tokens: 1000
---

## CULTURAL SENSITIVITY

[Instructions for handling cultural context, religious imagery, etc.]

## CONVERSATION GUIDELINES

[Instructions for tone, response length, citation style, etc.]

## RESPONSE FORMAT

[Optional: Specific formatting instructions]
```

### YAML Front Matter Reference

| Key | Type | Range | Default | Description |
|-----|------|-------|---------|-------------|
| `model` (or `modelid`) | string | - | `gpt-4o` | OpenAI model identifier |
| `temperature` | number | 0.0-1.0 | 0.7 | Sampling randomness (lower = more focused) |
| `top_p` (or `topp`) | number | 0.0-1.0 | 0.9 | Nucleus sampling threshold |
| `max_output_tokens` (or `maxoutputtokens`) | integer | 1-4096 | 1000 | Maximum response length |
| `frequency_penalty` (or `frequencypenalty`) | number | -2.0-2.0 | 0.0 | Penalize token repetition |
| `presence_penalty` (or `presencepenalty`) | number | -2.0-2.0 | 0.0 | Encourage topic diversity |

**Notes**:
- All keys are **case-insensitive**
- Snake_case and camelCase aliases supported
- Missing keys inherit from `appsettings.json` defaults
- Invalid values are ignored (fallback to defaults)

### Required Sections

Every prompt **must** include these sections:

1. **`## CULTURAL SENSITIVITY`** - Guidelines for handling diverse cultural contexts, religious themes, and sensitive historical content
2. **`## CONVERSATION GUIDELINES`** - Instructions for tone, style, citation format, and response structure

**Validation**: Missing sections will trigger fallback to hardcoded default prompts and log a warning.

---

## Token Replacement

### What are Tokens?

Tokens are placeholders like `{artwork.Title}` that get replaced with actual artwork data at runtime. They enable personalized AI responses.

### Token Syntax

Format: `{artwork.PropertyName}`

**Example**:
```markdown
You are discussing **{artwork.Title}** by {artwork.ArtistDisplay}, created in {artwork.DateDisplay}.
```

Runtime replacement:
```markdown
You are discussing **Starry Night** by Vincent van Gogh, created in 1889.
```

### Available Tokens by Persona

#### Artwork Persona (10 tokens)
| Token | Example Value | Description |
|-------|--------------|-------------|
| `{artwork.Title}` | "Starry Night" | Artwork title |
| `{artwork.ArtistDisplay}` | "Vincent van Gogh\nDutch, 1853-1890" | Artist name with dates |
| `{artwork.DateDisplay}` | "1889" | Creation date |
| `{artwork.PlaceOfOrigin}` | "France" | Geographic origin |
| `{artwork.Medium}` | "Oil on canvas" | Materials/technique |
| `{artwork.Dimensions}` | "73.7 × 92.1 cm" | Physical size |
| `{artwork.Description}` | "[Long description]" | Museum description text |
| `{artwork.CulturalContext}` | "Post-Impressionism" | Cultural/historical context |
| `{artwork.StyleTitle}` | "Post-Impressionist" | Artistic style |
| `{artwork.Classification}` | "Painting" | Object classification |

#### Artist Persona (6 tokens)
| Token | Example Value |
|-------|--------------|
| `{artwork.Title}` | "Starry Night" |
| `{artwork.ArtistDisplay}` | "Vincent van Gogh\nDutch, 1853-1890" |
| `{artwork.DateDisplay}` | "1889" |
| `{artwork.PlaceOfOrigin}` | "France" |
| `{artwork.StyleTitle}` | "Post-Impressionist" |
| `{artwork.Classification}` | "Painting" |

#### Curator Persona (6 tokens)
Same as Artist persona: Title, ArtistDisplay, DateDisplay, PlaceOfOrigin, StyleTitle, Classification

#### Historian Persona (7 tokens)
| Token | Example Value |
|-------|--------------|
| `{artwork.Title}` | "Starry Night" |
| `{artwork.DateDisplay}` | "1889" |
| `{artwork.PlaceOfOrigin}` | "France" |
| `{artwork.CulturalContext}` | "Post-Impressionism" |
| `{artwork.StyleTitle}` | "Post-Impressionist" |
| `{artwork.Classification}` | "Painting" |
| `{artwork.Description}` | "[Long description]" |

### Token Validation (Security)

**Whitelist Enforcement**: Only the tokens listed above are allowed for each persona. Using unauthorized tokens will:
1. Trigger validation error: `PromptTokenValidationFailed`
2. Fall back to hardcoded default prompt
3. Log warning with invalid token name

**Example of BLOCKED token**:
```markdown
{artwork.ApiLink}  ❌ Not in whitelist, will cause fallback
```

**Why?** Prevents injection attacks where malicious tokens could expose internal data or break responses.

---

## Authoring Best Practices

### 1. Start with Existing Prompts
Copy an existing prompt file and modify rather than starting from scratch. This ensures required sections are present.

### 2. Test Locally First
**Development workflow**:
1. Enable hot reload in `appsettings.Development.json`:
   ```json
   "EnableHotReload": true
   ```
2. Run Demo in watch mode: `dotnet watch run --project WebSpark.ArtSpark.Demo`
3. Edit prompt file and save
4. Wait 500ms for reload detection
5. Test AI chat to verify changes

### 3. Use Appropriate Temperature

| Temperature | Use Case | Example |
|-------------|----------|---------|
| 0.0-0.3 | Factual, deterministic | Historical dates, artwork dimensions |
| 0.4-0.7 | Balanced creativity | General conversation (default) |
| 0.8-1.0 | Highly creative | Poetry, creative interpretation |

**Recommendation**: Start with 0.7 and adjust based on response quality.

### 4. Token Placement Strategy

**Do**:
- Place tokens early in prompt for context
- Use tokens in factual statements
- Combine multiple tokens for rich context

**Don't**:
- Place tokens in questions to the AI
- Use tokens in YAML front matter
- Nest tokens or use conditional logic

**Good Example**:
```markdown
## CULTURAL SENSITIVITY

When discussing **{artwork.Title}** by {artwork.ArtistDisplay}, 
be mindful of its {artwork.CulturalContext} origins.
```

**Bad Example**:
```markdown
## CULTURAL SENSITIVITY

If {artwork.Classification} is "Painting", then discuss technique...
❌ Conditional logic not supported
```

### 5. Section Organization

**Recommended order**:
1. CULTURAL SENSITIVITY (required)
2. CONVERSATION GUIDELINES (required)
3. RESPONSE FORMAT (optional)
4. CITATION STYLE (optional)
5. EXAMPLE EXCHANGES (optional)

### 6. Response Length Guidance

| max_output_tokens | Approximate Words | Use Case |
|-------------------|-------------------|----------|
| 500 | ~375 words | Brief answers |
| 1000 | ~750 words | Standard responses (default) |
| 1500 | ~1125 words | Detailed analysis |
| 2000 | ~1500 words | Comprehensive essays |

**Note**: Token count includes prompt + response, so actual response may be shorter.

---

## Validation Checklist

Before deploying a prompt, verify:

- [ ] **File naming**: Follows `artspark.{persona}.prompt.md` convention
- [ ] **YAML syntax**: Valid YAML between `---` delimiters (no tabs, consistent indentation)
- [ ] **Required sections**: `## CULTURAL SENSITIVITY` and `## CONVERSATION GUIDELINES` present
- [ ] **Token whitelist**: Only authorized tokens used for that persona
- [ ] **Temperature range**: 0.0-1.0 (decimals required, e.g., `0.7` not `0,7`)
- [ ] **Model availability**: Model ID exists in OpenAI API (e.g., `gpt-4o`, `gpt-4-turbo`)
- [ ] **Character encoding**: UTF-8 without BOM
- [ ] **Line endings**: LF (`\n`) preferred, CRLF (`\r\n`) acceptable

---

## Common Errors & Solutions

### Error: "PromptLoadFailed"
**Symptom**: AI chat uses generic responses, log shows file load error

**Causes**:
- File not found (check `DataPath` in appsettings.json)
- File permissions (ensure read access)
- Invalid UTF-8 encoding

**Solution**:
1. Verify file exists: `ls prompts/agents/artspark.artwork.prompt.md`
2. Check logs for specific error: `grep PromptLoadFailed logs/artspark-*.txt`
3. Validate file encoding: Should be UTF-8

### Error: "PromptTokenValidationFailed"
**Symptom**: Prompt falls back to default, warning in logs

**Causes**:
- Using token not in persona's whitelist
- Typo in token name (e.g., `{artwork.Ttile}`)
- Wrong case (tokens are case-sensitive)

**Solution**:
1. Check logs for invalid token name
2. Compare against whitelist tables above
3. Remove or replace unauthorized token

### Error: "PromptFallbackUsed"
**Symptom**: AI responses don't reflect prompt changes

**Causes**:
- Validation failure (missing sections, invalid tokens)
- Empty prompt file
- Syntax error in YAML front matter

**Solution**:
1. Run validation checklist above
2. Check for empty file: `cat prompts/agents/artspark.artwork.prompt.md`
3. Validate YAML online: https://www.yamllint.com/

### Warning: "Prompt data path does not exist"
**Symptom**: Startup log shows path warning, all personas use fallback

**Causes**:
- `DataPath` misconfigured in appsettings.json
- Directory not created
- Wrong relative path

**Solution**:
1. Check `DataPath` value in appsettings.json
2. Create directory if missing: `mkdir -p prompts/agents`
3. Use absolute path for testing: `"DataPath": "C:/path/to/prompts/agents"`

---

## Deployment Workflow

### Local Development
1. Edit prompt file
2. Save (hot reload triggers automatically in ~500ms)
3. Test in browser
4. Iterate until satisfied

### Staging/Production
1. Disable hot reload: `"EnableHotReload": false` in appsettings.json
2. Deploy prompt files with application
3. Restart application to load new prompts
4. Monitor logs for `PromptLoaded` events
5. Verify AI responses in UI

### Rollback Plan
1. **Quick**: Delete problematic prompt file → Forces fallback to hardcoded default
2. **Safer**: Replace with previous version of prompt file
3. **Emergency**: Set `"FallbackToDefault": true` in config (already default)

---

## Advanced Topics

### Persona Variants (Future)
The `VariantsPath` configuration supports A/B testing prompt versions:

```json
"Prompts": {
  "DataPath": "./prompts/agents",
  "VariantsPath": "./prompts/agents/variants"
}
```

Place alternate prompts in variants folder with same naming convention. (Implementation pending T027 completion)

### Metadata Inheritance
Metadata resolution order (highest priority first):
1. YAML front matter in prompt file
2. `DefaultMetadata` in appsettings.json
3. Hardcoded defaults in PromptOptions.cs

**Example**:
```
appsettings.json:    temperature: 0.7
prompt file:         temperature: 0.9  ← Winner
Runtime:             temperature: 0.9
```

### Hot Reload Internals
- Uses `PhysicalFileProvider` with `IChangeToken`
- Cache invalidated on file write
- Next AI request loads fresh content
- ~500ms detection latency (file system dependent)
- Only enabled when `EnableHotReload: true`

---

## Getting Help

**Logs**: Check Serilog output in `c:\temp\WebSpark\Logs\artspark-*.txt`

**Events to monitor**:
- `PromptLoaded`: Successful load with hash
- `PromptLoadFailed`: Error details
- `PromptFallbackUsed`: When defaults activate
- `ConfigurationReloaded`: Hot reload detection

**Technical Questions**: Review `specs/003-prompt-management/` documentation

**Content Questions**: Contact AI content team

---

## Example Prompts

### Minimal Valid Prompt
```markdown
---
model: gpt-4o
temperature: 0.7
---

## CULTURAL SENSITIVITY

Treat all artworks and cultures with respect.

## CONVERSATION GUIDELINES

Provide concise, accurate information about the artwork.
```

### Production-Ready Prompt (Artwork Persona)
```markdown
---
model: gpt-4o
temperature: 0.7
top_p: 0.9
max_output_tokens: 1000
---

## CULTURAL SENSITIVITY

You are discussing **{artwork.Title}** by {artwork.ArtistDisplay}, 
created in {artwork.DateDisplay}. This work originates from 
{artwork.PlaceOfOrigin} and represents {artwork.CulturalContext}.

When discussing artworks:
- Acknowledge diverse cultural perspectives
- Respect religious and spiritual significance
- Avoid imposing Western art historical frameworks universally
- Recognize colonial histories in museum collections
- Use inclusive, accessible language

## CONVERSATION GUIDELINES

**Your Role**: You are an AI assistant specializing in this specific 
artwork. Provide insights based on:
- Medium and technique: {artwork.Medium}
- Physical characteristics: {artwork.Dimensions}
- Artistic style: {artwork.StyleTitle}
- Classification: {artwork.Classification}

**Tone**: Engaging yet scholarly, accessible to general audiences

**Response Format**:
1. Open with artwork-specific insight
2. Connect to broader artistic/historical context
3. Invite deeper engagement with specific questions

**Citations**: Reference the museum's description when appropriate:
{artwork.Description}

**Response Length**: 2-3 paragraphs (aim for 150-250 words)

**Avoid**:
- Generic art history lectures
- Overly academic jargon
- Speculation beyond artwork evidence
- Personal opinions without context
```

---

**Version**: 1.0  
**Specification**: `specs/003-prompt-management/spec.md`  
**Last Reviewed**: November 17, 2025
