using Microsoft.AspNetCore.Mvc;
using WebSpark.ArtSpark.Client.Interfaces;
using WebSpark.ArtSpark.Client.Models.Common;
using WebSpark.ArtSpark.Client.Models.Website;

namespace WebSpark.ArtSpark.Demo.Controllers.Api;

/// <summary>
/// API controller for content-related operations (articles, publications, educator resources, highlights, etc.)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ContentApiController : ControllerBase
{
    private readonly IArtInstituteClient _artInstituteClient;
    private readonly ILogger<ContentApiController> _logger;

    public ContentApiController(
        IArtInstituteClient artInstituteClient,
        ILogger<ContentApiController> logger)
    {
        _artInstituteClient = artInstituteClient ?? throw new ArgumentNullException(nameof(artInstituteClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get a specific article by ID
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Article details</returns>
    [HttpGet("articles/{id:int}")]
    [ProducesResponseType(typeof(Article), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Article>> GetArticle(
        int id,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var article = await _artInstituteClient.GetArticleAsync(id, fields, cancellationToken);

            if (article == null)
            {
                _logger.LogWarning("Article with ID {ArticleId} not found", id);
                return NotFound($"Article with ID {id} not found");
            }

            return Ok(article);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving article with ID {ArticleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the article");
        }
    }

    /// <summary>
    /// Get all articles with optional query parameters
    /// </summary>
    /// <param name="limit">Number of results to return</param>
    /// <param name="page">Page number</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of articles</returns>
    [HttpGet("articles")]
    [ProducesResponseType(typeof(ApiResponse<Article>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<Article>>> GetArticles(
        [FromQuery] int? limit = null,
        [FromQuery] int? page = null,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new ApiQuery
            {
                Limit = limit,
                Page = page,
                Fields = fields != null ? string.Join(",", fields) : null
            };

            var result = await _artInstituteClient.GetArticlesAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving articles");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving articles");
        }
    }

    /// <summary>
    /// Search articles using advanced query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results for articles</returns>
    [HttpPost("articles/search")]
    [ProducesResponseType(typeof(SearchResponse<Article>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchResponse<Article>>> SearchArticles(
        [FromBody] SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (query == null)
            {
                return BadRequest("Search query is required");
            }

            var result = await _artInstituteClient.SearchArticlesAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching articles with advanced query");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching articles");
        }
    }

    /// <summary>
    /// Get a specific digital publication by ID
    /// </summary>
    /// <param name="id">Digital publication ID</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Digital publication details</returns>
    [HttpGet("digital-publications/{id:int}")]
    [ProducesResponseType(typeof(DigitalPublication), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DigitalPublication>> GetDigitalPublication(
        int id,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var publication = await _artInstituteClient.GetDigitalPublicationAsync(id, fields, cancellationToken);

            if (publication == null)
            {
                _logger.LogWarning("Digital publication with ID {PublicationId} not found", id);
                return NotFound($"Digital publication with ID {id} not found");
            }

            return Ok(publication);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving digital publication with ID {PublicationId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the digital publication");
        }
    }

    /// <summary>
    /// Get all digital publications with optional query parameters
    /// </summary>
    /// <param name="limit">Number of results to return</param>
    /// <param name="page">Page number</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of digital publications</returns>
    [HttpGet("digital-publications")]
    [ProducesResponseType(typeof(ApiResponse<DigitalPublication>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DigitalPublication>>> GetDigitalPublications(
        [FromQuery] int? limit = null,
        [FromQuery] int? page = null,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new ApiQuery
            {
                Limit = limit,
                Page = page,
                Fields = fields != null ? string.Join(",", fields) : null
            };

            var result = await _artInstituteClient.GetDigitalPublicationsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving digital publications");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving digital publications");
        }
    }

    /// <summary>
    /// Search digital publications using advanced query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results for digital publications</returns>
    [HttpPost("digital-publications/search")]
    [ProducesResponseType(typeof(SearchResponse<DigitalPublication>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchResponse<DigitalPublication>>> SearchDigitalPublications(
        [FromBody] SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (query == null)
            {
                return BadRequest("Search query is required");
            }

            var result = await _artInstituteClient.SearchDigitalPublicationsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching digital publications with advanced query");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching digital publications");
        }
    }

    /// <summary>
    /// Get a specific digital publication article by ID
    /// </summary>
    /// <param name="id">Digital publication article ID</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Digital publication article details</returns>
    [HttpGet("digital-publication-articles/{id:int}")]
    [ProducesResponseType(typeof(DigitalPublicationArticle), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DigitalPublicationArticle>> GetDigitalPublicationArticle(
        int id,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var article = await _artInstituteClient.GetDigitalPublicationArticleAsync(id, fields, cancellationToken);

            if (article == null)
            {
                _logger.LogWarning("Digital publication article with ID {ArticleId} not found", id);
                return NotFound($"Digital publication article with ID {id} not found");
            }

            return Ok(article);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving digital publication article with ID {ArticleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the digital publication article");
        }
    }

    /// <summary>
    /// Get all digital publication articles with optional query parameters
    /// </summary>
    /// <param name="limit">Number of results to return</param>
    /// <param name="page">Page number</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of digital publication articles</returns>
    [HttpGet("digital-publication-articles")]
    [ProducesResponseType(typeof(ApiResponse<DigitalPublicationArticle>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DigitalPublicationArticle>>> GetDigitalPublicationArticles(
        [FromQuery] int? limit = null,
        [FromQuery] int? page = null,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new ApiQuery
            {
                Limit = limit,
                Page = page,
                Fields = fields != null ? string.Join(",", fields) : null
            };

            var result = await _artInstituteClient.GetDigitalPublicationArticlesAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving digital publication articles");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving digital publication articles");
        }
    }

    /// <summary>
    /// Get a specific educator resource by ID
    /// </summary>
    /// <param name="id">Educator resource ID</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Educator resource details</returns>
    [HttpGet("educator-resources/{id:int}")]
    [ProducesResponseType(typeof(EducatorResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EducatorResource>> GetEducatorResource(
        int id,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resource = await _artInstituteClient.GetEducatorResourceAsync(id, fields, cancellationToken);

            if (resource == null)
            {
                _logger.LogWarning("Educator resource with ID {ResourceId} not found", id);
                return NotFound($"Educator resource with ID {id} not found");
            }

            return Ok(resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving educator resource with ID {ResourceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the educator resource");
        }
    }

    /// <summary>
    /// Get all educator resources with optional query parameters
    /// </summary>
    /// <param name="limit">Number of results to return</param>
    /// <param name="page">Page number</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of educator resources</returns>
    [HttpGet("educator-resources")]
    [ProducesResponseType(typeof(ApiResponse<EducatorResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EducatorResource>>> GetEducatorResources(
        [FromQuery] int? limit = null,
        [FromQuery] int? page = null,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new ApiQuery
            {
                Limit = limit,
                Page = page,
                Fields = fields != null ? string.Join(",", fields) : null
            };

            var result = await _artInstituteClient.GetEducatorResourcesAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving educator resources");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving educator resources");
        }
    }

    /// <summary>
    /// Search educator resources using advanced query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results for educator resources</returns>
    [HttpPost("educator-resources/search")]
    [ProducesResponseType(typeof(SearchResponse<EducatorResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchResponse<EducatorResource>>> SearchEducatorResources(
        [FromBody] SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (query == null)
            {
                return BadRequest("Search query is required");
            }

            var result = await _artInstituteClient.SearchEducatorResourcesAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching educator resources with advanced query");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching educator resources");
        }
    }

    /// <summary>
    /// Get a specific highlight by ID
    /// </summary>
    /// <param name="id">Highlight ID</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Highlight details</returns>
    [HttpGet("highlights/{id:int}")]
    [ProducesResponseType(typeof(Highlight), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Highlight>> GetHighlight(
        int id,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var highlight = await _artInstituteClient.GetHighlightAsync(id, fields, cancellationToken);

            if (highlight == null)
            {
                _logger.LogWarning("Highlight with ID {HighlightId} not found", id);
                return NotFound($"Highlight with ID {id} not found");
            }

            return Ok(highlight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving highlight with ID {HighlightId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the highlight");
        }
    }

    /// <summary>
    /// Get all highlights with optional query parameters
    /// </summary>
    /// <param name="limit">Number of results to return</param>
    /// <param name="page">Page number</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of highlights</returns>
    [HttpGet("highlights")]
    [ProducesResponseType(typeof(ApiResponse<Highlight>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<Highlight>>> GetHighlights(
        [FromQuery] int? limit = null,
        [FromQuery] int? page = null,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new ApiQuery
            {
                Limit = limit,
                Page = page,
                Fields = fields != null ? string.Join(",", fields) : null
            };

            var result = await _artInstituteClient.GetHighlightsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving highlights");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving highlights");
        }
    }

    /// <summary>
    /// Search highlights using advanced query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results for highlights</returns>
    [HttpPost("highlights/search")]
    [ProducesResponseType(typeof(SearchResponse<Highlight>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchResponse<Highlight>>> SearchHighlights(
        [FromBody] SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (query == null)
            {
                return BadRequest("Search query is required");
            }

            var result = await _artInstituteClient.SearchHighlightsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching highlights with advanced query");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching highlights");
        }
    }

    /// <summary>
    /// Get a specific generic page by ID
    /// </summary>
    /// <param name="id">Generic page ID</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generic page details</returns>
    [HttpGet("generic-pages/{id:int}")]
    [ProducesResponseType(typeof(GenericPage), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GenericPage>> GetGenericPage(
        int id,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var page = await _artInstituteClient.GetGenericPageAsync(id, fields, cancellationToken);

            if (page == null)
            {
                _logger.LogWarning("Generic page with ID {PageId} not found", id);
                return NotFound($"Generic page with ID {id} not found");
            }

            return Ok(page);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving generic page with ID {PageId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the generic page");
        }
    }

    /// <summary>
    /// Get all generic pages with optional query parameters
    /// </summary>
    /// <param name="limit">Number of results to return</param>
    /// <param name="page">Page number</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of generic pages</returns>
    [HttpGet("generic-pages")]
    [ProducesResponseType(typeof(ApiResponse<GenericPage>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GenericPage>>> GetGenericPages(
        [FromQuery] int? limit = null,
        [FromQuery] int? page = null,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new ApiQuery
            {
                Limit = limit,
                Page = page,
                Fields = fields != null ? string.Join(",", fields) : null
            };

            var result = await _artInstituteClient.GetGenericPagesAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving generic pages");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving generic pages");
        }
    }

    /// <summary>
    /// Search generic pages using advanced query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results for generic pages</returns>
    [HttpPost("generic-pages/search")]
    [ProducesResponseType(typeof(SearchResponse<GenericPage>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchResponse<GenericPage>>> SearchGenericPages(
        [FromBody] SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (query == null)
            {
                return BadRequest("Search query is required");
            }

            var result = await _artInstituteClient.SearchGenericPagesAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching generic pages with advanced query");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching generic pages");
        }
    }
}
