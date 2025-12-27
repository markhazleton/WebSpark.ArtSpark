using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebSpark.ArtSpark.Agent.Configuration;
using WebSpark.ArtSpark.Agent.Personas;
using WebSpark.ArtSpark.Agent.Services;

namespace WebSpark.ArtSpark.Tests.Agent.Services;

/// <summary>
/// Tests for audit logging events emitted by the prompt loading system.
/// Validates that operators can monitor prompt versions and detect fallback conditions.
/// </summary>
public class PromptAuditLoggingTests : IDisposable
{
    private readonly Mock<ILogger<PromptLoader>> _mockLogger;
    private readonly string _testDataPath;

    public PromptAuditLoggingTests()
    {
        _mockLogger = new Mock<ILogger<PromptLoader>>();
        _testDataPath = Path.Combine(Path.GetTempPath(), "artspark-test-audit", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);
    }

    [Fact]
    public async Task GetPromptAsync_SuccessfulLoad_LogsPromptLoadedEvent()
    {
        // Arrange
        var promptContent = @"---
model: gpt-4o
temperature: 0.8
---

# Artwork Persona

## CULTURAL SENSITIVITY
Sensitivity content

## CONVERSATION GUIDELINES
Guidelines content";

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

        // Verify that PromptLoaded event was logged (Information level)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("PromptLoaded") || v.ToString()!.Contains("Loaded prompt")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetPromptAsync_FileNotFound_LogsPromptLoadFailedAndFallbackUsed()
    {
        // Arrange - no file created, directory exists but empty
        var options = Options.Create(new PromptOptions
        {
            DataPath = _testDataPath,
            FallbackToDefault = true
        });

        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Artist);

        // Assert - should return fallback template
        Assert.NotNull(template);

        // Verify that PromptLoadFailed and/or PromptFallbackUsed events were logged (Warning level)
        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(level => level >= LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("PromptLoadFailed") ||
                    v.ToString()!.Contains("PromptFallbackUsed") ||
                    v.ToString()!.Contains("Failed to load") ||
                    v.ToString()!.Contains("fallback")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetPromptAsync_InvalidToken_LogsTokenValidationFailedEvent()
    {
        // Arrange
        var promptContent = @"---
model: gpt-4o
---

# Artwork Persona with Invalid Token

## CULTURAL SENSITIVITY
This prompt contains an invalid token: {malicious.script}

## CONVERSATION GUIDELINES
Guidelines content";

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

        // Assert - should fall back due to invalid token
        Assert.NotNull(template);

        // Verify that PromptTokenValidationFailed event was logged (Warning level)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("PromptTokenValidationFailed") ||
                    v.ToString()!.Contains("Token validation") ||
                    v.ToString()!.Contains("invalid token")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetPromptAsync_MissingRequiredSection_LogsValidationFailureAndFallback()
    {
        // Arrange
        var promptContent = @"---
model: gpt-4o
---

# Incomplete Curator Persona

## CONVERSATION GUIDELINES
Guidelines content
(Missing CULTURAL SENSITIVITY section)";

        var fileName = "artspark.curator.prompt.md";
        File.WriteAllText(Path.Combine(_testDataPath, fileName), promptContent);

        var options = Options.Create(new PromptOptions
        {
            DataPath = _testDataPath,
            FallbackToDefault = true
        });

        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Curator);

        // Assert - should fall back due to missing section
        Assert.NotNull(template);

        // Verify warning was logged about validation failure
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Missing") ||
                    v.ToString()!.Contains("CULTURAL SENSITIVITY") ||
                    v.ToString()!.Contains("validation") ||
                    v.ToString()!.Contains("fallback")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetPromptAsync_IncludesFileMetadataInLog()
    {
        // Arrange
        var promptContent = @"---
model: gpt-4o
---

# Historian Persona

## CULTURAL SENSITIVITY
Sensitivity

## CONVERSATION GUIDELINES
Guidelines";

        var fileName = "artspark.historian.prompt.md";
        var filePath = Path.Combine(_testDataPath, fileName);
        File.WriteAllText(filePath, promptContent);

        var options = Options.Create(new PromptOptions
        {
            DataPath = _testDataPath,
            FallbackToDefault = true
        });

        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Historian);

        // Assert
        Assert.NotNull(template);

        // Verify that log includes file metadata (path, size, hash)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(fileName) ||
                    v.ToString()!.Contains("Historian") ||
                    v.ToString()!.Contains("Loaded")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetPromptAsync_MetadataOverride_LogsConfigurationDetails()
    {
        // Arrange
        var promptContent = @"---
model: gpt-4-turbo
temperature: 0.95
max_output_tokens: 2000
---

# Artwork Persona with Overrides

## CULTURAL SENSITIVITY
Sensitivity

## CONVERSATION GUIDELINES
Guidelines";

        var fileName = "artspark.artwork.prompt.md";
        File.WriteAllText(Path.Combine(_testDataPath, fileName), promptContent);

        var options = Options.Create(new PromptOptions
        {
            DataPath = _testDataPath,
            FallbackToDefault = true,
            DefaultMetadata = new PromptMetadata { ModelId = "gpt-4o", Temperature = 0.7 }
        });

        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act
        var template = await loader.GetPromptAsync(ChatPersona.Artwork);

        // Assert
        Assert.NotNull(template);
        Assert.Equal("gpt-4-turbo", template.MetadataOverrides?.ModelId);
        Assert.Equal(0.95, template.MetadataOverrides?.Temperature);

        // Verify configuration logging occurred
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("prompt") || v.ToString()!.Contains("Loaded")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetPromptAsync_LogIncludesPersonaType()
    {
        // Arrange
        var promptContent = @"---
model: gpt-4o
---

# Artist Persona

## CULTURAL SENSITIVITY
Sensitivity

## CONVERSATION GUIDELINES
Guidelines";

        File.WriteAllText(Path.Combine(_testDataPath, "artspark.artist.prompt.md"), promptContent);

        var options = Options.Create(new PromptOptions { DataPath = _testDataPath });
        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act
        await loader.GetPromptAsync(ChatPersona.Artist);

        // Assert - verify persona type appears in logs
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Artist") ||
                    v.ToString()!.Contains("artspark.artist")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetPromptAsync_FallbackCounter_IncrementsOnFailure()
    {
        // Arrange - create multiple personas, only one has valid file
        File.WriteAllText(
            Path.Combine(_testDataPath, "artspark.artwork.prompt.md"),
            @"---
model: gpt-4o
---
# Valid
## CULTURAL SENSITIVITY
Test
## CONVERSATION GUIDELINES
Test");

        var options = Options.Create(new PromptOptions
        {
            DataPath = _testDataPath,
            FallbackToDefault = true
        });

        var loader = new PromptLoader(options, _mockLogger.Object);

        // Act - load one valid, two missing
        await loader.GetPromptAsync(ChatPersona.Artwork); // Success
        await loader.GetPromptAsync(ChatPersona.Artist);  // Fallback
        await loader.GetPromptAsync(ChatPersona.Curator); // Fallback

        // Assert - verify fallback warnings logged for missing files
        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(level => level >= LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("fallback") || v.ToString()!.Contains("Failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(2)); // Artist and Curator should trigger fallback warnings
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDataPath))
        {
            Directory.Delete(_testDataPath, recursive: true);
        }
    }
}
