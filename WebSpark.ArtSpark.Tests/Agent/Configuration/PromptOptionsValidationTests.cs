using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebSpark.ArtSpark.Agent.Configuration;
using WebSpark.ArtSpark.Agent.Personas;
using WebSpark.ArtSpark.Agent.Services;

namespace WebSpark.ArtSpark.Tests.Agent.Configuration;

public class PromptOptionsValidationTests
{
    [Fact]
    public void PromptOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new PromptOptions();

        // Assert
        Assert.Equal("./prompts/agents", options.DataPath);
        Assert.False(options.EnableHotReload);
        Assert.True(options.FallbackToDefault);
        Assert.Null(options.VariantsPath);
        Assert.NotNull(options.DefaultMetadata);
        Assert.Equal("gpt-4o", options.DefaultMetadata.ModelId);
        Assert.Equal(0.7, options.DefaultMetadata.Temperature);
    }

    [Fact]
    public void PromptOptions_CanBeConfiguredFromAppSettings()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["Prompts:DataPath"] = "./custom/prompts",
            ["Prompts:EnableHotReload"] = "true",
            ["Prompts:FallbackToDefault"] = "false",
            ["Prompts:VariantsPath"] = "./custom/variants",
            ["Prompts:DefaultMetadata:ModelId"] = "gpt-4-turbo",
            ["Prompts:DefaultMetadata:Temperature"] = "0.5"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var services = new ServiceCollection();
        services.Configure<PromptOptions>(configuration.GetSection("Prompts"));

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<PromptOptions>>().Value;

        // Assert
        Assert.Equal("./custom/prompts", options.DataPath);
        Assert.True(options.EnableHotReload);
        Assert.False(options.FallbackToDefault);
        Assert.Equal("./custom/variants", options.VariantsPath);
        Assert.Equal("gpt-4-turbo", options.DefaultMetadata.ModelId);
        Assert.Equal(0.5, options.DefaultMetadata.Temperature);
    }

    [Fact]
    public void PromptMetadata_MergeWith_PrefersOverrideValues()
    {
        // Arrange
        var baseMetadata = new PromptMetadata
        {
            ModelId = "gpt-4o",
            Temperature = 0.7,
            TopP = 0.9,
            MaxOutputTokens = 1000
        };

        var overrides = new PromptMetadata
        {
            ModelId = "gpt-4-turbo",
            Temperature = 0.5
        };

        // Act
        var merged = baseMetadata.MergeWith(overrides);

        // Assert
        Assert.Equal("gpt-4-turbo", merged.ModelId);
        Assert.Equal(0.5, merged.Temperature);
        Assert.Equal(0.9, merged.TopP);
        Assert.Equal(1000, merged.MaxOutputTokens);
    }

    [Fact]
    public void PromptMetadata_MergeWith_NullOverrides_ReturnsBase()
    {
        // Arrange
        var baseMetadata = new PromptMetadata
        {
            ModelId = "gpt-4o",
            Temperature = 0.7
        };

        // Act
        var merged = baseMetadata.MergeWith(null);

        // Assert
        Assert.Same(baseMetadata, merged);
    }

    [Fact]
    public void PromptLoader_LogsWarning_WhenDataPathDoesNotExist()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<PromptLoader>>();
        var options = Options.Create(new PromptOptions
        {
            DataPath = "./nonexistent/path"
        });

        // Act
        var loader = new PromptLoader(options, mockLogger.Object);

        // Assert - Verify warning was logged about missing path
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("does not exist")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void PersonaPromptConfiguration_CreateDefaultMap_ContainsAllPersonas()
    {
        // Act
        var map = PersonaPromptConfiguration.CreateDefaultMap();

        // Assert
        Assert.Equal(4, map.Count);
        Assert.Contains(ChatPersona.Artwork, map.Keys);
        Assert.Contains(ChatPersona.Artist, map.Keys);
        Assert.Contains(ChatPersona.Curator, map.Keys);
        Assert.Contains(ChatPersona.Historian, map.Keys);
    }

    [Fact]
    public void PersonaPromptConfiguration_AllPersonas_HaveValidFileNames()
    {
        // Act
        var map = PersonaPromptConfiguration.CreateDefaultMap();

        // Assert
        foreach (var config in map.Values)
        {
            Assert.NotEmpty(config.PromptFileName);
            Assert.EndsWith(".md", config.PromptFileName);
            Assert.StartsWith("artspark.", config.PromptFileName);
        }
    }

    [Fact]
    public void PersonaPromptConfiguration_AllPersonas_HaveFallbackPrompts()
    {
        // Act
        var map = PersonaPromptConfiguration.CreateDefaultMap();

        // Assert
        foreach (var config in map.Values)
        {
            Assert.NotEmpty(config.FallbackPrompt);
        }
    }

    [Fact]
    public void PersonaPromptConfiguration_AllPersonas_HaveAllowedTokens()
    {
        // Act
        var map = PersonaPromptConfiguration.CreateDefaultMap();

        // Assert
        foreach (var config in map.Values)
        {
            Assert.NotEmpty(config.AllowedTokens);
            Assert.All(config.AllowedTokens, token => Assert.StartsWith("artwork.", token));
        }
    }
}
