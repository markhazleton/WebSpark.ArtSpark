# 🎨 WebSpark.ArtSpark

**Where Open Art Meets .NET Excellence**

WebSpark.ArtSpark is a modern, open-source ASP.NET Core application that beautifully merges the cultural richness of the [Art Institute of Chicago](https://api.artic.edu/docs/) with the architectural clarity of the WebSpark NuGet packages. It demonstrates how public data, art, and .NET development can intersect to create immersive, responsive, and resilient applications.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/WebSpark.Bootswatch.svg)](https://www.nuget.org/packages/WebSpark.Bootswatch/)
[![NuGet](https://img.shields.io/nuget/v/WebSpark.HttpClientUtility.svg)](https://www.nuget.org/packages/WebSpark.HttpClientUtility/)
[![.NET Version](https://img.shields.io/badge/.NET-10%20(Preview)-purple)](https://dotnet.microsoft.com/)

---

## 🚀 Live Demo

Experience WebSpark.ArtSpark live at: **[https://art.makeboldspark.com](https://art.makeboldspark.com)**

---

## 📦 NuGet Packages Used

### [WebSpark.Bootswatch](https://www.nuget.org/packages/WebSpark.Bootswatch)

* Runtime Bootswatch theme switching
* Integrated with Razor Pages/MVC
* Cookie-based user theme persistence

### [WebSpark.HttpClientUtility](https://www.nuget.org/packages/WebSpark.HttpClientUtility)

* Simplified, resilient HttpClient with Polly
* Retry, timeout, circuit breaker, and fallback
* Integrated logging and telemetry support

---

## ✨ Features

* 🎨 Explore public domain artworks from AIC
* 🎲 **Random Collection Showcase** - Dynamic home page featuring random public collections
* 🎭 **AI Chat with Personas** - Revolutionary chat system with 4 AI personas (Artwork, Artist, Curator, Historian)
* 👁️ **Visual Analysis** - AI-powered image analysis with OpenAI Vision
* 🧠 **Conversation Memory** - Persistent chat history and contextual conversations
* 🎭 Live theme switching with Bootswatch themes
* 🔁 Resilient API integrations via HttpClientFactory
* 🧠 Developer and educator modes
* 🖼️ Curated and themed collections
* 🔍 Deep metadata and IIIF support
* 🎯 **Cultural Sensitivity** - Respectful handling of cultural artifacts
* 🔄 **Interactive Discovery** - "New Collection" button for instant content refresh

---

## 🧰 Tech Stack

* ASP.NET Core 10 Preview (Razor Pages)
* Bootstrap 5 with Bootswatch
* Polly for .NET resilience
* Art Institute of Chicago API
* Custom middleware and component system
* Microsoft Semantic Kernel for AI capabilities

---

## 🎭 AI Chat with Personas

Experience artworks like never before with our groundbreaking AI chat system! Each artwork can be explored through four distinct AI personas:

### Available Personas

#### 🖼️ Artwork Persona

* Chat directly with the artwork itself

* First-person narratives from the art's perspective  
* Personal stories from creation to museum display
* AI vision-powered self-descriptions

#### 🎨 Artist Persona  

* Converse with the artist who created the work

* Learn about creative process and inspiration
* Understand techniques and cultural context
* Discover personal motivations behind the art

#### 🏛️ Curator Persona

* Professional museum insights and analysis

* Art historical context and interpretation
* Comparative studies with other works
* Academic research perspectives

#### 📚 Historian Persona

* Deep historical and cultural context

* Social and political background of the era
* Cross-cultural influences and connections
* Impact of historical events on artistic expression

### Chat Features

* **🧠 Contextual Memory**: Conversations maintain history for natural dialogue flow
* **👁️ AI Vision**: Advanced image analysis describes artwork details
* **🎯 Cultural Sensitivity**: Respectful handling of cultural artifacts
* **⚡ Real-time**: Fast, responsive AI-powered conversations
* **🔧 Configurable**: Adaptable settings for different educational needs

Try it: Navigate to any artwork detail page and start chatting!

---

## 📐 Architecture Example

```csharp
// Program.cs
builder.Services.AddRazorPages();
builder.Services.AddBootswatchStyles();
builder.Services.AddBootswatchThemeSwitcher();
builder.Services.AddHttpClientWithPolicies("ArtApiClient", client =>
{
    client.BaseAddress = new Uri("https://api.artic.edu/api/v1/");
});
```

```html
<!-- _Layout.cshtml -->
<script src="/_content/WebSpark.Bootswatch/js/bootswatch-theme-switcher.js"></script>
```

```json
{
  "id": 27992,
  "title": "Water Lilies",
  "artist_display": "Claude Monet",
  "image_id": "e9a1c4d3-f9f7-4a55-ae6c-8f742abe56a3"
}
```

---

## 🧪 Getting Started

### Prerequisites

* [.NET 10 SDK (Preview)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
* A modern code editor (Visual Studio 2022+, VS Code)

> **Note**: .NET 10 is currently in preview. For production deployments, consider using the latest LTS version.

### Clone and Run

```bash
git clone https://github.com/MarkHazleton/WebSpark.ArtSpark.git
cd WebSpark.ArtSpark
dotnet run --project WebSpark.ArtSpark.Demo
```

Then visit `http://localhost:5139` or `https://localhost:7282` in your browser.

For the live production version, visit: **[https://art.makeboldspark.com](https://art.makeboldspark.com)**

---

## 🧭 Roadmap

* [ ] User-curated collections with login support
* [ ] Export to PDF/Markdown
* [ ] Plugins for additional art APIs (MoMA, MET)
* [ ] `dotnet new artspark` project template

---

## 📚 Related Resources

* [Art Institute of Chicago API](https://api.artic.edu/docs/)
* [Bootswatch](https://bootswatch.com/)
* [Polly GitHub](https://github.com/App-vNext/Polly)
* [WebSpark NuGet Packages](https://www.nuget.org/profiles/markhazleton)
* [Project Mechanics Blog](https://markhazleton.com)

---

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a new branch (`git checkout -b feature/your-feature-name`)
3. Commit your changes (`git commit -m 'Add your message here'`)
4. Push to the branch (`git push origin feature/your-feature-name`)
5. Open a pull request

Issues and suggestions are also welcome via the [GitHub Issues page](https://github.com/MarkHazleton/WebSpark.ArtSpark/issues).

---

## 🧾 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

## 💡 About

**WebSpark.ArtSpark** is developed and maintained by [Mark Hazleton](https://markhazleton.com), showcasing the power of open public datasets and modern .NET development.

> WebSpark.ArtSpark — bridging creative culture and clean code.
