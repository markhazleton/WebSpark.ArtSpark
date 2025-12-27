using WebSpark.ArtSpark.Agent.Configuration;
using WebSpark.ArtSpark.Agent.Personas;

namespace WebSpark.ArtSpark.Tests.Agent.Services;

public class PromptTemplateTokenTests
{
    [Theory]
    [InlineData("artwork.Title", true)]
    [InlineData("artwork.ArtistDisplay", true)]
    [InlineData("artwork.DateDisplay", true)]
    [InlineData("artwork.InvalidProperty", false)]
    [InlineData("user.Name", false)]
    [InlineData("system.Prompt", false)]
    public void TokenWhitelist_Artwork_ValidatesCorrectly(string token, bool shouldBeValid)
    {
        // Arrange
        var config = PersonaPromptConfiguration.CreateDefaultMap()[ChatPersona.Artwork];

        // Act
        var isValid = config.AllowedTokens.Contains(token);

        // Assert
        Assert.Equal(shouldBeValid, isValid);
    }

    [Fact]
    public void TokenWhitelist_AllPersonas_ContainOnlyArtworkTokens()
    {
        // Arrange
        var configs = PersonaPromptConfiguration.CreateDefaultMap();

        // Assert
        foreach (var config in configs.Values)
        {
            Assert.All(config.AllowedTokens, token =>
            {
                Assert.StartsWith("artwork.", token);
            });
        }
    }

    [Fact]
    public void TokenWhitelist_ArtworkPersona_ContainsExpectedTokens()
    {
        // Arrange
        var config = PersonaPromptConfiguration.CreateDefaultMap()[ChatPersona.Artwork];

        // Assert
        Assert.Contains("artwork.Title", config.AllowedTokens);
        Assert.Contains("artwork.ArtistDisplay", config.AllowedTokens);
        Assert.Contains("artwork.DateDisplay", config.AllowedTokens);
        Assert.Contains("artwork.Medium", config.AllowedTokens);
        Assert.Contains("artwork.Dimensions", config.AllowedTokens);
    }

    [Fact]
    public void TokenWhitelist_ArtistPersona_ContainsExpectedTokens()
    {
        // Arrange
        var config = PersonaPromptConfiguration.CreateDefaultMap()[ChatPersona.Artist];

        // Assert
        Assert.Contains("artwork.ArtistDisplay", config.AllowedTokens);
        Assert.Contains("artwork.Title", config.AllowedTokens);
        Assert.Contains("artwork.DateDisplay", config.AllowedTokens);
    }

    [Fact]
    public void TokenWhitelist_CuratorPersona_ContainsExpectedTokens()
    {
        // Arrange
        var config = PersonaPromptConfiguration.CreateDefaultMap()[ChatPersona.Curator];

        // Assert
        Assert.Contains("artwork.Title", config.AllowedTokens);
        Assert.Contains("artwork.ArtistDisplay", config.AllowedTokens);
        Assert.Contains("artwork.CulturalContext", config.AllowedTokens);
    }

    [Fact]
    public void TokenWhitelist_HistorianPersona_ContainsExpectedTokens()
    {
        // Arrange
        var config = PersonaPromptConfiguration.CreateDefaultMap()[ChatPersona.Historian];

        // Assert
        Assert.Contains("artwork.Title", config.AllowedTokens);
        Assert.Contains("artwork.PlaceOfOrigin", config.AllowedTokens);
        Assert.Contains("artwork.Classification", config.AllowedTokens);
    }

    [Fact]
    public void TokenWhitelist_NoPersona_AllowsArbitraryProperties()
    {
        // This test ensures we're validating strictly against whitelists
        // and not allowing arbitrary property access

        // Arrange
        var allConfigs = PersonaPromptConfiguration.CreateDefaultMap();
        var dangerousTokens = new[] { "artwork.Password", "artwork.Secret", "user.Email" };

        // Assert
        foreach (var config in allConfigs.Values)
        {
            foreach (var dangerousToken in dangerousTokens)
            {
                Assert.DoesNotContain(dangerousToken, config.AllowedTokens);
            }
        }
    }
}
