using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using WebSpark.ArtSpark.Agent.Configuration;
using WebSpark.ArtSpark.Agent.Personas;
using WebSpark.ArtSpark.Agent.Services;

namespace WebSpark.ArtSpark.Tests.Console;

/// <summary>
/// Tests for prompt variant loading in console harness scenarios.
/// Validates that developers can test different prompt variants locally.
/// </summary>
public class PromptVariantTests : IDisposable
{
    private readonly string _testDataPath;
    private readonly string _variantsPath;

    public PromptVariantTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), "artspark-test-variants", Guid.NewGuid().ToString());
        _variantsPath = Path.Combine(_testDataPath, "variants");
        Directory.CreateDirectory(_testDataPath);
        Directory.CreateDirectory(_variantsPath);
    }

    [Fact]
    public async Task GetPromptAsync_WithVariantsPath_LoadsFromVariantsDirectory()
    {
        // Arrange
        var defaultPrompt = @"---
model: gpt-4o
temperature: 0.7
---

# Default Artwork Prompt

## CULTURAL SENSITIVITY
Default sensitivity

## CONVERSATION GUIDELINES
Default guidelines";

        var variantPrompt = @"---
model: gpt-4o
temperature: 0.9
---

# Variant Artwork Prompt (Experimental)

## CULTURAL SENSITIVITY
Enhanced variant sensitivity

## CONVERSATION GUIDELINES
Experimental variant guidelines";

        // Write default prompt
        File.WriteAllText(
            Path.Combine(_testDataPath, "artspark.artwork.prompt.md"),
            defaultPrompt);

        // Write variant prompt
        File.WriteAllText(
            Path.Combine(_variantsPath, "artspark.artwork.prompt.md"),
            variantPrompt);

        var options = Options.Create(new PromptOptions
        {
            DataPath = _variantsPath, // Point to variants directory
            FallbackToDefault = true
        });

        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<PromptLoader>>();
        var loader = new PromptLoader(options, mockLogger.Object);

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Artwork);

        // Assert
        Assert.NotNull(template);
        Assert.Contains("Variant Artwork Prompt", template.Content);
        Assert.Contains("Experimental variant guidelines", template.Content);
        Assert.Equal(0.9, template.MetadataOverrides?.Temperature);
    }

    [Fact]
    public async Task GetPromptAsync_VariantMissing_FallsBackToDefault()
    {
        // Arrange
        var defaultPrompt = @"---
model: gpt-4o
---

# Default Curator Prompt

## CULTURAL SENSITIVITY
Default sensitivity

## CONVERSATION GUIDELINES
Default guidelines";

        // Only write default, no variant
        File.WriteAllText(
            Path.Combine(_testDataPath, "artspark.curator.prompt.md"),
            defaultPrompt);

        var options = Options.Create(new PromptOptions
        {
            DataPath = _variantsPath, // Point to variants (which doesn't have this file)
            FallbackToDefault = true
        });

        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<PromptLoader>>();
        var loader = new PromptLoader(options, mockLogger.Object);

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Curator);

        // Assert
        Assert.NotNull(template);
        // Should fall back to hardcoded default since file not in variants
        Assert.NotNull(template.Content);
    }

    [Fact]
    public void ConsoleConfiguration_CanLoadVariantsPathFromConfig()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            { "ArtSparkAgent:Prompts:DataPath", "./prompts/agents" },
            { "ArtSparkAgent:Prompts:VariantsPath", "./prompts/agents/variants" },
            { "ArtSparkAgent:Prompts:EnableHotReload", "true" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var services = new ServiceCollection();
        services.Configure<PromptOptions>(configuration.GetSection("ArtSparkAgent:Prompts"));
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetRequiredService<IOptions<PromptOptions>>().Value;

        // Assert
        Assert.Equal("./prompts/agents", options.DataPath);
        Assert.Equal("./prompts/agents/variants", options.VariantsPath);
        Assert.True(options.EnableHotReload);
    }

    [Fact]
    public async Task MultipleVariants_CanSwitchBetweenVariants()
    {
        // Arrange
        var variant1Path = Path.Combine(_testDataPath, "variant1");
        var variant2Path = Path.Combine(_testDataPath, "variant2");
        Directory.CreateDirectory(variant1Path);
        Directory.CreateDirectory(variant2Path);

        var variant1Content = @"---
temperature: 0.5
---

# Variant 1 - Conservative

## CULTURAL SENSITIVITY
Very careful approach

## CONVERSATION GUIDELINES
Conservative guidelines";

        var variant2Content = @"---
temperature: 0.95
---

# Variant 2 - Creative

## CULTURAL SENSITIVITY
Creative yet respectful

## CONVERSATION GUIDELINES
Experimental creative guidelines";

        File.WriteAllText(
            Path.Combine(variant1Path, "artspark.artist.prompt.md"),
            variant1Content);

        File.WriteAllText(
            Path.Combine(variant2Path, "artspark.artist.prompt.md"),
            variant2Content);

        // Test variant 1
        var options1 = Options.Create(new PromptOptions { DataPath = variant1Path });
        var mockLogger1 = new Mock<Microsoft.Extensions.Logging.ILogger<PromptLoader>>();
        var loader1 = new PromptLoader(options1, mockLogger1.Object);
        var template1 = await loader1.GetPromptAsync(ChatPersona.Artist);

        // Test variant 2
        var options2 = Options.Create(new PromptOptions { DataPath = variant2Path });
        var mockLogger2 = new Mock<Microsoft.Extensions.Logging.ILogger<PromptLoader>>();
        var loader2 = new PromptLoader(options2, mockLogger2.Object);
        var template2 = await loader2.GetPromptAsync(ChatPersona.Artist);

        // Assert
        Assert.Contains("Conservative", template1.Content);
        Assert.Equal(0.5, template1.MetadataOverrides?.Temperature);

        Assert.Contains("Creative", template2.Content);
        Assert.Equal(0.95, template2.MetadataOverrides?.Temperature);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDataPath))
        {
            Directory.Delete(_testDataPath, recursive: true);
        }
    }
}
