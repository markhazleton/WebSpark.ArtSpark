# WebSpark.ArtSpark.Agent

🎭 **Revolutionary AI Chat System for Art & Culture**

A sophisticated AI-powered conversational agent library that brings artworks to life through dynamic persona-based conversations. Built with Microsoft Semantic Kernel and OpenAI integration, this groundbreaking library enables museums, galleries, and cultural institutions to create immersive educational experiences where visitors can literally talk with artworks.

## 🌟 What Makes This Special

**Experience Art Like Never Before**: Instead of reading static descriptions, visitors can now have actual conversations with artworks, learning through natural dialogue as if speaking with the artwork itself, its creator, a knowledgeable curator, or a historian.

**Four Unique Perspectives**: Each artwork can be explored through four distinct AI personas, each offering unique insights and knowledge:

- 🖼️ **The Artwork Itself** - First-person stories from the art's perspective
- 🎨 **The Artist** - Creative insights from the creator's viewpoint  
- 🏛️ **Museum Curator** - Professional scholarly analysis
- 📚 **Art Historian** - Deep historical and cultural context

**AI Vision Integration**: Advanced image analysis allows the AI to actually "see" and describe artworks, discussing specific visual elements, colors, patterns, and artistic techniques in real-time.

## 🎨 Overview

WebSpark.ArtSpark.Agent transforms static artwork data into dynamic, conversational experiences by allowing users to chat with artworks as if they were living entities. The library supports multiple AI personas, visual analysis capabilities, and contextual conversations that adapt to different educational and cultural perspectives.

### Key Features

- **🎭 Multiple AI Personas**: Chat with artworks from different perspectives (Artwork, Artist, Curator, Historian)
- **👁️ Visual Analysis**: AI-powered image analysis for detailed artwork descriptions
- **🧠 Conversation Memory**: Persistent chat history and context awareness
- **🎯 Cultural Sensitivity**: Respectful handling of cultural artifacts and contexts
- **⚡ High Performance**: Optimized for real-time conversational experiences
- **🔧 Configurable**: Flexible settings for different use cases and environments
- **📝 Externalized Prompts**: Edit AI persona prompts without code changes (new in v1.1)
- **🔥 Hot Reload**: Live prompt updates in development without restarts (new in v1.1)

## 🏗️ Architecture

The library is built on a modular architecture with the following key components:

### Core Interfaces

- `IArtworkChatAgent`: Main conversation interface
- `IArtworkDataProvider`: Data source abstraction
- `IChatMemory`: Conversation persistence
- `IPersonaFactory`: Persona creation and management
- `IPersonaHandler`: Individual persona implementations
- `IPromptLoader`: Prompt file loading and hot reload (new in v1.1)

### AI Personas

#### 🖼️ Artwork Persona

Speak directly with the artwork itself! The artwork takes on consciousness and shares its story in first person:

- Personal experiences from creation to museum display
- Cultural significance and sacred purposes
- Emotional responses to being viewed and interpreted
- Visual self-description using AI vision capabilities

#### 🎨 Artist Persona

Chat with the artist who created the work:

- Creative process and inspiration
- Technical methods and materials
- Historical context of creation
- Personal stories and motivations

#### 🏛️ Curator Persona

Museum curator perspective offering:

- Art historical analysis
- Comparative studies with other works
- Exhibition context and significance
- Academic interpretation

#### 📚 Historian Persona

Historical expert providing:

- Cultural and historical context
- Period-specific knowledge
- Social and political background
- Cross-cultural connections

## 🚀 Quick Start

### Installation

Add the package reference to your project:

```xml
<PackageReference Include="WebSpark.ArtSpark.Agent" Version="1.0.0" />
```

### Basic Setup

1. **Configure Services** in your `Program.cs` or `Startup.cs`:

```csharp
using WebSpark.ArtSpark.Agent.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add ArtSpark Agent services
builder.Services.AddArtSparkAgent(builder.Configuration, config =>
{
    config.OpenAI.ModelId = "gpt-4o";
    config.OpenAI.Temperature = 0.7;
    config.Cache.Enabled = true;
});

var app = builder.Build();
```

2. **Configure Settings** in your `appsettings.json`:

```json
{
  "ArtSparkAgent": {
    "OpenAI": {
      "ApiKey": "your-openai-api-key",
      "ModelId": "gpt-4o",
      "VisionModelId": "gpt-4o",
      "MaxTokens": 1000,
      "Temperature": 0.7,
      "TopP": 0.9,
      "RequestTimeout": "00:02:00",
      "MaxRetries": 3
    },
    "Prompts": {
      "DataPath": "./prompts/agents",
      "EnableHotReload": false,
      "FallbackToDefault": true,
      "DefaultMetadata": {
        "ModelId": "gpt-4o",
        "Temperature": 0.7,
        "TopP": 0.9,
        "MaxOutputTokens": 1000
      }
    },
    "ArtInstitute": {
      "BaseUrl": "https://api.artic.edu/api/v1",
      "RequestTimeout": "00:01:00",
      "MaxRetries": 3
    },
    "DefaultChatSettings": {
      "MaxTokens": 1000,
      "Temperature": 0.7,
      "TopP": 0.9,
      "MaxConversationLength": 20
    },
    "Cache": {
      "Enabled": true,
      "ConversationExpiry": "01:00:00"
    },
    "Logging": {
      "EnableTelemetry": true,
      "LogLevel": "Information"
    }
  }
}
```

