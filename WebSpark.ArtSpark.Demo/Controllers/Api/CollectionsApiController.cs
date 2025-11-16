using Microsoft.AspNetCore.Mvc;
using WebSpark.ArtSpark.Client.Interfaces;
using WebSpark.ArtSpark.Client.Models.Collections;
using WebSpark.ArtSpark.Client.Models.Common;

namespace WebSpark.ArtSpark.Demo.Controllers.Api;

/// <summary>
/// API controller for collections-related operations (agents, agent roles, agent types, category terms)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CollectionsApiController : ControllerBase
{
    private readonly IArtInstituteClient _artInstituteClient;
    private readonly ILogger<CollectionsApiController> _logger;

    public CollectionsApiController(
        IArtInstituteClient artInstituteClient,
        ILogger<CollectionsApiController> logger)
    {
        _artInstituteClient = artInstituteClient ?? throw new ArgumentNullException(nameof(artInstituteClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get a specific agent by ID
    /// </summary>
    /// <param name="id">Agent ID</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Agent details</returns>
    [HttpGet("agents/{id:int}")]
    [ProducesResponseType(typeof(WebSpark.ArtSpark.Client.Models.Collections.Agent), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WebSpark.ArtSpark.Client.Models.Collections.Agent>> GetAgent(
        int id,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var agent = await _artInstituteClient.GetAgentAsync(id, fields, cancellationToken);

            if (agent == null)
            {
                _logger.LogWarning("Agent with ID {AgentId} not found", id);
                return NotFound($"Agent with ID {id} not found");
            }

            return Ok(agent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agent with ID {AgentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the agent");
        }
    }

    /// <summary>
    /// Get all agents with optional query parameters
    /// </summary>
    /// <param name="limit">Number of results to return</param>
    /// <param name="page">Page number</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of agents</returns>
    [HttpGet("agents")]
    [ProducesResponseType(typeof(ApiResponse<WebSpark.ArtSpark.Client.Models.Collections.Agent>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<WebSpark.ArtSpark.Client.Models.Collections.Agent>>> GetAgents(
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

            var result = await _artInstituteClient.GetAgentsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agents");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving agents");
        }
    }

    /// <summary>
    /// Search agents using advanced query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results for agents</returns>
    [HttpPost("agents/search")]
    [ProducesResponseType(typeof(SearchResponse<WebSpark.ArtSpark.Client.Models.Collections.Agent>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchResponse<WebSpark.ArtSpark.Client.Models.Collections.Agent>>> SearchAgents(
        [FromBody] SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (query == null)
            {
                return BadRequest("Search query is required");
            }

            var result = await _artInstituteClient.SearchAgentsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching agents with advanced query");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching agents");
        }
    }

    /// <summary>
    /// Get a specific agent role by ID
    /// </summary>
    /// <param name="id">Agent role ID</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Agent role details</returns>
    [HttpGet("agent-roles/{id:int}")]
    [ProducesResponseType(typeof(AgentRole), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AgentRole>> GetAgentRole(
        int id,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var agentRole = await _artInstituteClient.GetAgentRoleAsync(id, fields, cancellationToken);

            if (agentRole == null)
            {
                _logger.LogWarning("Agent role with ID {AgentRoleId} not found", id);
                return NotFound($"Agent role with ID {id} not found");
            }

            return Ok(agentRole);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agent role with ID {AgentRoleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the agent role");
        }
    }

    /// <summary>
    /// Get all agent roles with optional query parameters
    /// </summary>
    /// <param name="limit">Number of results to return</param>
    /// <param name="page">Page number</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of agent roles</returns>
    [HttpGet("agent-roles")]
    [ProducesResponseType(typeof(ApiResponse<AgentRole>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AgentRole>>> GetAgentRoles(
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

            var result = await _artInstituteClient.GetAgentRolesAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agent roles");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving agent roles");
        }
    }

    /// <summary>
    /// Get a specific agent type by ID
    /// </summary>
    /// <param name="id">Agent type ID</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Agent type details</returns>
    [HttpGet("agent-types/{id:int}")]
    [ProducesResponseType(typeof(AgentType), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AgentType>> GetAgentType(
        int id,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var agentType = await _artInstituteClient.GetAgentTypeAsync(id, fields, cancellationToken);

            if (agentType == null)
            {
                _logger.LogWarning("Agent type with ID {AgentTypeId} not found", id);
                return NotFound($"Agent type with ID {id} not found");
            }

            return Ok(agentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agent type with ID {AgentTypeId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the agent type");
        }
    }

    /// <summary>
    /// Get all agent types with optional query parameters
    /// </summary>
    /// <param name="limit">Number of results to return</param>
    /// <param name="page">Page number</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of agent types</returns>
    [HttpGet("agent-types")]
    [ProducesResponseType(typeof(ApiResponse<AgentType>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AgentType>>> GetAgentTypes(
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

            var result = await _artInstituteClient.GetAgentTypesAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agent types");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving agent types");
        }
    }

    /// <summary>
    /// Get a specific category term by ID
    /// </summary>
    /// <param name="id">Category term ID</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Category term details</returns>
    [HttpGet("category-terms/{id}")]
    [ProducesResponseType(typeof(CategoryTerm), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryTerm>> GetCategoryTerm(
        string id,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var categoryTerm = await _artInstituteClient.GetCategoryTermAsync(id, fields, cancellationToken);

            if (categoryTerm == null)
            {
                _logger.LogWarning("Category term with ID {CategoryTermId} not found", id);
                return NotFound($"Category term with ID {id} not found");
            }

            return Ok(categoryTerm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category term with ID {CategoryTermId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the category term");
        }
    }

    /// <summary>
    /// Get all category terms with optional query parameters
    /// </summary>
    /// <param name="limit">Number of results to return</param>
    /// <param name="page">Page number</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of category terms</returns>
    [HttpGet("category-terms")]
    [ProducesResponseType(typeof(ApiResponse<CategoryTerm>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CategoryTerm>>> GetCategoryTerms(
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

            var result = await _artInstituteClient.GetCategoryTermsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category terms");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving category terms");
        }
    }

    /// <summary>
    /// Search category terms using advanced query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results for category terms</returns>
    [HttpPost("category-terms/search")]
    [ProducesResponseType(typeof(SearchResponse<CategoryTerm>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchResponse<CategoryTerm>>> SearchCategoryTerms(
        [FromBody] SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (query == null)
            {
                return BadRequest("Search query is required");
            }

            var result = await _artInstituteClient.SearchCategoryTermsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching category terms with advanced query");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching category terms");
        }
    }

    /// <summary>
    /// Get places
    /// </summary>
    /// <param name="limit">Number of results to return</param>
    /// <param name="page">Page number</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of places</returns>
    [HttpGet("places")]
    [ProducesResponseType(typeof(ApiResponse<Place>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<Place>>> GetPlaces(
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

            var result = await _artInstituteClient.GetPlacesAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving places");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving places");
        }
    }

    /// <summary>
    /// Get a specific place by ID
    /// </summary>
    /// <param name="id">Place ID</param>
    /// <param name="fields">Optional fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Place details</returns>
    [HttpGet("places/{id:int}")]
    [ProducesResponseType(typeof(Place), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Place>> GetPlace(
        int id,
        [FromQuery] string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var place = await _artInstituteClient.GetPlaceAsync(id, fields, cancellationToken);

            if (place == null)
            {
                _logger.LogWarning("Place with ID {PlaceId} not found", id);
                return NotFound($"Place with ID {id} not found");
            }

            return Ok(place);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving place with ID {PlaceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the place");
        }
    }

    /// <summary>
    /// Search places using advanced query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results for places</returns>
    [HttpPost("places/search")]
    [ProducesResponseType(typeof(SearchResponse<Place>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchResponse<Place>>> SearchPlaces(
        [FromBody] SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (query == null)
            {
                return BadRequest("Search query is required");
            }

            var result = await _artInstituteClient.SearchPlacesAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching places with advanced query");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching places");
        }
    }
}
