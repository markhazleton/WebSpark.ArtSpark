using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebSpark.ArtSpark.Agent.Configuration;
using WebSpark.ArtSpark.Agent.Personas;
using WebSpark.ArtSpark.Agent.Services;
using Xunit;

namespace WebSpark.ArtSpark.Tests.Agent.Services;

/// <summary>
/// Tests for hot reload functionality in PromptLoader.
/// Verifies that prompt files are reloaded when changed on disk.
/// </summary>
public sealed class PromptLoaderHotReloadTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly Mock<ILogger<PromptLoader>> _mockLogger;

    public PromptLoaderHotReloadTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"artspark_hotreload_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
        _mockLogger = new Mock<ILogger<PromptLoader>>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task HotReload_PromptBodyChanged_LoadsUpdatedContent()
    {
        // Arrange
        var fileName = "artspark.artwork.prompt.md";
        var filePath = Path.Combine(_tempDirectory, fileName);
        var originalContent = @"---
model: gpt-4o
---

## CULTURAL SENSITIVITY
Original content

## CONVERSATION GUIDELINES
Original guidelines";

        var updatedContent = @"---
model: gpt-4o
---

## CULTURAL SENSITIVITY
Updated content

## CONVERSATION GUIDELINES
Updated guidelines";

        File.WriteAllText(filePath, originalContent);

        var options = Options.Create(new PromptOptions
        {
            DataPath = _tempDirectory,
            EnableHotReload = true,
            FallbackToDefault = true
        });

        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act 1: Load original
        var original = await loader.GetPromptAsync(ChatPersona.Artwork);

        // Simulate file change
        await Task.Delay(100); // Ensure file system has processed the initial write
        File.WriteAllText(filePath, updatedContent);
        await Task.Delay(500); // Give hot reload time to detect change

        // Act 2: Load after change
        var updated = await loader.GetPromptAsync(ChatPersona.Artwork);

        // Assert
        Assert.Contains("Original content", original.Content);
        Assert.Contains("Updated content", updated.Content);
        Assert.NotEqual(original.ContentHash, updated.ContentHash);
    }

    [Fact]
    public async Task HotReload_MetadataChanged_LoadsUpdatedMetadata()
    {
        // Arrange
        var fileName = "artspark.artist.prompt.md";
        var filePath = Path.Combine(_tempDirectory, fileName);
        var originalContent = @"---
model: gpt-4o
temperature: 0.5
---

## CULTURAL SENSITIVITY
Artist analysis

## CONVERSATION GUIDELINES
Artist conversation";

        var updatedContent = @"---
model: gpt-4-turbo
temperature: 0.9
---

## CULTURAL SENSITIVITY
Artist analysis

## CONVERSATION GUIDELINES
Artist conversation";

        File.WriteAllText(filePath, originalContent);

        var options = Options.Create(new PromptOptions
        {
            DataPath = _tempDirectory,
            EnableHotReload = true,
            FallbackToDefault = true,
            DefaultMetadata = new PromptMetadata { ModelId = "gpt-3.5-turbo", Temperature = 0.7 }
        });

        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act 1: Load original
        var original = await loader.GetPromptAsync(ChatPersona.Artist);

        // Simulate file change
        await Task.Delay(100);
        File.WriteAllText(filePath, updatedContent);
        await Task.Delay(500);

        // Act 2: Load after change
        var updated = await loader.GetPromptAsync(ChatPersona.Artist);

        // Assert
        Assert.Equal("gpt-4o", original.MetadataOverrides?.ModelId);
        Assert.Equal(0.5, original.MetadataOverrides?.Temperature);
        Assert.Equal("gpt-4-turbo", updated.MetadataOverrides?.ModelId);
        Assert.Equal(0.9, updated.MetadataOverrides?.Temperature);
    }

    [Fact]
    public async Task HotReload_Disabled_DoesNotReloadPrompt()
    {
        // Arrange
        var fileName = "artspark.curator.prompt.md";
        var filePath = Path.Combine(_tempDirectory, fileName);
        var originalContent = @"---
model: gpt-4o
---

## CULTURAL SENSITIVITY
Curator content

## CONVERSATION GUIDELINES
Curator guidelines";

        var updatedContent = @"---
model: gpt-4-turbo
---

## CULTURAL SENSITIVITY
Updated curator content

## CONVERSATION GUIDELINES
Updated curator guidelines";

        File.WriteAllText(filePath, originalContent);

        var options = Options.Create(new PromptOptions
        {
            DataPath = _tempDirectory,
            EnableHotReload = false, // Disabled
            FallbackToDefault = true
        });

        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act 1: Load original
        var original = await loader.GetPromptAsync(ChatPersona.Curator);

        // Simulate file change
        await Task.Delay(100);
        File.WriteAllText(filePath, updatedContent);
        await Task.Delay(500);

        // Act 2: Load after change
        var cached = await loader.GetPromptAsync(ChatPersona.Curator);

        // Assert - should still have original content (cached)
        Assert.Contains("Curator content", cached.Content);
        Assert.DoesNotContain("Updated curator content", cached.Content);
        Assert.Equal(original.ContentHash, cached.ContentHash);
    }

    [Fact]
    public async Task HotReload_FileDeleted_FallsBackToDefault()
    {
        // Arrange
        var fileName = "artspark.historian.prompt.md";
        var filePath = Path.Combine(_tempDirectory, fileName);
        var originalContent = @"---
model: gpt-4o
---

## CULTURAL SENSITIVITY
Historian content

## CONVERSATION GUIDELINES
Historian guidelines";

        File.WriteAllText(filePath, originalContent);

        var options = Options.Create(new PromptOptions
        {
            DataPath = _tempDirectory,
            EnableHotReload = true,
            FallbackToDefault = true
        });

        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act 1: Load original
        var original = await loader.GetPromptAsync(ChatPersona.Historian);
        Assert.True(original.IsValid);

        // Delete file
        File.Delete(filePath);
        await Task.Delay(500);

        // Act 2: Load after deletion
        var fallback = await loader.GetPromptAsync(ChatPersona.Historian);

        // Assert - should use fallback
        Assert.True(fallback.IsValid);
        Assert.NotEqual(original.Content, fallback.Content);
    }

    [Fact]
    public async Task HotReload_LogsConfigurationReloaded()
    {
        // Arrange
        var fileName = "artspark.artwork.prompt.md";
        var filePath = Path.Combine(_tempDirectory, fileName);
        var originalContent = @"---
model: gpt-4o
---

## CULTURAL SENSITIVITY
Test content

## CONVERSATION GUIDELINES
Test guidelines";

        File.WriteAllText(filePath, originalContent);

        var options = Options.Create(new PromptOptions
        {
            DataPath = _tempDirectory,
            EnableHotReload = true,
            FallbackToDefault = true
        });

        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act 1: Load original
        await loader.GetPromptAsync(ChatPersona.Artwork);

        // Simulate file change
        await Task.Delay(100);
        File.WriteAllText(filePath, originalContent + "\n\n<!-- Updated -->");
        await Task.Delay(500);

        // Act 2: Trigger reload by loading again
        await loader.GetPromptAsync(ChatPersona.Artwork);

        // Assert - verify ConfigurationReloaded log event
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ConfigurationReloaded")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