### Basic Usage Example

```csharp
using WebSpark.ArtSpark.Agent.Interfaces;
using WebSpark.ArtSpark.Agent.Models;
using WebSpark.ArtSpark.Agent.Personas;

public class ChatController : ControllerBase
{
    private readonly IArtworkChatAgent _chatAgent;

    public ChatController(IArtworkChatAgent chatAgent)
    {
        _chatAgent = chatAgent;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        var response = await _chatAgent.ChatAsync(request);
          if (response.Success)
        {
            return Ok(new
            {
                message = response.Response,
                suggestedQuestions = response.SuggestedQuestions,
                analytics = response.Analytics
            });
        }

        return BadRequest(response.Error);
    }
}
```

### Frontend Integration Example

```javascript
async function chatWithArtwork(artworkId, message, persona = 'Artwork') {
    const response = await fetch('/api/chat', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            artworkId: artworkId,
            message: message,
            persona: persona,
            includeVisualAnalysis: true
        })
    });

    const data = await response.json();
    return data;
}

// Example usage
const response = await chatWithArtwork(
    111628, 
    "Tell me about your cultural significance",
    "Artwork"
);

console.log(response.message);
console.log("Suggested questions:", response.suggestedQuestions);
```

## � Prompt Management (New in v1.1)

### Overview

Content editors can now modify AI persona prompts without touching code! Prompts are stored as markdown files with YAML front matter for configuration.

### Prompt File Location

Create prompt files in your configured `DataPath` (default: `./prompts/agents/`):

```
prompts/agents/
├── artspark.artwork.prompt.md
├── artspark.artist.prompt.md
├── artspark.curator.prompt.md
└── artspark.historian.prompt.md
```

### Prompt File Format

```markdown
---
model: gpt-4o
temperature: 0.7
top_p: 0.9
max_output_tokens: 1000
---

## CULTURAL SENSITIVITY

Instructions for handling cultural context and sensitive topics...

## CONVERSATION GUIDELINES

Instructions for tone, style, and response format...
```

### Configuration Options

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `DataPath` | string | `./prompts/agents` | Directory containing prompt files |
| `EnableHotReload` | bool | `false` | Reload prompts on file changes (dev only) |
| `FallbackToDefault` | bool | `true` | Use hardcoded prompts if file fails |
| `DefaultMetadata` | object | See above | Default OpenAI parameters |

### Hot Reload in Development

Enable live prompt editing without restarts:

```json
{
  "ArtSparkAgent": {
    "Prompts": {
      "DataPath": "./prompts/agents",
      "EnableHotReload": true,
      "FallbackToDefault": true
    }
  }
}
```

**How it works**:
1. Edit prompt file and save
2. File watcher detects change (~500ms)
3. Cache invalidated automatically
4. Next AI request uses updated prompt

### Token Replacement

Use tokens like `{artwork.Title}` to personalize prompts with artwork data:

```markdown
You are discussing **{artwork.Title}** by {artwork.ArtistDisplay}.
```

**Available tokens per persona**:
- **Artwork**: 10 tokens (Title, ArtistDisplay, DateDisplay, PlaceOfOrigin, Medium, Dimensions, Description, CulturalContext, StyleTitle, Classification)
- **Artist**: 6 tokens (Title, ArtistDisplay, DateDisplay, PlaceOfOrigin, StyleTitle, Classification)
- **Curator**: 6 tokens (same as Artist)
- **Historian**: 7 tokens (Title, DateDisplay, PlaceOfOrigin, CulturalContext, StyleTitle, Classification, Description)

**Security**: Token validation prevents injection attacks. Only whitelisted tokens are allowed.

### Monitoring

Structured Serilog events for operational insight:
- `PromptLoaded`: Successful load with file hash and size
- `PromptLoadFailed`: Error details for troubleshooting
- `PromptFallbackUsed`: When hardcoded defaults activate
- `PromptTokenValidationFailed`: Invalid token detection
- `ConfigurationReloaded`: Hot reload cache invalidation

### Prompt Authoring Guide

For detailed prompt authoring instructions, see:  
📖 **[Prompt Authoring Guide](../../docs/copilot/prompt-authoring-guide.md)**

Topics covered:
- File structure and naming conventions
- YAML front matter reference
- Token replacement syntax
- Validation checklist
- Common errors and solutions
- Deployment workflow

## �🔧 Advanced Configuration

### Custom Data Provider

Implement your own data source by creating a custom `IArtworkDataProvider`:

```csharp
public class CustomArtworkDataProvider : IArtworkDataProvider
{
    public async Task<ArtworkData?> GetArtworkAsync(int id, CancellationToken cancellationToken)
    {
        // Your custom data retrieval logic
        return new ArtworkData
        {
            Id = id,
            Title = "Custom Artwork",
            // ... other properties
        };
    }

    public async Task<ArtworkData> EnrichArtworkDataAsync(ArtworkData artwork, CancellationToken cancellationToken)
    {
        // Add additional context or data enrichment
        return artwork;
    }
}

// Register in DI container
services.AddScoped<IArtworkDataProvider, CustomArtworkDataProvider>();
```

