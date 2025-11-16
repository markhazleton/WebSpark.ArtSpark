using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebSpark.ArtSpark.Agent.Models;
using WebSpark.ArtSpark.Demo.Controllers.Api;
using WebSpark.ArtSpark.Demo.Models;
using WebSpark.ArtSpark.Demo.Models.Api;
using WebSpark.ArtSpark.Demo.Services;

namespace WebSpark.ArtSpark.Tests.Controllers;

/// <summary>
/// Unit tests for SeoOptimizationController
/// </summary>
public class SeoOptimizationControllerTests
{
    private readonly Mock<ISeoOptimizationService> _mockSeoService;
    private readonly Mock<ILogger<SeoOptimizationController>> _mockLogger;
    private readonly SeoOptimizationController _controller;

    public SeoOptimizationControllerTests()
    {
        _mockSeoService = new Mock<ISeoOptimizationService>();
        _mockLogger = new Mock<ILogger<SeoOptimizationController>>();
        _controller = new SeoOptimizationController(_mockSeoService.Object, _mockLogger.Object);

        // Setup mock user context
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-123")
        }));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task OptimizeCollection_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new OptimizeCollectionRequest
        {
            Description = "Renaissance art collection featuring masterpieces from the Italian Renaissance period"
        };

        var expectedCollection = new UserCollection
        {
            Id = 1,
            Name = "Renaissance Masterpieces Collection",
            Description = "Test Description",
            Slug = "renaissance-masterpieces-collection",
            MetaTitle = "Renaissance Masterpieces | Art Collection",
            MetaDescription = "Explore our curated collection of Renaissance masterpieces",
            MetaKeywords = "renaissance art, masterpieces",
            Tags = "Renaissance, Art History",
            SocialImageUrl = "https://example.com/renaissance.jpg",
            UserId = "test-user-123"
        };

        _mockSeoService
            .Setup(x => x.OptimizeCollectionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCollection);

        // Act
        var result = await _controller.OptimizeCollection(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<OptimizeCollectionResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<OptimizeCollectionResponse>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Collection);
        Assert.Equal(expectedCollection.Name, response.Collection.Name);
        Assert.Equal(expectedCollection.Slug, response.Collection.Slug);
    }
    [Fact]
    public async Task OptimizeCollection_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new OptimizeCollectionRequest
        {
            Description = "Short" // Too short - minimum is 10 characters
        };

        // Manually validate the model and set ModelState
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

        if (!isValid)
        {
            foreach (var validationResult in validationResults)
            {
                foreach (var memberName in validationResult.MemberNames)
                {
                    _controller.ModelState.AddModelError(memberName, validationResult.ErrorMessage ?? "Invalid value");
                }
            }
        }

        // Act
        var result = await _controller.OptimizeCollection(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<OptimizeCollectionResponse>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var response = Assert.IsType<OptimizeCollectionResponse>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task OptimizeArtwork_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new OptimizeArtworkRequest
        {
            ArtworkData = new ArtworkData
            {
                Id = 123,
                Title = "The Starry Night",
                ArtistDisplay = "Vincent van Gogh",
                DateDisplay = "1889",
                Medium = "Oil on canvas",
                Description = "A masterpiece depicting swirling night skies"
            },
            CollectionId = 456
        };

        var expectedArtwork = new CollectionArtwork
        {
            Id = 1,
            CollectionId = 456,
            ArtworkId = 123,
            CustomTitle = "The Starry Night - Van Gogh's Masterpiece",
            CustomDescription = "Experience Vincent van Gogh's iconic masterpiece",
            Slug = "starry-night-van-gogh-masterpiece",
            MetaTitle = "The Starry Night by Vincent van Gogh | Art Collection",
            MetaDescription = "Discover Van Gogh's The Starry Night (1889)",
            CuratorNotes = "This post-impressionist masterpiece showcases Van Gogh's unique style"
        };

        _mockSeoService
            .Setup(x => x.OptimizeCollectionArtworkAsync(It.IsAny<ArtworkData>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedArtwork);

        // Act
        var result = await _controller.OptimizeArtwork(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<OptimizeArtworkResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<OptimizeArtworkResponse>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.CollectionArtwork);
        Assert.Equal(expectedArtwork.CustomTitle, response.CollectionArtwork.CustomTitle);
        Assert.Equal(expectedArtwork.Slug, response.CollectionArtwork.Slug);
    }
    [Fact]
    public async Task OptimizeArtwork_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new OptimizeArtworkRequest
        {
            ArtworkData = null!, // Null artwork data should fail validation
            CollectionId = 123
        };

        // Manually validate the model and set ModelState
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

        if (!isValid)
        {
            foreach (var validationResult in validationResults)
            {
                foreach (var memberName in validationResult.MemberNames)
                {
                    _controller.ModelState.AddModelError(memberName, validationResult.ErrorMessage ?? "Invalid value");
                }
            }
        }

        // Act
        var result = await _controller.OptimizeArtwork(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<OptimizeArtworkResponse>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var response = Assert.IsType<OptimizeArtworkResponse>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task OptimizeCollection_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new OptimizeCollectionRequest
        {
            Description = "Valid test collection description with enough characters"
        };

        _mockSeoService
            .Setup(x => x.OptimizeCollectionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.OptimizeCollection(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<OptimizeCollectionResponse>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusCodeResult.StatusCode); var response = Assert.IsType<OptimizeCollectionResponse>(statusCodeResult.Value);
        Assert.False(response.Success);
        Assert.Contains("An error occurred while optimizing the collection", response.ErrorMessage);
    }

    [Fact]
    public async Task OptimizeArtwork_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new OptimizeArtworkRequest
        {
            ArtworkData = new ArtworkData
            {
                Id = 123,
                Title = "Test Artwork",
                ArtistDisplay = "Test Artist"
            },
            CollectionId = 456
        };

        _mockSeoService
            .Setup(x => x.OptimizeCollectionArtworkAsync(It.IsAny<ArtworkData>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.OptimizeArtwork(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<OptimizeArtworkResponse>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusCodeResult.StatusCode); var response = Assert.IsType<OptimizeArtworkResponse>(statusCodeResult.Value);
        Assert.False(response.Success);
        Assert.Contains("An error occurred while optimizing the artwork", response.ErrorMessage);
    }
}
