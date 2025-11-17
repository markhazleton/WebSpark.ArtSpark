# WebSpark.ArtSpark

**A comprehensive .NET solution for the Art Institute of Chicago's public API, featuring a complete client library, revolutionary AI chat system with multiple personas, and demo applications showcasing modern .NET development practices.**

[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

🚀 **[Live Demo: https://artspark.markhazleton.com](https://artspark.markhazleton.com)**

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [🎭 AI Chat with Personas](#-ai-chat-with-personas)
- [Quick Start](#quick-start)
- [API Coverage](#api-coverage)
- [Usage Examples](#usage-examples)
- [Project Structure](#project-structure)
- [Models](#models)
- [Client Configuration](#client-configuration)
- [Development](#development)
- [License](#license)

## Overview

This solution provides a complete .NET ecosystem for interacting with the Art Institute of Chicago's public REST API. The solution includes **four main projects** covering all 33 API endpoints across 6 major categories, plus revolutionary AI chat capabilities that bring artworks to life through conversational AI.

### 🏗️ Solution Architecture

- **WebSpark.ArtSpark.Client** - Complete API client library with all 33 endpoints
- **WebSpark.ArtSpark.Agent** - Revolutionary AI chat system with multiple personas  
- **WebSpark.ArtSpark.Demo** - Interactive web application showcasing all features
- **WebSpark.ArtSpark.Console** - Command-line application for developers

## Features

### 🎨 Core Library Features

- ✅ **Complete API Coverage**: All 33 endpoints across 6 categories
- ✅ **Strongly Typed Models**: Comprehensive C# models for all resource types
- ✅ **Async/Await Support**: Modern asynchronous programming patterns
- ✅ **JSON Deserialization**: Using `System.Text.Json` with proper naming policies
- ✅ **Search Capabilities**: Full-text search with Elasticsearch integration
- ✅ **IIIF Support**: Built-in IIIF URL construction for high-quality images
- ✅ **Flexible Querying**: Pagination, field selection, and resource inclusion
- ✅ **Error Handling**: Graceful error handling and HTTP status management
- ✅ **Modern .NET**: Uses .NET 9.0 with minimal external dependencies

### 🎭 Revolutionary AI Features

- 🎭 **AI Chat with Personas**: Revolutionary AI chat system featuring multiple personas (Artwork, Artist, Curator, Historian)
- 👁️ **Visual Analysis**: AI-powered image analysis with artwork descriptions using OpenAI Vision
- 🧠 **Conversation Memory**: Persistent chat history and contextual conversations
- 🎯 **Cultural Sensitivity**: Respectful handling of cultural artifacts and educational contexts
- 🛡️ **Guard Rails**: Input validation and content filtering for appropriate conversations

### 🚀 Demo Application Features

- 👤 **User Authentication**: ASP.NET Core Identity with SQLite database
- 📚 **User Collections**: Personal artwork organization and favorites
- 🎲 **Random Collection Showcase**: Dynamic home page featuring random public collections
- 🎨 **Responsive Design**: Mobile-first Bootstrap 5 implementation  
- 🎭 **Theme Switching**: Dynamic Bootswatch theme selection with 26+ themes
- 📊 **Build Information**: Version tracking and deployment details
- 🔍 **Enhanced Filtering**: Artwork filtering by style, medium, and classification
- 🔄 **Interactive Discovery**: "New Collection" button for instant content refresh
- 🌐 **SEO-Enhanced Routing**: Clean URLs with slug-based collection routing
- 📱 **Mobile-First Navigation**: Consolidated dropdown navigation for optimal UX

## Quick Start

### Try the Live Demo

🌐 **[Experience WebSpark.ArtSpark live at artspark.markhazleton.com](https://artspark.markhazleton.com)**

### Local Installation

1. Clone the repository:

```bash
git clone https://github.com/MarkHazleton/WebSpark.ArtSpark.git
cd WebSpark.ArtSpark
```

1. Build the solution:

```bash
dotnet build
```

1. Run the demo web application:

```bash
dotnet run --project WebSpark.ArtSpark.Demo
```

Or run the console application:

```bash
dotnet run --project WebSpark.ArtSpark.Console
```

### 🎭 AI Chat Setup (Optional)

To enable AI chat features, add your OpenAI API key to the demo application:

1. Set up user secrets:

```bash
cd WebSpark.ArtSpark.Demo
dotnet user-secrets set "ArtSparkAgent:OpenAI:ApiKey" "your-openai-api-key-here"
```

1. Configure settings in `appsettings.json`:

```json
{
  "ArtSparkAgent": {
    "OpenAI": {
      "ModelId": "gpt-4o",
      "VisionModelId": "gpt-4o",
      "Temperature": 0.7
    },
    "Cache": {
      "Enabled": true
    }
  }
}
```

### Basic Usage

```csharp
using WebSpark.ArtSpark.Client.Clients;
using WebSpark.ArtSpark.Client.Models.Common;

// Create HTTP client and API client
var httpClient = new HttpClient();
var client = new ArtInstituteClient(httpClient);

// Get artworks with pagination
var query = new ApiQuery { Limit = 10, Page = 1 };
var response = await client.GetArtworksAsync(query);

// Display results
foreach (var artwork in response.Data ?? [])
{
    Console.WriteLine($"{artwork.Title} (ID: {artwork.Id})");
    if (!string.IsNullOrEmpty(artwork.ImageId))
    {
        var imageUrl = client.BuildIiifUrl(artwork.ImageId, "843,");
        Console.WriteLine($"  Image: {imageUrl}");
    }
}
```

## API Coverage

The client provides complete coverage of the Art Institute of Chicago API:

### Collections (15 endpoints)

- **Artworks** - `/artworks` - The museum's collection of artworks
- **Agents** - `/agents` - Artists, creators, and other people/organizations
- **Places** - `/places` - Geographic locations related to artworks
- **Galleries** - `/galleries` - Museum gallery spaces
- **Exhibitions** - `/exhibitions` - Past and current exhibitions
- **Agent Types** - `/agent-types` - Classification of agents
- **Agent Roles** - `/agent-roles` - Roles agents play in relation to artworks
- **Artwork Place Qualifiers** - `/artwork-place-qualifiers` - Geographic relationship qualifiers
- **Artwork Date Qualifiers** - `/artwork-date-qualifiers` - Temporal relationship qualifiers
- **Artwork Types** - `/artwork-types` - Classification of artworks
- **Category Terms** - `/category-terms` - Hierarchical categories
- **Images** - `/images` - Digital images
- **Videos** - `/videos` - Video content
- **Sounds** - `/sounds` - Audio content
- **Texts** - `/texts` - Textual content

### Shop (1 endpoint)

- **Products** - `/products` - Museum shop products

### Mobile (2 endpoints)

- **Tours** - `/tours` - Self-guided mobile tours
- **Mobile Sounds** - `/mobile-sounds` - Audio content for mobile tours

### Digital Scholarly Catalogs (2 endpoints)

- **Publications** - `/publications` - Scholarly digital publications
- **Sections** - `/sections` - Publication sections and articles

### Static Archive (1 endpoint)

- **Sites** - `/sites` - Archived websites and digital content

### Website (12 endpoints)

- **Events** - `/events` - Museum events and programs
- **Event Occurrences** - `/event-occurrences` - Specific event instances
- **Event Programs** - `/event-programs` - Event program categories
- **Articles** - `/articles` - Website articles and blog posts
- **Highlights** - `/highlights` - Featured content
- **Static Pages** - `/static-pages` - Website static content
- **Generic Pages** - `/generic-pages` - General website pages
- **Press Releases** - `/press-releases` - Press releases and media
- **Educator Resources** - `/educator-resources` - Educational materials
- **Digital Publications** - `/digital-publications` - Online publications
- **Digital Publication Articles** - `/digital-publication-sections` - Publication articles
- **Printed Publications** - `/printed-publications` - Print publications

## Usage Examples

### Search for Artworks

```csharp
using WebSpark.ArtSpark.Client.Models.Common;

// Full-text search
var searchQuery = new SearchQuery 
{ 
    Q = "Van Gogh", 
    Size = 20,
    Fields = "id,title,artist_display,date_display,image_id"
};

var searchResults = await client.SearchArtworksAsync(searchQuery);

foreach (var artwork in searchResults.Data ?? [])
{
    Console.WriteLine($"{artwork.Title} - {artwork.ArtistDisplay}");
}
```

### Get Specific Fields

```csharp
// Request only specific fields
var artwork = await client.GetArtworkAsync(
    id: 20684, 
    fields: new[] { "id", "title", "artist_display", "date_display", "image_id" }
);

if (artwork != null)
{
    Console.WriteLine($"Title: {artwork.Title}");
    Console.WriteLine($"Artist: {artwork.ArtistDisplay}");
    Console.WriteLine($"Date: {artwork.DateDisplay}");
}
```

### Work with IIIF Images

```csharp
var artwork = await client.GetArtworkAsync(20684);
if (!string.IsNullOrEmpty(artwork?.ImageId))
{
    // Get different image sizes
    var thumbnailUrl = client.BuildIiifUrl(artwork.ImageId, "200,", "jpg");
    var mediumUrl = client.BuildIiifUrl(artwork.ImageId, "843,", "jpg");
    var largeUrl = client.BuildIiifUrl(artwork.ImageId, "1686,", "jpg");
    
    Console.WriteLine($"Thumbnail: {thumbnailUrl}");
    Console.WriteLine($"Medium: {mediumUrl}");
    Console.WriteLine($"Large: {largeUrl}");
}
```

### Browse Exhibitions

```csharp
var query = new ApiQuery { Limit = 10 };
var exhibitions = await client.GetExhibitionsAsync(query);

foreach (var exhibition in exhibitions.Data ?? [])
{
    Console.WriteLine($"{exhibition.Title}");
    Console.WriteLine($"  Status: {exhibition.Status}");
    Console.WriteLine($"  Dates: {exhibition.DateDisplay}");
}
```

### Get Related Resources

```csharp
// Get artwork with related agents and galleries
var artwork = await client.GetArtworkAsync(
    id: 20684,
    include: new[] { "agent", "gallery" }
);

if (artwork != null)
{
    Console.WriteLine($"Gallery: {artwork.GalleryTitle}");
    // Access included related data through the response
}
```

### Batch Operations

```csharp
// Get multiple artworks by IDs
var artworkIds = new object[] { 20684, 229393, 125592 };
var batchResponse = await client.GetResourcesByIdsAsync<ArtWork>(
    "artworks", 
    artworkIds, 
    fields: new[] { "id", "title", "artist_display" }
);

foreach (var artwork in batchResponse.Data ?? [])
{
    Console.WriteLine($"{artwork.Title} - {artwork.ArtistDisplay}");
}
```

### Work with Other Resources

```csharp
// Browse museum events
var events = await client.GetEventsAsync(new ApiQuery { Limit = 5 });

// Search for educational resources
var eduResources = await client.SearchEducatorResourcesAsync(
    new SearchQuery { Q = "impressionism", Size = 10 }
);

// Get mobile tours
var tours = await client.GetToursAsync();

// Browse shop products
var products = await client.GetProductsAsync(new ApiQuery { Limit = 20 });
```

## 🎭 AI Chat with Personas

Experience artworks like never before with our revolutionary AI chat system! Chat with artworks from multiple perspectives using our intelligent persona system.

### Available Personas

#### 🖼️ Artwork Persona

Chat directly with the artwork itself! The artwork takes on consciousness and shares its personal story:

- First-person narrative from the artwork's perspective
- Personal experiences from creation to museum display
- Cultural significance and sacred purposes
- Visual self-description using AI vision capabilities

#### 🎨 Artist Persona

Converse with the artist who created the work:

- Creative process and inspiration behind the piece
- Technical methods and materials used
- Historical context of creation
- Personal stories and cultural motivations

#### 🏛️ Curator Persona

Get professional museum curator insights:

- Art historical analysis and interpretation
- Comparative studies with other works
- Exhibition context and significance
- Academic research and scholarly perspectives

#### 📚 Historian Persona

Learn from a historical expert:

- Cultural and historical context of the time period
- Social and political background during creation
- Cross-cultural connections and influences
- Impact of historical events on artistic expression

### Quick Start with AI Chat

```csharp
using WebSpark.ArtSpark.Agent.Interfaces;
using WebSpark.ArtSpark.Agent.Models;
using WebSpark.ArtSpark.Agent.Personas;

// Inject the chat agent in your controller
public class ArtworkController : ControllerBase
{
    private readonly IArtworkChatAgent _chatAgent;
    
    public ArtworkController(IArtworkChatAgent chatAgent)
    {
        _chatAgent = chatAgent;
    }
    
    // Chat with an artwork as the artwork itself
    var request = new ChatRequest
    {
        ArtworkId = 111628,
        Message = "Tell me about your cultural significance",
        Persona = ChatPersona.Artwork,
        IncludeVisualAnalysis = true
    };
    
    var response = await _chatAgent.ChatAsync(request);
    
    if (response.Success)
    {
        Console.WriteLine($"Artwork says: {response.Response}");
        Console.WriteLine($"Suggested questions: {string.Join(", ", response.SuggestedQuestions)}");
    }
}
```

### Chat Features

- **🧠 Contextual Conversations**: Maintains conversation history for natural dialogues
- **👁️ Visual Analysis**: AI vision capabilities for detailed artwork descriptions
- **🎯 Cultural Sensitivity**: Respectful handling of cultural artifacts and contexts
- **⚡ Real-time Chat**: Fast, responsive AI-powered conversations
- **🔧 Configurable**: Flexible settings for different educational needs

### Demo Experience

Experience the interactive chat feature in our [live demo application](https://artspark.markhazleton.com) where you can:

- Switch between different personas seamlessly
- View suggested conversation starters for each persona
- Experience persistent chat history
- See AI vision analysis in action

---

## 🏗️ Technical Architecture & Recent Enhancements

### Core Technologies

- **.NET 9.0**: Latest .NET features and performance improvements
- **ASP.NET Core Identity**: User authentication and authorization
- **Entity Framework Core**: Data access with SQLite database
- **Microsoft Semantic Kernel**: AI orchestration and conversation management
- **OpenAI GPT-4 Vision**: Advanced AI with image analysis capabilities
- **Bootstrap 5**: Modern, mobile-first responsive design
- **WebSpark.Bootswatch**: Dynamic theme switching capabilities

### Recent Major Enhancements

#### 🎭 AI Chat System

- **Four distinct AI personas** for different conversation perspectives
- **Visual analysis integration** using OpenAI Vision API
- **Conversation memory** with persistent chat history
- **Cultural sensitivity** guidelines for respectful interactions

#### 👤 User Management

- **ASP.NET Core Identity** implementation with SQLite
- **User collections** for organizing favorite artworks
- **Personal artwork reviews** and ratings system
- **Secure user authentication** and session management

#### 🎨 Enhanced UI/UX

- **Mobile-first navigation** with Bootstrap 5
- **Dynamic theme switching** with Bootswatch integration
- **Responsive design patterns** for all screen sizes
- **ViewComponent architecture** for modular UI development

#### 🔍 Advanced Search & Filtering

- **Artwork filtering** by style, medium, and classification
- **Enhanced search capabilities** with Elasticsearch integration
- **Batch operations** for efficient data retrieval
- **IIIF image support** for high-quality artwork display

#### 🧪 Developer Experience

- **Comprehensive Logging** with Serilog implementation
- **Build Information Display** with version tracking
- **Dependency Injection** patterns throughout
- **Comprehensive Documentation** and implementation guides
- **Live Testing** validated across desktop, tablet, and mobile devices
- **Production Ready** with 26+ Bootswatch themes and robust error handling

---

## Project Structure

```text
WebSpark.ArtSpark/
├── WebSpark.ArtSpark.Client/            # 📚 Main client library
│   ├── Clients/
│   │   └── ArtInstituteClient.cs        # Complete API client implementation
│   ├── Interfaces/
│   │   └── IArtInstituteClient.cs       # Client interface
│   ├── Models/
│   │   ├── Collections/                 # Collection resource models
│   │   ├── Shop/                        # Shop resource models
│   │   ├── Mobile/                      # Mobile app resource models
│   │   ├── DigitalScholarlyCalatogs/    # Scholarly publication models
│   │   ├── StaticArchive/               # Archive resource models
│   │   ├── Website/                     # Website resource models
│   │   └── Common/                      # Shared models and utilities
│   └── README.md                        # Client library documentation
├── WebSpark.ArtSpark.Agent/             # 🎭 AI Chat System
│   ├── Configuration/                   # Configuration classes
│   ├── Extensions/                      # Service registration & examples
│   ├── Interfaces/                      # Core interfaces
│   ├── Models/                          # Chat request/response models
│   ├── Personas/                        # AI persona implementations
│   ├── Services/                        # Core chat services
│   └── README.md                        # Agent library documentation
├── WebSpark.ArtSpark.Demo/              # 🚀 Interactive web application
│   ├── Controllers/                     # MVC controllers
│   ├── Data/                           # Entity Framework context
│   ├── Migrations/                     # Database migrations
│   ├── Models/                         # View models
│   ├── Services/                       # Application services
│   ├── ViewComponents/                 # Reusable UI components
│   ├── Views/                          # Razor views
│   ├── wwwroot/                        # Static web assets
│   └── README.md                        # Demo application guide
├── WebSpark.ArtSpark.Console/           # 💻 Command-line application
│   ├── Program.cs                       # Console app entry point
│   └── appsettings.json                # Configuration
├── docs/                                # 📖 Comprehensive documentation
│   ├── AI-Chat-Personas-Implementation.md
│   ├── Final-Implementation-Report.md
│   └── [Additional implementation guides]
└── README.md                           # This file
```

### Project Dependencies

```text
WebSpark.ArtSpark.Demo
├── WebSpark.ArtSpark.Agent (AI chat features)
└── WebSpark.ArtSpark.Client (API access)

WebSpark.ArtSpark.Agent
├── Microsoft.SemanticKernel (AI orchestration)
├── Microsoft.SemanticKernel.Connectors.OpenAI (OpenAI integration)
└── WebSpark.ArtSpark.Client (artwork data)

WebSpark.ArtSpark.Console
└── WebSpark.ArtSpark.Client (API access)
```

## Models

The library includes comprehensive models for all API resources:

### Core Models

- `ArtWork` - Museum artworks and objects
- `Agent` - Artists, creators, organizations
- `Gallery` - Museum gallery spaces
- `Exhibition` - Museum exhibitions
- `Place` - Geographic locations

### Content Models

- `Image`, `Video`, `Sound`, `Text` - Digital media resources
- `Tour`, `MobileSound` - Mobile app content
- `Publication`, `Section` - Scholarly publications

### Website Models

- `Article`, `Event`, `Highlight` - Website content
- `EducatorResource` - Educational materials
- `PressRelease` - Media and communications

### Query Models

- `ApiQuery` - Standard API query parameters
- `SearchQuery` - Search-specific parameters
- `ApiResponse<T>` - Standard API responses
- `SearchResponse<T>` - Search responses

## Client Configuration

### HTTP Client Configuration

```csharp
var httpClientHandler = new HttpClientHandler();
var httpClient = new HttpClient(httpClientHandler)
{
    Timeout = TimeSpan.FromSeconds(30)
};

// Optional: Add custom headers
httpClient.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");

var client = new ArtInstituteClient(httpClient);
```

### JSON Serialization

The client automatically configures JSON serialization with:

- Snake case naming policy (e.g., `date_display`)
- Null value ignoring
- Case-insensitive property names

### AI Agent Configuration

For applications using the AI chat features:

```json
{
  "ArtSparkAgent": {
    "OpenAI": {
      "ApiKey": "your-openai-api-key",
      "ModelId": "gpt-4o",
      "VisionModelId": "gpt-4o",
      "MaxTokens": 1000,
      "Temperature": 0.7
    },
    "Cache": {
      "Enabled": true,
      "ConversationTimeoutMinutes": 60
    },
    "DefaultChatSettings": {
      "MaxTokens": 1000,
      "Temperature": 0.7,
      "TopP": 0.9
    }
  }
}
```

## 🚀 Deployment & Production

### Live Application

The WebSpark.ArtSpark application is currently running live at:

**🌐 [https://artspark.markhazleton.com](https://artspark.markhazleton.com)**

### Environment Setup

The demo application supports multiple deployment scenarios:

#### Local Development

```bash
dotnet run --project WebSpark.ArtSpark.Demo
```

#### Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "WebSpark.ArtSpark.Demo.dll"]
```

#### Azure Deployment

The solution is ready for Azure App Service deployment with the following recommended services:

- **App Service**: Web application hosting
- **Azure SQL**: Production database (optional, defaults to SQLite)  
- **Key Vault**: Secure API key storage
- **Application Insights**: Monitoring and telemetry

### Production Considerations

#### Security

- Store OpenAI API keys in Azure Key Vault or similar secure storage
- Enable HTTPS in production environments
- Configure proper CORS policies
- Use production-grade database (Azure SQL, PostgreSQL)

#### Performance

- Enable response caching for API responses
- Configure CDN for static assets
- Use Application Insights for monitoring
- Set appropriate timeout values for external API calls

#### Data Privacy

- AI conversation history is stored in-memory by default
- No conversation data is sent to external services without explicit configuration
- User data follows ASP.NET Core Identity security practices

## Development

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

### Quality Audit

This solution includes an automated quality audit system to maintain build health and dependency currency:

```powershell
# Run full quality audit
pwsh -File scripts/audit/run-quality-audit.ps1

# Generate filtered backlog
pwsh -File scripts/audit/run-quality-audit.ps1 -Severity Warning -MaxItems 10
```

**Audit Reports**: Generated under `docs/copilot/YYYY-MM-DD/quality-audit.md`

The audit checks:
- ✅ Build diagnostics (compiler warnings/errors)
- ✅ NuGet package currency
- ✅ npm package currency (if package.json exists)
- ✅ AI safeguard compliance (persona definitions, moderation hooks)

See [`specs/001-quality-audit/quickstart.md`](specs/001-quality-audit/quickstart.md) for detailed usage.

### Development Setup

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Make your changes
4. Add tests if applicable
5. Run quality audit to verify changes
6. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
7. Push to the branch (`git push origin feature/AmazingFeature`)
8. Open a Pull Request

### Acknowledgments

- [Art Institute of Chicago](https://www.artic.edu/) for providing the public API
- [Art Institute of Chicago API Documentation](https://api.artic.edu/docs/)
- The museum's commitment to open access and digital scholarship

### Support

If you encounter any issues or have questions:

1. Check the [API documentation](https://api.artic.edu/docs/)
2. Search existing [GitHub issues](https://github.com/MarkHazleton/WebSpark.ArtSpark/issues)
3. Create a new issue if needed

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## Final Note

🎨 **Ready to explore art and technology together?**

👉 **Experience the live application**: [artspark.markhazleton.com](https://artspark.markhazleton.com)  
👨‍💻 **Start building with the API**: Clone this repository and run the examples  
🎭 **Chat with artworks**: Set up your OpenAI API key and experience AI personas  

Happy coding with the Art Institute of Chicago API! 🎨
