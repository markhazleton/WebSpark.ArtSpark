using Microsoft.SemanticKernel;
using Moq;
using WebSpark.ArtSpark.Agent.Models;
using WebSpark.ArtSpark.Demo.Services;

namespace WebSpark.ArtSpark.Tests.Services;

/// <summary>
/// Unit tests for SeoOptimizationService - Basic validation tests
/// Note: Since Kernel is sealed, we test basic validation and structure
/// Integration tests would be needed for full AI functionality testing
/// </summary>
public class SeoOptimizationServiceTests
{
    [Fact]
    public void ISeoOptimizationService_Interface_HasCorrectMethods()
    {
        // This test verifies the interface contract exists
        var interfaceType = typeof(ISeoOptimizationService);

        Assert.NotNull(interfaceType);

        var optimizeCollectionMethod = interfaceType.GetMethod("OptimizeCollectionAsync");
        Assert.NotNull(optimizeCollectionMethod);
        Assert.Equal(typeof(Task<>).MakeGenericType(typeof(WebSpark.ArtSpark.Demo.Models.UserCollection)), optimizeCollectionMethod.ReturnType);

        var optimizeArtworkMethod = interfaceType.GetMethod("OptimizeCollectionArtworkAsync");
        Assert.NotNull(optimizeArtworkMethod);
        Assert.Equal(typeof(Task<>).MakeGenericType(typeof(WebSpark.ArtSpark.Demo.Models.CollectionArtwork)), optimizeArtworkMethod.ReturnType);
    }
    [Fact]
    public void SeoOptimizationService_Constructor_ThrowsWhenLoggerIsNull()
    {
        // Arrange
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion("test-model", "test-key");
        var kernel = kernelBuilder.Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SeoOptimizationService(null!, kernel));
    }

    [Fact]
    public void SeoOptimizationService_Constructor_ThrowsWhenKernelIsNull()
    {
        // Arrange
        var logger = new Mock<ILogger<SeoOptimizationService>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SeoOptimizationService(logger.Object, null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task OptimizeCollectionAsync_InvalidDescription_ThrowsArgumentException(string? description)
    {
        // Arrange
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion("test-model", "test-key");
        var kernel = kernelBuilder.Build();
        var logger = new Mock<ILogger<SeoOptimizationService>>();
        var service = new SeoOptimizationService(logger.Object, kernel);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.OptimizeCollectionAsync(description!, "test-user-id"));
    }

    [Fact]
    public async Task OptimizeCollectionArtworkAsync_NullArtworkData_ThrowsArgumentNullException()
    {
        // Arrange
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion("test-model", "test-key");
        var kernel = kernelBuilder.Build();
        var logger = new Mock<ILogger<SeoOptimizationService>>();
        var service = new SeoOptimizationService(logger.Object, kernel);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.OptimizeCollectionArtworkAsync(null!, 123));
    }

    [Fact]
    public void ArtworkData_Model_HasRequiredProperties()
    {
        // Test the ArtworkData model structure
        var artwork = new ArtworkData
        {
            Id = 123,
            Title = "Test Title",
            ArtistDisplay = "Test Artist",
            DateDisplay = "1800",
            Medium = "Oil on canvas",
            Dimensions = "24x36 inches",
            PlaceOfOrigin = "France",
            Description = "Test description"
        };

        Assert.Equal(123, artwork.Id);
        Assert.Equal("Test Title", artwork.Title);
        Assert.Equal("Test Artist", artwork.ArtistDisplay);
        Assert.Equal("1800", artwork.DateDisplay);
        Assert.Equal("Oil on canvas", artwork.Medium);
        Assert.Equal("24x36 inches", artwork.Dimensions);
        Assert.Equal("France", artwork.PlaceOfOrigin);
        Assert.Equal("Test description", artwork.Description);
    }
}
