using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebSpark.ArtSpark.Agent.Interfaces;
using WebSpark.ArtSpark.Agent.Models;
using WebSpark.ArtSpark.Agent.Personas;
using WebSpark.ArtSpark.Client.Interfaces;
using WebSpark.ArtSpark.Client.Models.Common;
using WebSpark.ArtSpark.Demo.Services;

namespace WebSpark.ArtSpark.Demo.Controllers;

/// <summary>
/// Controller for displaying artwork from the Art Institute of Chicago
/// </summary>
[Authorize]
public class ArtworkController : Controller
{
    private readonly IArtInstituteClient _artInstituteClient;
    private readonly IArtworkChatAgent _chatAgent;
    private readonly ILogger<ArtworkController> _logger;
    private readonly IReviewService _reviewService;
    private readonly ICollectionService _collectionService;
    private readonly IFavoriteService _favoriteService;
    private readonly IProfilePhotoService _profilePhotoService;

    public ArtworkController(
        IArtInstituteClient artInstituteClient,
        IArtworkChatAgent chatAgent,
        ILogger<ArtworkController> logger,
        IReviewService reviewService,
        IFavoriteService favoriteService,
        ICollectionService collectionService,
        IProfilePhotoService profilePhotoService)
    {
        _artInstituteClient = artInstituteClient ?? throw new ArgumentNullException(nameof(artInstituteClient));
        _chatAgent = chatAgent ?? throw new ArgumentNullException(nameof(chatAgent));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _reviewService = reviewService ?? throw new ArgumentNullException(nameof(reviewService));
        _favoriteService = favoriteService ?? throw new ArgumentNullException(nameof(favoriteService));
        _collectionService = collectionService ?? throw new ArgumentNullException(nameof(collectionService));
        _profilePhotoService = profilePhotoService ?? throw new ArgumentNullException(nameof(profilePhotoService));
    }

