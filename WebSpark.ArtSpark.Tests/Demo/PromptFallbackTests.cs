using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebSpark.ArtSpark.Agent.Configuration;
using WebSpark.ArtSpark.Agent.Personas;
using WebSpark.ArtSpark.Agent.Services;

namespace WebSpark.ArtSpark.Tests.Demo;

/// <summary>
/// Integration tests verifying prompt loading behavior in Demo scenarios.
/// Tests file-based prompts, metadata overrides, and fallback mechanisms.
/// </summary>
public class PromptFallbackTests : IDisposable
{
    private readonly string _testDataPath;
    private readonly IServiceProvider _serviceProvider;

    public PromptFallbackTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), "artspark-demo-test", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);

        // Setup DI container similar to Demo
        var services = new ServiceCollection();

        var configData = new Dictionary<string, string?>
        {
            { "ArtSparkAgent:Prompts:DataPath", _testDataPath },
            { "ArtSparkAgent:Prompts:EnableHotReload", "false" },
            { "ArtSparkAgent:Prompts:FallbackToDefault", "true" },
            { "ArtSparkAgent:Prompts:DefaultMetadata:ModelId", "gpt-4o" },
            { "ArtSparkAgent:Prompts:DefaultMetadata:Temperature", "0.7" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<PromptOptions>(configuration.GetSection("ArtSparkAgent:Prompts"));
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddSingleton<IPromptLoader, PromptLoader>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Demo_FileBasedPrompt_LoadsSuccessfully()
    {
        // Arrange
        var promptContent = @"---
model: gpt-4o
temperature: 0.8
---

# Artwork Persona for Demo

## CULTURAL SENSITIVITY
Respectful approach to artwork interpretation

## CONVERSATION GUIDELINES
Engage visitors with artwork details";

        File.WriteAllText(
            Path.Combine(_testDataPath, "artspark.artwork.prompt.md"),
            promptContent);

        var loader = _serviceProvider.GetRequiredService<IPromptLoader>();

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Artwork);

        // Assert
        Assert.NotNull(template);
        Assert.Contains("Artwork Persona for Demo", template.Content);
        Assert.Equal(0.8, template.MetadataOverrides?.Temperature);
        Assert.Equal("gpt-4o", template.MetadataOverrides?.ModelId);
    }

    [Fact]
    public async Task Demo_MetadataOverride_AppliesCorrectly()
    {
        // Arrange
        var promptContent = @"---
model: gpt-4-turbo
temperature: 0.9
max_output_tokens: 1500
frequency_penalty: 0.5
---

# Artist Persona with Custom Settings

## CULTURAL SENSITIVITY
Cultural sensitivity content

## CONVERSATION GUIDELINES
Conversation guidelines";

        File.WriteAllText(
            Path.Combine(_testDataPath, "artspark.artist.prompt.md"),
            promptContent);

        var loader = _serviceProvider.GetRequiredService<IPromptLoader>();

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Artist);

        // Assert - verify metadata overrides from front matter
        Assert.NotNull(template.MetadataOverrides);
        Assert.Equal("gpt-4-turbo", template.MetadataOverrides.ModelId);
        Assert.Equal(0.9, template.MetadataOverrides.Temperature);
        Assert.Equal(1500, template.MetadataOverrides.MaxOutputTokens);
        Assert.Equal(0.5, template.MetadataOverrides.FrequencyPenalty);
    }

    [Fact]
    public async Task Demo_MissingPromptFile_FallsBackToDefault()
    {
        // Arrange - no file created for Curator
        var loader = _serviceProvider.GetRequiredService<IPromptLoader>();

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Curator);

        // Assert - should return hardcoded fallback
        Assert.NotNull(template);
        Assert.NotNull(template.Content);
        Assert.NotEmpty(template.Content);
    }

    [Fact]
    public async Task Demo_InvalidPromptFile_FallsBackGracefully()
    {
        // Arrange - create invalid prompt (missing required section)
        var invalidPrompt = @"---
model: gpt-4o
---

# Invalid Historian Prompt

## CONVERSATION GUIDELINES
Only one section present (missing CULTURAL SENSITIVITY)";

        File.WriteAllText(
            Path.Combine(_testDataPath, "artspark.historian.prompt.md"),
            invalidPrompt);

        var loader = _serviceProvider.GetRequiredService<IPromptLoader>();

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Historian);

        // Assert - should fall back to default
        Assert.NotNull(template);
        Assert.NotNull(template.Content);
    }

    [Fact]
    public async Task Demo_AllPersonas_CanLoadWithMixedFileAvailability()
    {
        // Arrange - create files for some personas, not others
        File.WriteAllText(
            Path.Combine(_testDataPath, "artspark.artwork.prompt.md"),
            CreateValidPrompt("Artwork"));

        File.WriteAllText(
            Path.Combine(_testDataPath, "artspark.curator.prompt.md"),
            CreateValidPrompt("Curator"));

        // Artist and Historian will use fallback

        var loader = _serviceProvider.GetRequiredService<IPromptLoader>();

        // Act - load all four personas
        var artworkTemplate = await loader.GetPromptAsync(ChatPersona.Artwork);
        var artistTemplate = await loader.GetPromptAsync(ChatPersona.Artist);
        var curatorTemplate = await loader.GetPromptAsync(ChatPersona.Curator);
        var historianTemplate = await loader.GetPromptAsync(ChatPersona.Historian);

        // Assert - all should succeed (file-based or fallback)
        Assert.NotNull(artworkTemplate);
        Assert.Contains("Artwork Persona", artworkTemplate.Content);

        Assert.NotNull(artistTemplate);
        Assert.NotEmpty(artistTemplate.Content);

        Assert.NotNull(curatorTemplate);
        Assert.Contains("Curator Persona", curatorTemplate.Content);

        Assert.NotNull(historianTemplate);
        Assert.NotEmpty(historianTemplate.Content);
    }

    [Fact]
    public async Task Demo_DefaultMetadata_AppliesWhenNoOverride()
    {
        // Arrange - prompt without metadata front matter
        var promptContent = @"# Simple Artwork Prompt

## CULTURAL SENSITIVITY
Sensitivity content

## CONVERSATION GUIDELINES
Guidelines content";

        File.WriteAllText(
            Path.Combine(_testDataPath, "artspark.artwork.prompt.md"),
            promptContent);

        var loader = _serviceProvider.GetRequiredService<IPromptLoader>();

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Artwork);

        // Assert - MetadataOverrides should be null when no front matter present
        // Application layer will merge with defaults at runtime
        Assert.Null(template.MetadataOverrides);
    }

    [Fact]
    public async Task Demo_PartialMetadataOverride_MergesWithDefaults()
    {
        // Arrange - override only temperature, other fields use defaults
        var promptContent = @"---
temperature: 0.95
---

# Artist Persona with Partial Override

## CULTURAL SENSITIVITY
Sensitivity

## CONVERSATION GUIDELINES
Guidelines";

        File.WriteAllText(
            Path.Combine(_testDataPath, "artspark.artist.prompt.md"),
            promptContent);

        var loader = _serviceProvider.GetRequiredService<IPromptLoader>();

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Artist);

        // Assert - temperature overridden, model uses default
        Assert.NotNull(template.MetadataOverrides);
        Assert.Equal("gpt-4o", template.MetadataOverrides.ModelId); // Default
        Assert.Equal(0.95, template.MetadataOverrides.Temperature);  // Override
    }

    [Fact]
    public async Task Demo_PromptLoading_DoesNotBlockChatFunctionality()
    {
        // Arrange - simulate various prompt states
        File.WriteAllText(
            Path.Combine(_testDataPath, "artspark.artwork.prompt.md"),
            CreateValidPrompt("Artwork"));

        var loader = _serviceProvider.GetRequiredService<IPromptLoader>();

        // Act - verify chat can proceed regardless of file state
        var tasks = new[]
        {
            loader.GetPromptAsync(ChatPersona.Artwork).AsTask(),  // File exists
            loader.GetPromptAsync(ChatPersona.Artist).AsTask(),   // No file, use fallback
            loader.GetPromptAsync(ChatPersona.Curator).AsTask(),  // No file, use fallback
            loader.GetPromptAsync(ChatPersona.Historian).AsTask() // No file, use fallback
        };

        var results = await Task.WhenAll(tasks);

        // Assert - all loads should complete without throwing
        Assert.All(results, template =>
        {
            Assert.NotNull(template);
            Assert.NotNull(template.Content);
            Assert.NotEmpty(template.Content);
        });
    }

    [Fact]
    public async Task Demo_Configuration_ValidatesAtStartup()
    {
        // Arrange - create service provider with invalid path
        var services = new ServiceCollection();

        var configData = new Dictionary<string, string?>
        {
            { "ArtSparkAgent:Prompts:DataPath", "/nonexistent/path/should/warn" },
            { "ArtSparkAgent:Prompts:FallbackToDefault", "true" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<PromptOptions>(configuration.GetSection("ArtSparkAgent:Prompts"));
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IPromptLoader, PromptLoader>();

        var provider = services.BuildServiceProvider();
        var loader = provider.GetRequiredService<IPromptLoader>();

        // Act - attempt to load with invalid path
        var template = await loader.GetPromptAsync(ChatPersona.Artwork);

        // Assert - should fall back gracefully
        Assert.NotNull(template);
        Assert.NotNull(template.Content);
    }

    private string CreateValidPrompt(string personaName) =>
        $@"---
model: gpt-4o
temperature: 0.7
---

# {personaName} Persona

## CULTURAL SENSITIVITY
Culturally sensitive content for {personaName}

## CONVERSATION GUIDELINES
Conversation guidelines for {personaName} persona";

    public void Dispose()
    {
        if (Directory.Exists(_testDataPath))
        {
            Directory.Delete(_testDataPath, recursive: true);
        }
    }
}
