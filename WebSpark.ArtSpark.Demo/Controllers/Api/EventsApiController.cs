using Microsoft.AspNetCore.Mvc;
using WebSpark.ArtSpark.Client.Interfaces;
using WebSpark.ArtSpark.Client.Models.Common;
using WebSpark.ArtSpark.Client.Models.Website;

namespace WebSpark.ArtSpark.Demo.Controllers.Api;

/// <summary>
/// API controller for event-related operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsApiController : ControllerBase
{
    private readonly IArtInstituteClient _artInstituteClient;
    private readonly ILogger<EventsApiController> _logger;

    public EventsApiController(
        IArtInstituteClient artInstituteClient,
        ILogger<EventsApiController> logger)
    {
        _artInstituteClient = artInstituteClient ?? throw new ArgumentNullException(nameof(artInstituteClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get a specific event by ID
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Event details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Event>> GetEvent(
        int id,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var eventItem = await _artInstituteClient.GetEventAsync(id, fields, cancellationToken);

            if (eventItem == null)
            {
                _logger.LogWarning("Event with ID {EventId} not found", id);
                return NotFound($"Event with ID {id} not found");
            }

            return Ok(eventItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event with ID {EventId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the event");
        }
    }

    /// <summary>
    /// Get all events with optional query parameters
    /// </summary>
    /// <param name="limit">Number of results to return</param>
    /// <param name="page">Page number</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of events</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<Event>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<Event>>> GetEvents(
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

            var result = await _artInstituteClient.GetEventsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving events");
        }
    }

    /// <summary>
    /// Search events using advanced query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results for events</returns>
    [HttpPost("search")]
    [ProducesResponseType(typeof(SearchResponse<Event>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchResponse<Event>>> SearchEvents(
        [FromBody] SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (query == null)
            {
                return BadRequest("Search query is required");
            }

            var result = await _artInstituteClient.SearchEventsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching events with advanced query");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching events");
        }
    }

    /// <summary>
    /// Get a specific event program by ID
    /// </summary>
    /// <param name="id">Event program ID</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Event program details</returns>
    [HttpGet("programs/{id:int}")]
    [ProducesResponseType(typeof(EventProgram), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventProgram>> GetEventProgram(
        int id,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var eventProgram = await _artInstituteClient.GetEventProgramAsync(id, fields, cancellationToken);

            if (eventProgram == null)
            {
                _logger.LogWarning("Event program with ID {EventProgramId} not found", id);
                return NotFound($"Event program with ID {id} not found");
            }

            return Ok(eventProgram);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event program with ID {EventProgramId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the event program");
        }
    }

    /// <summary>
    /// Get all event programs with optional query parameters
    /// </summary>
    /// <param name="limit">Number of results to return</param>
    /// <param name="page">Page number</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of event programs</returns>
    [HttpGet("programs")]
    [ProducesResponseType(typeof(ApiResponse<EventProgram>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EventProgram>>> GetEventPrograms(
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

            var result = await _artInstituteClient.GetEventProgramsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event programs");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving event programs");
        }
    }

    /// <summary>
    /// Search event programs using advanced query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results for event programs</returns>
    [HttpPost("programs/search")]
    [ProducesResponseType(typeof(SearchResponse<EventProgram>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchResponse<EventProgram>>> SearchEventPrograms(
        [FromBody] SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (query == null)
            {
                return BadRequest("Search query is required");
            }

            var result = await _artInstituteClient.SearchEventProgramsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching event programs with advanced query");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching event programs");
        }
    }

    /// <summary>
    /// Get a specific event occurrence by ID
    /// </summary>
    /// <param name="id">Event occurrence ID</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Event occurrence details</returns>
    [HttpGet("occurrences/{id}")]
    [ProducesResponseType(typeof(EventOccurrence), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventOccurrence>> GetEventOccurrence(
        string id,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var eventOccurrence = await _artInstituteClient.GetEventOccurrenceAsync(id, fields, cancellationToken);

            if (eventOccurrence == null)
            {
                _logger.LogWarning("Event occurrence with ID {EventOccurrenceId} not found", id);
                return NotFound($"Event occurrence with ID {id} not found");
            }

            return Ok(eventOccurrence);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event occurrence with ID {EventOccurrenceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the event occurrence");
        }
    }

    /// <summary>
    /// Get all event occurrences with optional query parameters
    /// </summary>
    /// <param name="limit">Number of results to return</param>
    /// <param name="page">Page number</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of event occurrences</returns>
    [HttpGet("occurrences")]
    [ProducesResponseType(typeof(ApiResponse<EventOccurrence>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EventOccurrence>>> GetEventOccurrences(
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

            var result = await _artInstituteClient.GetEventOccurrencesAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event occurrences");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving event occurrences");
        }
    }

    /// <summary>
    /// Search event occurrences using advanced query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results for event occurrences</returns>
    [HttpPost("occurrences/search")]
    [ProducesResponseType(typeof(SearchResponse<EventOccurrence>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchResponse<EventOccurrence>>> SearchEventOccurrences(
        [FromBody] SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (query == null)
            {
                return BadRequest("Search query is required");
            }

            var result = await _artInstituteClient.SearchEventOccurrencesAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching event occurrences with advanced query");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching event occurrences");
        }
    }
}