    /// <summary>
    /// Display a paginated list of artwork
    /// </summary>
    [AllowAnonymous]
    public async Task<IActionResult> Index(int page = 1, int limit = 12)
    {
        try
        {
            var query = new ApiQuery
            {
                Page = page,
                Limit = limit,
                Fields = "id,title,artist_display,date_display,medium_display,image_id"
            };

            var response = await _artInstituteClient.GetArtworksAsync(query);

            if (response?.Data == null)
            {
                _logger.LogWarning("No artwork returned from API");
                return View("Error");
            }

            return View(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching artwork");
            return View("Error");
        }
    }

    /// <summary>
    /// Display details for a specific artwork
    /// </summary>
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id, string? returnUrl = null)
    {
        try
        {
            var artwork = await _artInstituteClient.GetArtworkAsync(id);

            if (artwork == null)
            {
                _logger.LogWarning("Artwork with ID {ArtworkId} not found", id);
                return NotFound();
            }

            // Store the return URL in ViewBag for the view to use
            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index");

            return View(artwork);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching artwork details for ID {ArtworkId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// Search for artwork
    /// </summary>
    [AllowAnonymous]
    public async Task<IActionResult> Search(string q, int page = 1, int limit = 12)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return RedirectToAction(nameof(Index));
            }

            var searchQuery = new SearchQuery
            {
                Q = q,
                From = (page - 1) * limit,
                Size = limit,
                Fields = "id,title,artist_display,date_display,medium_display,image_id"
            };

            var response = await _artInstituteClient.SearchArtworksAsync(searchQuery);

            if (response?.Data == null)
            {
                _logger.LogWarning("No search results returned for query: {SearchQuery}", q);
                ViewBag.SearchQuery = q;
                return View("SearchResults", response);
            }

            ViewBag.SearchQuery = q;
            return View("SearchResults", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching artwork for query: {SearchQuery}", q);
            return View("Error");
        }
    }

    /// <summary>
    /// Display featured/highlighted artwork
    /// </summary>
    [AllowAnonymous]
    public async Task<IActionResult> Featured()
    {
        try
        {
            // Get a selection of featured artwork with images
            var query = new ApiQuery
            {
                Page = 1,
                Limit = 20,
                Fields = "id,title,artist_display,date_display,medium_display,image_id,thumbnail"
            };

            var response = await _artInstituteClient.GetArtworksAsync(query);

            if (response?.Data == null)
            {
                _logger.LogWarning("No featured artwork returned from API");
                return View("Error");
            }

            // Filter to only show items with images
            var featuredArtworks = response.Data.Where(a => !string.IsNullOrEmpty(a.ImageId)).Take(12).ToList();

            return View(featuredArtworks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching featured artwork");
            return View("Error");
        }
    }    /// <summary>
         /// Submit or update a review for an artwork
         /// </summary>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReview(int artworkId, int rating, string? reviewText, string? returnUrl = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            await _reviewService.CreateOrUpdateReviewAsync(artworkId, userId, rating, reviewText);

            TempData["SuccessMessage"] = "Your review has been submitted successfully!";
            return RedirectToAction("Details", new { id = artworkId, returnUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting review for artwork {ArtworkId}", artworkId);
            TempData["ErrorMessage"] = "There was an error submitting your review. Please try again.";
            return RedirectToAction("Details", new { id = artworkId, returnUrl });
        }
    }

    /// <summary>
    /// Get reviews for an artwork (AJAX endpoint)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviews(int artworkId)
    {
        try
        {
            var reviews = await _reviewService.GetReviewsForArtworkAsync(artworkId);
            var averageRating = await _reviewService.GetAverageRatingAsync(artworkId);
            var reviewCount = await _reviewService.GetReviewCountAsync(artworkId);

            var result = new
            {
                reviews = reviews.Select(r => new
                {
                    id = r.Id,
                    userName = r.User.DisplayName,
                    userPhotoUrl = _profilePhotoService.GetPhotoUrl(r.User.ProfilePhotoThumbnail64),
                    userInitial = r.User.DisplayName.FirstOrDefault().ToString().ToUpper(),
                    rating = r.Rating,
                    reviewText = r.ReviewText,
                    createdAt = r.CreatedAt.ToString("MMM dd, yyyy"),
                    isOwner = User.FindFirstValue(ClaimTypes.NameIdentifier) == r.UserId
                }),
                averageRating = Math.Round(averageRating, 1),
                reviewCount = reviewCount
            };

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching reviews for artwork {ArtworkId}", artworkId);
            return Json(new { error = "Failed to load reviews" });
        }
    }

    /// <summary>
    /// Toggle favorite status for an artwork
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleFavorite(int artworkId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var isFavorited = await _favoriteService.IsArtworkFavoritedAsync(artworkId, userId);
            bool success;

            if (isFavorited)
            {
                success = await _favoriteService.RemoveFromFavoritesAsync(artworkId, userId);
            }
            else
            {
                success = await _favoriteService.AddToFavoritesAsync(artworkId, userId);
            }

            return Json(new { success = success, isFavorited = !isFavorited });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling favorite for artwork {ArtworkId}", artworkId);
            return Json(new { success = false, error = "Failed to update favorite status" });
        }
    }

    /// <summary>
    /// Delete a review
    /// </summary>
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await _reviewService.DeleteReviewAsync(reviewId, userId);

            return Json(new { success = success });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review {ReviewId}", reviewId);
            return Json(new { success = false, error = "Failed to delete review" });
        }
    }

    /// <summary>
    /// Chat endpoint for AI-powered artwork discussions
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Chat(string message, int artworkId, string persona = "Curator")
    {
        try
        {
            _logger.LogInformation("Chat endpoint called - Message: {Message}, ArtworkId: {ArtworkId}, Persona: {Persona}",
                message, artworkId, persona);

            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("Chat message is null or empty: '{Message}'", message);
                return BadRequest(new { error = "Message is required" });
            }

            // Convert persona string to enum
            var chatPersona = persona.ToLowerInvariant() switch
            {
                "curator" => ChatPersona.Curator,
                "artist" => ChatPersona.Artist,
                "historian" => ChatPersona.Historian,
                "artwork" => ChatPersona.Artwork,
                _ => ChatPersona.Curator
            };

            var request = new ChatRequest
            {
                Message = message,
                ArtworkId = artworkId,
                Persona = chatPersona,
                ConversationHistory = new List<ChatMessage>(),
                IncludeVisualAnalysis = true
            };

            _logger.LogInformation("Processing chat request for artwork {ArtworkId} with message: {Message}",
                request.ArtworkId, request.Message);

            var response = await _chatAgent.ChatAsync(request);

            return Json(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request: {Message}", message);
            return Json(new ChatResponse
            {
                Response = "I apologize, but I'm having trouble processing your request right now. Please try again later.",
                Success = false,
                Error = "An error occurred while processing your request"
            });
        }
    }

    /// <summary>
    /// Generate conversation starters for an artwork
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConversationStarters(int artworkId, string persona = "Curator")
    {
        try
        {
            // Get artwork data first
            var artwork = await _artInstituteClient.GetArtworkAsync(artworkId);
            if (artwork == null)
            {
                return NotFound(new { error = "Artwork not found" });
            }            // Convert to ArtworkData format
            var artworkData = new ArtworkData
            {
                Id = artwork.Id is int id ? id : (artwork.Id?.ToString() is string idStr && int.TryParse(idStr, out var parsedId) ? parsedId : 0),
                Title = artwork.Title ?? "Unknown Title",
                ArtistDisplay = artwork.ArtistDisplay ?? "Unknown Artist",
                DateDisplay = artwork.DateDisplay ?? "Unknown Date",
                Medium = artwork.MediumDisplay ?? "Unknown Medium",
                Dimensions = artwork.Dimensions ?? "Unknown Dimensions",
                PlaceOfOrigin = artwork.PlaceOfOrigin ?? "Unknown Origin",
                Description = artwork.PublicationHistory ?? "No description available",
                CulturalContext = artwork.StyleTitle ?? "Unknown style",
                Classification = artwork.ClassificationTitle ?? "Unknown classification",
                ImageUrl = !string.IsNullOrEmpty(artwork.ImageId) ?
                    $"https://www.artic.edu/iiif/2/{artwork.ImageId}/full/843,/0/default.jpg" : string.Empty,
                ThumbnailUrl = !string.IsNullOrEmpty(artwork.ImageId) ?
                    $"https://www.artic.edu/iiif/2/{artwork.ImageId}/full/200,/0/default.jpg" : string.Empty
            };

            // Get the persona
            var chatPersona = persona.ToLowerInvariant() switch
            {
                "curator" => ChatPersona.Curator,
                "artist" => ChatPersona.Artist,
                "historian" => ChatPersona.Historian,
                "artwork" => ChatPersona.Artwork,
                _ => ChatPersona.Curator
            };

            var starters = await _chatAgent.GenerateConversationStartersAsync(artworkData, chatPersona);
            return Json(new { conversationStarters = starters });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating conversation starters for artwork {ArtworkId}", artworkId);
            return Json(new { success = false, error = "Failed to generate conversation starters" });
        }
    }

    /// <summary>
    /// Get collections for the logged-in user
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> MyCollections()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var collections = await _collectionService.GetUserCollectionsAsync(userId);

            return View(collections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching collections for user");
            return View("Error");
        }
    }

    /// <summary>
    /// Add an artwork to a collection
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddToCollection(int artworkId, int collectionId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await _collectionService.AddArtworkToCollectionAsync(collectionId, artworkId, userId);

            return Json(new { success = success });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding artwork {ArtworkId} to collection {CollectionId}", artworkId, collectionId);
            return Json(new { success = false, error = "Failed to add artwork to collection" });
        }
    }

    /// <summary>
    /// Get user collections for Add to Collection modal (AJAX endpoint)
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetUserCollections()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var collections = await _collectionService.GetUserCollectionsAsync(userId);

            var result = collections.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                description = c.Description,
                artworkCount = c.Artworks?.Count ?? 0
            });

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user collections");
            return Json(new { error = "Failed to load collections" });
        }
    }
}
