using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebSpark.ArtSpark.Agent.Configuration;
using WebSpark.ArtSpark.Agent.Personas;
using WebSpark.ArtSpark.Agent.Services;

namespace WebSpark.ArtSpark.Tests.Agent.Services;

public class PromptLoaderTests : IDisposable
{
    private readonly Mock<ILogger<PromptLoader>> _mockLogger;
    private readonly string _testDataPath;

    public PromptLoaderTests()
    {
        _mockLogger = new Mock<ILogger<PromptLoader>>();
        _testDataPath = Path.Combine(Path.GetTempPath(), "artspark-test-prompts", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);
    }

    [Fact]
    public async Task GetPromptAsync_ValidFile_ReturnsLoadedTemplate()
    {
        // Arrange
        var promptContent = @"---
model: gpt-4o
temperature: 0.8
---

# Test Prompt

## CULTURAL SENSITIVITY
Test sensitivity content

## CONVERSATION GUIDELINES
Test guidelines content";

        var fileName = "artspark.artwork.prompt.md";
        File.WriteAllText(Path.Combine(_testDataPath, fileName), promptContent);

        var options = Options.Create(new PromptOptions
        {
            DataPath = _testDataPath,
            FallbackToDefault = true
        });

        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Artwork);

        // Assert
        Assert.NotNull(template);
        Assert.False(template.IsFallback);
        Assert.Equal(ChatPersona.Artwork, template.PersonaType);
        Assert.Contains("Test Prompt", template.Content);
        Assert.Contains("Test sensitivity content", template.Content);
        Assert.NotNull(template.MetadataOverrides);
        Assert.Equal("gpt-4o", template.MetadataOverrides.ModelId);
        Assert.Equal(0.8, template.MetadataOverrides.Temperature);
    }

    [Fact]
    public async Task GetPromptAsync_MissingFile_ReturnsFallbackTemplate()
    {
        // Arrange
        var options = Options.Create(new PromptOptions
        {
            DataPath = _testDataPath,
            FallbackToDefault = true
        });

        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Artwork);

        // Assert
        Assert.NotNull(template);
        Assert.True(template.IsFallback);
        Assert.Equal("[fallback]", template.FilePath);
        Assert.NotEmpty(template.Content);
    }

    [Fact]
    public async Task GetPromptAsync_MissingRequiredSection_ReturnsFallback()
    {
        // Arrange
        var promptContent = @"# Test Prompt

## CONVERSATION GUIDELINES
Missing CULTURAL SENSITIVITY section";

        var fileName = "artspark.artwork.prompt.md";
        File.WriteAllText(Path.Combine(_testDataPath, fileName), promptContent);

        var options = Options.Create(new PromptOptions
        {
            DataPath = _testDataPath,
            FallbackToDefault = true
        });

        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Artwork);

        // Assert
        Assert.True(template.IsFallback);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("validation failed")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPromptAsync_InvalidToken_ReturnsFallback()
    {
        // Arrange
        var promptContent = @"# Test Prompt

## CULTURAL SENSITIVITY
Test sensitivity

## CONVERSATION GUIDELINES
Test guidelines {artwork.InvalidProperty}";

        var fileName = "artspark.artwork.prompt.md";
        File.WriteAllText(Path.Combine(_testDataPath, fileName), promptContent);

        var options = Options.Create(new PromptOptions
        {
            DataPath = _testDataPath,
            FallbackToDefault = true
        });

        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Artwork);

        // Assert
        Assert.True(template.IsFallback);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("PromptTokenValidationFailed")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetPromptAsync_EmptyFile_ReturnsFallback()
    {
        // Arrange
        var fileName = "artspark.artwork.prompt.md";
        File.WriteAllText(Path.Combine(_testDataPath, fileName), string.Empty);

        var options = Options.Create(new PromptOptions
        {
            DataPath = _testDataPath,
            FallbackToDefault = true
        });

        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Artwork);

        // Assert
        Assert.True(template.IsFallback);
    }

    [Fact]
    public async Task GetPromptAsync_CachesLoadedPrompt()
    {
        // Arrange
        var promptContent = @"---
model: gpt-4o
---

# Test Prompt

## CULTURAL SENSITIVITY
Test

## CONVERSATION GUIDELINES
Test";

        var fileName = "artspark.artwork.prompt.md";
        var filePath = Path.Combine(_testDataPath, fileName);
        File.WriteAllText(filePath, promptContent);

        var options = Options.Create(new PromptOptions
        {
            DataPath = _testDataPath,
            FallbackToDefault = true
        });

        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act
        var template1 = await loader.GetPromptAsync(ChatPersona.Artwork);
        var template2 = await loader.GetPromptAsync(ChatPersona.Artwork);

        // Assert
        Assert.Same(template1, template2);
    }

    [Fact]
    public async Task GetPromptAsync_ValidatesContentHash()
    {
        // Arrange
        var promptContent = @"# Test Prompt

## CULTURAL SENSITIVITY
Test

## CONVERSATION GUIDELINES
Test";

        var fileName = "artspark.artwork.prompt.md";
        File.WriteAllText(Path.Combine(_testDataPath, fileName), promptContent);

        var options = Options.Create(new PromptOptions
        {
            DataPath = _testDataPath,
            FallbackToDefault = true
        });

        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Artwork);

        // Assert
        Assert.NotEmpty(template.ContentHash);
        Assert.Equal(32, template.ContentHash.Length); // MD5 hash in hex is 32 chars
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDataPath))
        {
            Directory.Delete(_testDataPath, recursive: true);
        }
    }
}