### Multi-turn Conversations

```csharp
public async Task<ChatResponse> ContinueConversation(
    int artworkId, 
    string newMessage, 
    List<ChatMessage> history)
{
    var request = new ChatRequest
    {
        ArtworkId = artworkId,
        Message = newMessage,
        Persona = ChatPersona.Artwork,
        ConversationHistory = history,
        IncludeVisualAnalysis = false // Skip visual analysis for follow-up messages
    };

    return await _chatAgent.ChatAsync(request);
}
```

### Visual Analysis

```csharp
public async Task<string> AnalyzeArtworkVisuals(int artworkId)
{
    var artwork = await _artworkProvider.GetArtworkAsync(artworkId);
    
    var analysis = await _chatAgent.AnalyzeArtworkVisuallyAsync(
        artwork, 
        "Describe the visual elements, colors, and composition in detail"
    );
    
    return analysis;
}
```

## 📊 Models and Data Structures

### ArtworkData

```csharp
public class ArtworkData
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string ArtistDisplay { get; set; }
    public string DateDisplay { get; set; }
    public string PlaceOfOrigin { get; set; }
    public string Medium { get; set; }
    public string Dimensions { get; set; }
    public string Description { get; set; }
    public string CulturalContext { get; set; }
    public string StyleTitle { get; set; }
    public string Classification { get; set; }
    public string ImageUrl { get; set; }
    public string ThumbnailUrl { get; set; }
    public Dictionary<string, object> FullApiData { get; set; }
    public List<string> Tags { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### ChatRequest

```csharp
public class ChatRequest
{
    public int ArtworkId { get; set; }
    public string Message { get; set; }
    public ChatPersona Persona { get; set; } = ChatPersona.Artwork;
    public List<ChatMessage> ConversationHistory { get; set; } = new();
    public bool IncludeVisualAnalysis { get; set; } = true;
    public ChatSettings? Settings { get; set; }
}
```

### ChatResponse

```csharp
public class ChatResponse
{
    public string Response { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Error { get; set; }
    public List<string> SuggestedQuestions { get; set; } = new();
    public ChatAnalytics Analytics { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}
```

## 🎯 Use Cases

### Museum Interactive Displays

Create touchscreen kiosks where visitors can have conversations with artworks, learning about cultural significance, creation stories, and historical context.

### Educational Applications

Integrate into educational platforms for art history courses, allowing students to engage with artworks in an immersive, conversational manner.

### Virtual Museum Tours

Enhance online museum experiences with AI-powered conversations that adapt to visitor interests and knowledge levels.

### Cultural Heritage Preservation

Document and share cultural knowledge through AI personas that can communicate traditional stories and cultural significance.

## 🔒 Security and Privacy

- **API Key Protection**: Store OpenAI API keys securely using Azure Key Vault or similar services
- **Data Privacy**: No conversation data is stored by default unless explicitly configured
- **Rate Limiting**: Built-in retry logic and timeout handling
- **Cultural Sensitivity**: AI prompts are designed to respect cultural contexts and avoid appropriation

## 🚀 Performance Optimization

- **Caching**: Optional conversation caching to improve response times
- **Async Operations**: Fully asynchronous for high-concurrency scenarios
- **Memory Management**: Efficient conversation history management
- **Visual Analysis Optimization**: Smart caching of image analysis results

## 🧪 Testing

Run the included examples to test your configuration:

```csharp
await BasicUsageExample.RunExample();
await AdvancedUsageExample.RunAdvancedExample();
```

## 📈 Monitoring and Analytics

The library includes built-in analytics and telemetry:

```csharp
public class ChatAnalytics
{
    public int TokensUsed { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public string ModelUsed { get; set; }
    public bool UsedVisualAnalysis { get; set; }
    public string PersonaType { get; set; }
}
```

## 🛠️ Dependencies

- **.NET 9.0**: Modern C# language features and performance
- **Microsoft.SemanticKernel**: AI orchestration and prompt management
- **Microsoft.SemanticKernel.Connectors.OpenAI**: OpenAI integration
- **Microsoft.Extensions.Hosting**: Dependency injection and configuration
- **Microsoft.Extensions.Configuration**: Configuration management
- **Microsoft.Extensions.Options**: Options pattern implementation

## 🤝 Contributing

We welcome contributions! Please see our contributing guidelines for:

- Code style and conventions
- Testing requirements
- Documentation standards
- Cultural sensitivity guidelines

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🔗 Related Projects

- **WebSpark.ArtSpark.Client**: Client library for Art Institute of Chicago API
- **WebSpark.ArtSpark.Demo**: Complete demo application showcasing the library

## 📞 Support

For questions, issues, or feature requests:

- Create an issue on GitHub
- Check the documentation and examples
- Review the configuration options

---

*WebSpark.ArtSpark.Agent - Bringing art to life through AI conversation* 🎨✨
