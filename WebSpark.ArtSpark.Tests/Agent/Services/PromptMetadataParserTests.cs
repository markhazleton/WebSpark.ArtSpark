using WebSpark.ArtSpark.Agent.Services;

namespace WebSpark.ArtSpark.Tests.Agent.Services;

public class PromptMetadataParserTests
{
    private readonly PromptMetadataParser _parser;

    public PromptMetadataParserTests()
    {
        _parser = new PromptMetadataParser();
    }

    [Fact]
    public void Parse_ValidYamlFrontMatter_ExtractsMetadata()
    {
        // Arrange
        var markdown = @"---
model: gpt-4-turbo
temperature: 0.5
top_p: 0.9
max_output_tokens: 2000
frequency_penalty: 0.1
presence_penalty: 0.2
---

# Content Body
Test content";

        // Act
        var (metadata, body) = _parser.Parse(markdown);

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal("gpt-4-turbo", metadata.ModelId);
        Assert.Equal(0.5, metadata.Temperature);
        Assert.Equal(0.9, metadata.TopP);
        Assert.Equal(2000, metadata.MaxOutputTokens);
        Assert.Equal(0.1, metadata.FrequencyPenalty);
        Assert.Equal(0.2, metadata.PresencePenalty);
        Assert.Contains("# Content Body", body);
        Assert.DoesNotContain("---", body);
    }

    [Fact]
    public void Parse_NoFrontMatter_ReturnsNullMetadata()
    {
        // Arrange
        var markdown = @"# Content Body
Test content without front matter";

        // Act
        var (metadata, body) = _parser.Parse(markdown);

        // Assert
        Assert.Null(metadata);
        Assert.Equal(markdown, body);
    }

    [Fact]
    public void Parse_PartialMetadata_ParsesAvailableFields()
    {
        // Arrange
        var markdown = @"---
model: gpt-4o
temperature: 0.7
---

# Content";

        // Act
        var (metadata, body) = _parser.Parse(markdown);

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal("gpt-4o", metadata.ModelId);
        Assert.Equal(0.7, metadata.Temperature);
        Assert.Null(metadata.TopP);
        Assert.Null(metadata.MaxOutputTokens);
    }

    [Fact]
    public void Parse_AlternativeKeyFormats_ParsesCorrectly()
    {
        // Arrange - Test underscore and camelCase variants
        var markdown = @"---
modelId: gpt-4o
topP: 0.95
maxOutputTokens: 1500
frequencyPenalty: -0.5
presencePenalty: 0.3
---

# Content";

        // Act
        var (metadata, body) = _parser.Parse(markdown);

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal("gpt-4o", metadata.ModelId);
        Assert.Equal(0.95, metadata.TopP);
        Assert.Equal(1500, metadata.MaxOutputTokens);
        Assert.Equal(-0.5, metadata.FrequencyPenalty);
        Assert.Equal(0.3, metadata.PresencePenalty);
    }

    [Fact]
    public void Parse_InvalidNumericValues_IgnoresInvalidFields()
    {
        // Arrange
        var markdown = @"---
model: gpt-4o
temperature: invalid
top_p: 0.9
---

# Content";

        // Act
        var (metadata, body) = _parser.Parse(markdown);

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal("gpt-4o", metadata.ModelId);
        Assert.Equal(0.9, metadata.TopP);
        // Temperature should remain at default since invalid value was ignored
    }

    [Fact]
    public void Parse_EmptyFrontMatter_ReturnsDefaultMetadata()
    {
        // Arrange
        var markdown = @"---
---

# Content";

        // Act
        var (metadata, body) = _parser.Parse(markdown);

        // Assert
        Assert.NotNull(metadata);
        Assert.Contains("# Content", body);
    }

    [Fact]
    public void Parse_MultilineContent_PreservesFormatting()
    {
        // Arrange
        var markdown = @"---
model: gpt-4o
---

# Header

Paragraph 1

Paragraph 2

## Subheader";

        // Act
        var (metadata, body) = _parser.Parse(markdown);

        // Assert
        Assert.Contains("# Header", body);
        Assert.Contains("Paragraph 1", body);
        Assert.Contains("Paragraph 2", body);
        Assert.Contains("## Subheader", body);
    }
}
