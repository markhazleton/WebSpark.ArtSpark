using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebSpark.ArtSpark.Agent.Configuration;
using WebSpark.ArtSpark.Agent.Models;
using WebSpark.ArtSpark.Agent.Personas;
using WebSpark.ArtSpark.Agent.Services;

namespace WebSpark.ArtSpark.Tests.Agent.Personas;

public class PersonaFactoryTests
{
    private readonly Mock<IPromptLoader> _mockPromptLoader;
    private readonly Mock<ILogger<FileBackedPersonaHandler>> _mockLogger;
    private readonly IOptions<PromptOptions> _options;
    private readonly PersonaFactory _factory;

    public PersonaFactoryTests()
    {
        _mockPromptLoader = new Mock<IPromptLoader>();
        _mockLogger = new Mock<ILogger<FileBackedPersonaHandler>>();
        _options = Options.Create(new PromptOptions
        {
            DataPath = "./prompts/agents",
            FallbackToDefault = true
        });

        _factory = new PersonaFactory(_mockPromptLoader.Object, _options, _mockLogger.Object);
    }

    [Theory]
    [InlineData(ChatPersona.Artwork)]
    [InlineData(ChatPersona.Artist)]
    [InlineData(ChatPersona.Curator)]
    [InlineData(ChatPersona.Historian)]
    public void CreatePersona_AllPersonaTypes_ReturnsFileBackedHandler(ChatPersona personaType)
    {
        // Arrange
        var artwork = new ArtworkData { Title = "Test Artwork" };

        // Act
        var handler = _factory.CreatePersona(personaType, artwork);

        // Assert
        Assert.NotNull(handler);
        Assert.IsType<FileBackedPersonaHandler>(handler);
        Assert.Equal(personaType, handler.PersonaType);
    }

    [Fact]
    public void CreatePersona_UnknownPersona_ThrowsArgumentException()
    {
        // Arrange
        var artwork = new ArtworkData { Title = "Test" };
        var invalidPersona = (ChatPersona)999;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _factory.CreatePersona(invalidPersona, artwork));
    }

    [Fact]
    public void CreatePersona_WrapsBasePersonaHandler()
    {
        // Arrange
        var artwork = new ArtworkData { Title = "Test Artwork" };

        // Act
        var handler = _factory.CreatePersona(ChatPersona.Artwork, artwork);

        // Assert - FileBackedPersonaHandler should wrap the base handler
        Assert.IsType<FileBackedPersonaHandler>(handler);
    }

    [Fact]
    public async Task CreatePersona_GeneratesSystemPrompt_UsesPromptLoader()
    {
        // Arrange
        var artwork = new ArtworkData
        {
            Title = "Test Artwork",
            ArtistDisplay = "Test Artist"
        };

        var mockTemplate = new PromptTemplate
        {
            PersonaType = ChatPersona.Artwork,
            FilePath = "./test.md",
            FileName = "test.md",
            Content = "Test prompt with {artwork.Title}",
            ContentHash = "abc123",
            LastModifiedUtc = DateTime.UtcNow,
            FileSizeBytes = 100,
            IsFallback = false
        };

        _mockPromptLoader
            .Setup(x => x.GetPromptAsync(ChatPersona.Artwork, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTemplate);

        // Act
        var handler = _factory.CreatePersona(ChatPersona.Artwork, artwork);
        var prompt = handler.GenerateSystemPrompt(artwork);

        // Assert
        Assert.Contains("Test Artwork", prompt);
        _mockPromptLoader.Verify(
            x => x.GetPromptAsync(ChatPersona.Artwork, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreatePersona_FallbackTemplate_UsesFallbackPrompt()
    {
        // Arrange
        var artwork = new ArtworkData { Title = "Test" };

        var fallbackTemplate = new PromptTemplate
        {
            PersonaType = ChatPersona.Artwork,
            FilePath = "[fallback]",
            FileName = "[fallback]",
            Content = ArtworkPersona.DefaultSystemPrompt,
            ContentHash = "fallback",
            LastModifiedUtc = DateTime.UtcNow,
            FileSizeBytes = 100,
            IsFallback = true
        };

        _mockPromptLoader
            .Setup(x => x.GetPromptAsync(ChatPersona.Artwork, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fallbackTemplate);

        // Act
        var handler = _factory.CreatePersona(ChatPersona.Artwork, artwork);
        var prompt = handler.GenerateSystemPrompt(artwork);

        // Assert - Should use base handler's prompt when fallback
        Assert.NotEmpty(prompt);
    }
}
