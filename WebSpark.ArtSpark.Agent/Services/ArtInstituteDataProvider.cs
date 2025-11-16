using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using WebSpark.ArtSpark.Agent.Configuration;
using WebSpark.ArtSpark.Agent.Interfaces;
using WebSpark.ArtSpark.Agent.Models;
using WebSpark.HttpClientUtility.RequestResult;
namespace WebSpark.ArtSpark.Agent.Services;

public class ArtInstituteDataProvider : IArtworkDataProvider
{
    private readonly IHttpRequestResultService _service;
    private readonly ILogger<ArtInstituteDataProvider> _logger;
    private readonly AgentConfiguration _config;
    private readonly JsonSerializerOptions _jsonOptions;

    public ArtInstituteDataProvider(
        IHttpRequestResultService service,
        ILogger<ArtInstituteDataProvider> logger,
        IOptions<AgentConfiguration> config)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger;
        _config = config.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };
    }
    public async Task<ArtworkData?> GetArtworkAsync(int artworkId, CancellationToken cancellationToken = default)
    {
        try
        {
            var fieldsParam = string.Join(",", _config.ArtInstitute.RequiredFields);
            var url = $"{_config.ArtInstitute.BaseUrl}/artworks/{artworkId}?fields={fieldsParam}";

            _logger.LogDebug("Fetching artwork {ArtworkId} from {Url}", artworkId, url);

            var request = new HttpRequestResult<JsonElement>
            {
                RequestPath = url,
                CacheDurationMinutes = 60,
                Retries = 1
            }; request = await _service.HttpSendRequestResultAsync(request, ct: cancellationToken);

            if (!request.ResponseResults.Equals(default(JsonElement)))
            {
                var apiResponse = request.ResponseResults;

                if (!apiResponse.TryGetProperty("data", out var data))
                {
                    _logger.LogWarning("No data found in API response for artwork {ArtworkId}", artworkId);
                    return null;
                }

                var artwork = MapToArtworkData(data, artworkId);
                _logger.LogDebug("Successfully mapped artwork {ArtworkId}: {Title}", artworkId, artwork.Title);

                return artwork;
            }
            else
            {
                _logger.LogWarning("Failed to fetch artwork {ArtworkId}: No response data", artworkId);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching artwork {ArtworkId}", artworkId);
            return null;
        }
    }
    public async Task<ArtworkCollection> SearchArtworksAsync(
        string query,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fieldsParam = string.Join(",", _config.ArtInstitute.RequiredFields);
            var encodedQuery = Uri.EscapeDataString(query);
            var url = $"{_config.ArtInstitute.BaseUrl}/artworks/search?q={encodedQuery}&limit={Math.Min(limit, _config.ArtInstitute.MaxSearchResults)}&fields={fieldsParam}";

            _logger.LogDebug("Searching artworks with query '{Query}' from {Url}", query, url);

            var request = new HttpRequestResult<JsonElement>
            {
                RequestPath = url,
                CacheDurationMinutes = 30,
                Retries = 1
            };

            request = await _service.HttpSendRequestResultAsync(request, ct: cancellationToken);

            var collection = new ArtworkCollection
            {
                Name = $"Search Results: {query}",
                Description = $"Found artworks matching '{query}'"
            };

            if (!request.ResponseResults.Equals(default(JsonElement)))
            {
                var apiResponse = request.ResponseResults;

                if (apiResponse.TryGetProperty("data", out var dataArray))
                {
                    foreach (var item in dataArray.EnumerateArray())
                    {
                        if (item.TryGetProperty("id", out var idElement) && idElement.TryGetInt32(out var id))
                        {
                            var artwork = MapToArtworkData(item, id);
                            collection.Artworks.Add(artwork);
                        }
                    }
                }
            }
            else
            {
                _logger.LogWarning("Search failed for query '{Query}': No response data", query);
            }

            _logger.LogInformation("Found {Count} artworks for query '{Query}'", collection.Artworks.Count, query);
            return collection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching artworks with query '{Query}'", query);
            return new ArtworkCollection { Name = $"Search: {query}" };
        }
    }

    public async Task<List<ArtworkData>> GetFeaturedArtworksAsync(
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get featured artworks - using a curated list of well-known pieces
            var featuredIds = new[] { 111628, 82454, 81558, 109275, 155969, 146973, 90714, 79307, 30069, 137125 };
            var artworks = new List<ArtworkData>();

            var tasks = featuredIds.Take(count).Select(async id =>
            {
                var artwork = await GetArtworkAsync(id, cancellationToken);
                return artwork;
            });

            var results = await Task.WhenAll(tasks);
            artworks.AddRange(results.Where(a => a != null)!);

            _logger.LogInformation("Retrieved {Count} featured artworks", artworks.Count);
            return artworks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured artworks");
            return new List<ArtworkData>();
        }
    }
    public async Task<ArtworkData> EnrichArtworkDataAsync(
        ArtworkData artwork,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Add additional data enrichment if needed
            // For now, just ensure image URLs are properly formatted
            if (!string.IsNullOrEmpty(artwork.ImageUrl))
            {
                artwork.ThumbnailUrl = artwork.ImageUrl.Replace("/full/843,/", "/full/400,/");
            }

            // Generate tags based on artwork properties
            artwork.Tags = GenerateArtworkTags(artwork);

            _logger.LogDebug("Enriched artwork {ArtworkId} with {TagCount} tags", artwork.Id, artwork.Tags.Count);
            return await Task.FromResult(artwork);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enriching artwork {ArtworkId}", artwork.Id);
            return artwork;
        }
    }

    private ArtworkData MapToArtworkData(JsonElement data, int artworkId)
    {
        var artwork = new ArtworkData
        {
            Id = artworkId,
            Title = GetJsonStringProperty(data, "title"),
            ArtistDisplay = GetJsonStringProperty(data, "artist_display"),
            DateDisplay = GetJsonStringProperty(data, "date_display"),
            PlaceOfOrigin = GetJsonStringProperty(data, "place_of_origin"),
            Medium = GetJsonStringProperty(data, "medium_display"),
            Dimensions = GetJsonStringProperty(data, "dimensions"),
            Description = GetJsonStringProperty(data, "description"),
            CulturalContext = GetJsonStringProperty(data, "style_title"),
            StyleTitle = GetJsonStringProperty(data, "style_title"),
            Classification = GetJsonStringProperty(data, "classification_title"),
            FullApiData = JsonSerializer.Deserialize<Dictionary<string, object>>(data.GetRawText()) ?? new()
        };

        // Build image URLs
        if (data.TryGetProperty("image_id", out var imageId) && !string.IsNullOrEmpty(imageId.GetString()))
        {
            var imageIdStr = imageId.GetString();
            artwork.ImageUrl = $"{_config.ArtInstitute.ImageBaseUrl}/{imageIdStr}/full/{_config.ArtInstitute.DefaultImageSize}/0/default.jpg";
            artwork.ThumbnailUrl = $"{_config.ArtInstitute.ImageBaseUrl}/{imageIdStr}/full/400,/0/default.jpg";
        }

        return artwork;
    }

    private string GetJsonStringProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind != JsonValueKind.Null
            ? prop.GetString() ?? string.Empty : string.Empty;
    }

    private List<string> GenerateArtworkTags(ArtworkData artwork)
    {
        var tags = new List<string>();

        // Add culture/origin tags
        if (!string.IsNullOrEmpty(artwork.PlaceOfOrigin))
        {
            tags.Add(artwork.PlaceOfOrigin);
        }

        // Add classification tags
        if (!string.IsNullOrEmpty(artwork.Classification))
        {
            tags.Add(artwork.Classification);
        }

        // Add style tags
        if (!string.IsNullOrEmpty(artwork.StyleTitle))
        {
            tags.Add(artwork.StyleTitle);
        }

        // Add medium tags
        if (!string.IsNullOrEmpty(artwork.Medium))
        {
            var mediumTags = artwork.Medium.Split(',', ';')
                .Select(m => m.Trim())
                .Where(m => !string.IsNullOrEmpty(m))
                .Take(3);
            tags.AddRange(mediumTags);
        }

        // Add temporal tags
        if (!string.IsNullOrEmpty(artwork.DateDisplay))
        {
            if (artwork.DateDisplay.Contains("century"))
            {
                tags.Add(artwork.DateDisplay);
            }
        }

        return tags.Distinct().ToList();
    }
}
