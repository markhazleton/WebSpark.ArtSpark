using Microsoft.AspNetCore.Mvc;
using WebSpark.ArtSpark.Client.Interfaces;
using WebSpark.ArtSpark.Client.Models.Collections;
using WebSpark.ArtSpark.Client.Models.Common;

namespace WebSpark.ArtSpark.Demo.Controllers.Api;

/// <summary>
/// API controller for exhibitions and galleries
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExhibitionsApiController : ControllerBase
{
    private readonly IArtInstituteClient _artInstituteClient;
    private readonly ILogger<ExhibitionsApiController> _logger;

    public ExhibitionsApiController(
        IArtInstituteClient artInstituteClient,
        ILogger<ExhibitionsApiController> logger)
    {
        _artInstituteClient = artInstituteClient ?? throw new ArgumentNullException(nameof(artInstituteClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get a specific exhibition by ID
    /// </summary>
    /// <param name="id">Exhibition ID</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="include">Optional related data to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Exhibition details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Exhibition), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Exhibition>> GetExhibition(
        int id,
        [FromQuery] string[]? fields = null,
        [FromQuery] string[]? include = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var exhibition = await _artInstituteClient.GetExhibitionAsync(id, fields, include, cancellationToken);

            if (exhibition == null)
            {
                _logger.LogWarning("Exhibition with ID {ExhibitionId} not found", id);
                return NotFound($"Exhibition with ID {id} not found");
            }

            return Ok(exhibition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exhibition with ID {ExhibitionId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the exhibition");
        }
    }

    /// <summary>
    /// Get all exhibitions with optional query parameters
    /// </summary>
    /// <param name="limit">Number of results to return</param>
    /// <param name="page">Page number</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of exhibitions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<Exhibition>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<Exhibition>>> GetExhibitions(
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

            var result = await _artInstituteClient.GetExhibitionsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exhibitions");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving exhibitions");
        }
    }

    /// <summary>
    /// Search exhibitions using advanced query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results for exhibitions</returns>
    [HttpPost("search")]
    [ProducesResponseType(typeof(SearchResponse<Exhibition>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchResponse<Exhibition>>> SearchExhibitions(
        [FromBody] SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (query == null)
            {
                return BadRequest("Search query is required");
            }

            var result = await _artInstituteClient.SearchExhibitionsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching exhibitions with advanced query");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching exhibitions");
        }
    }

    /// <summary>
    /// Get a specific gallery by ID
    /// </summary>
    /// <param name="id">Gallery ID</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Gallery details</returns>
    [HttpGet("galleries/{id:int}")]
    [ProducesResponseType(typeof(Gallery), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Gallery>> GetGallery(
        int id,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var gallery = await _artInstituteClient.GetGalleryAsync(id, fields, cancellationToken);

            if (gallery == null)
            {
                _logger.LogWarning("Gallery with ID {GalleryId} not found", id);
                return NotFound($"Gallery with ID {id} not found");
            }

            return Ok(gallery);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving gallery with ID {GalleryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the gallery");
        }
    }

    /// <summary>
    /// Get all galleries with optional query parameters
    /// </summary>
    /// <param name="limit">Number of results to return</param>
    /// <param name="page">Page number</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of galleries</returns>
    [HttpGet("galleries")]
    [ProducesResponseType(typeof(ApiResponse<Gallery>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<Gallery>>> GetGalleries(
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

            var result = await _artInstituteClient.GetGalleriesAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving galleries");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving galleries");
        }
    }

    /// <summary>
    /// Search galleries using advanced query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results for galleries</returns>
    [HttpPost("galleries/search")]
    [ProducesResponseType(typeof(SearchResponse<Gallery>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchResponse<Gallery>>> SearchGalleries(
        [FromBody] SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (query == null)
            {
                return BadRequest("Search query is required");
            }

            var result = await _artInstituteClient.SearchGalleriesAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching galleries with advanced query");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching galleries");
        }
    }
}
