using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebSpark.ArtSpark.Client.Interfaces;
using WebSpark.ArtSpark.Demo.Models;
using WebSpark.ArtSpark.Demo.Services;
using WebSpark.ArtSpark.Demo.Utilities;

namespace WebSpark.ArtSpark.Demo.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;
    private readonly IFavoriteService _favoriteService;
    private readonly ICollectionService _collectionService;
    private readonly IArtInstituteClient _artInstituteClient;
    private readonly ISeoOptimizationService _seoOptimizationService;
    private readonly IProfilePhotoService _profilePhotoService;
    private readonly IAuditLogService _auditLogService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger,
        IFavoriteService favoriteService,
        ICollectionService collectionService,
        IArtInstituteClient artInstituteClient,
        ISeoOptimizationService seoOptimizationService,
        IProfilePhotoService profilePhotoService,
        IAuditLogService auditLogService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _favoriteService = favoriteService;
        _collectionService = collectionService;
        _artInstituteClient = artInstituteClient;
        _seoOptimizationService = seoOptimizationService;
        _profilePhotoService = profilePhotoService;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                DisplayName = model.DisplayName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Handle profile photo upload if provided
                if (model.ProfilePhoto != null)
                {
                    var photoResult = await _profilePhotoService.UploadPhotoAsync(model.ProfilePhoto, user.Id);
                    if (photoResult.Success)
                    {
                        user.ProfilePhotoFileName = photoResult.FileName;
                        user.ProfilePhotoThumbnail64 = photoResult.Thumbnail64;
                        user.ProfilePhotoThumbnail128 = photoResult.Thumbnail128;
                        user.ProfilePhotoThumbnail256 = photoResult.Thumbnail256;
                        await _userManager.UpdateAsync(user);

                        _logger.LogInformation("Profile photo uploaded for user {UserId} during registration", user.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to upload profile photo during registration: {Error}", photoResult.ErrorMessage);
                    }
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("User {UserId} created a new account with password", user.Id);

                // Log registration event
                await _auditLogService.LogActionAsync("UserRegistered", user.Id, user.Id, new { Email = user.Email, HasPhoto = model.ProfilePhoto != null });

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return View(model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var model = new ProfileViewModel
        {
            DisplayName = user.DisplayName,
            Email = user.Email!,
            Bio = user.Bio,
            CurrentPhotoUrl = _profilePhotoService.GetPhotoUrl(user.ProfilePhotoThumbnail128),
            CreatedAt = user.CreatedAt
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        user.DisplayName = model.DisplayName;
        user.Bio = model.Bio;

        // Handle email changes
        if (user.Email != model.Email)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already in use. If this is your account, please use the password reset feature.");
                model.CurrentPhotoUrl = _profilePhotoService.GetPhotoUrl(user.ProfilePhotoThumbnail128);
                return View(model);
            }
            user.Email = model.Email;
            user.UserName = model.Email;
            user.EmailVerified = false; // Reset verification status
            await _auditLogService.LogActionAsync("EmailChanged", user.Id, user.Id, new { OldEmail = user.Email, NewEmail = model.Email });
        }

        // Handle profile photo operations
        if (model.RemovePhoto)
        {
            await _profilePhotoService.DeletePhotoAsync(user.Id);
            user.ProfilePhotoFileName = null;
            user.ProfilePhotoThumbnail64 = null;
            user.ProfilePhotoThumbnail128 = null;
            user.ProfilePhotoThumbnail256 = null;
            _logger.LogInformation("Profile photo removed for user {UserId}", user.Id);
            await _auditLogService.LogActionAsync("ProfilePhotoRemoved", user.Id, user.Id);
        }
        else if (model.NewProfilePhoto != null)
        {
            var photoResult = await _profilePhotoService.UploadPhotoAsync(model.NewProfilePhoto, user.Id);
            if (photoResult.Success)
            {
                // Delete old photo if exists
                if (!string.IsNullOrEmpty(user.ProfilePhotoFileName))
                {
                    await _profilePhotoService.DeletePhotoAsync(user.Id);
                }

                user.ProfilePhotoFileName = photoResult.FileName;
                user.ProfilePhotoThumbnail64 = photoResult.Thumbnail64;
                user.ProfilePhotoThumbnail128 = photoResult.Thumbnail128;
                user.ProfilePhotoThumbnail256 = photoResult.Thumbnail256;
                _logger.LogInformation("Profile photo updated for user {UserId}", user.Id);
                await _auditLogService.LogActionAsync("ProfilePhotoUploaded", user.Id, user.Id);
            }
            else
            {
                ModelState.AddModelError(nameof(model.NewProfilePhoto), photoResult.ErrorMessage ?? "Failed to upload photo");
                model.CurrentPhotoUrl = _profilePhotoService.GetPhotoUrl(user.ProfilePhotoThumbnail128);
                return View(model);
            }
        }

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            _logger.LogInformation("Profile updated for user {UserId}", user.Id);
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Favorites()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var favorites = await _favoriteService.GetUserFavoritesAsync(user.Id);
        return View(favorites);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Collections()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var collections = await _collectionService.GetUserCollectionsAsync(user.Id);
        return View(collections);
    }

    [Authorize]
    [HttpGet]
    public IActionResult CreateCollection()
    {
        return View(new CreateCollectionViewModel());
    }
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCollection(CreateCollectionViewModel model)
    {
        // If we have an EditCollectionViewModel, we need to generate a slug
        if (model is EditCollectionViewModel editModel)
        {
            string slug = SlugGenerator.GenerateSlug(model.Name);
            editModel.Slug = slug;

            // Revalidate with the slug
            ModelState.Clear();
            TryValidateModel(model);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        await _collectionService.CreateCollectionAsync(model, user.Id);
        TempData["SuccessMessage"] = "Collection created successfully!";
        return RedirectToAction(nameof(Collections));
    }

    [Authorize]
    [HttpGet]
    [Route("Account/EditCollection/{id:int}")]
    public async Task<IActionResult> EditCollection(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var collection = await _collectionService.GetCollectionByIdAsync(id, user.Id);
        if (collection == null)
        {
            return NotFound();
        }

        var model = new EditCollectionViewModel
        {
            Id = collection.Id,
            Slug = collection.Slug,
            Name = collection.Name,
            Description = collection.Description ?? string.Empty,
            LongDescription = collection.LongDescription ?? string.Empty,
            CuratorNotes = collection.CuratorNotes ?? string.Empty,
            Tags = collection.Tags ?? string.Empty,
            IsPublic = collection.IsPublic,
            IsFeatured = collection.IsFeatured,
            FeaturedUntil = collection.FeaturedUntil,
            MetaTitle = collection.MetaTitle ?? string.Empty,
            MetaDescription = collection.MetaDescription ?? string.Empty,
            MetaKeywords = collection.MetaKeywords ?? string.Empty,
            SocialImageUrl = collection.SocialImageUrl ?? string.Empty,
            CreatedAt = collection.CreatedAt,
            ViewCount = collection.ViewCount
        };

        return View(model);
    }
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Account/EditCollection/{id:int}")]
    public async Task<IActionResult> EditCollection(int id, EditCollectionViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        // Generate slug from the name first before validation
        string slug = SlugGenerator.GenerateSlug(model.Name);
        model.Slug = slug;

        // Now validate the model with the generated slug
        ModelState.Clear();
        TryValidateModel(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var success = await _collectionService.UpdateCollectionAsync(id, model, user.Id);
        if (success)
        {
            TempData["SuccessMessage"] = "Collection updated successfully!";
            return RedirectToAction(nameof(CollectionDetails), new { id = id });
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to update collection. Please try again.";
            return View(model);
        }
    }

    [Authorize]
    [HttpGet]
    [Route("Account/Collection/{id:int}")]
    public async Task<IActionResult> CollectionDetails(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var collection = await _collectionService.GetCollectionByIdAsync(id, user.Id);
        if (collection == null)
        {
            return NotFound();
        }
        var artworks = await _collectionService.GetCollectionArtworksAsync(id, user.Id);

        var model = new CollectionDetailsViewModel
        {
            Collection = collection,
            Artworks = artworks,
            CanEdit = true  // User is authenticated and is the owner when accessing via Account controller
        };

        return View(model);
    }
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCollection(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var success = await _collectionService.DeleteCollectionAsync(id, user.Id);
        if (success)
        {
            TempData["SuccessMessage"] = "Collection deleted successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete collection.";
        }

        return RedirectToAction(nameof(Collections));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveArtworkFromCollection(int collectionId, int artworkId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var success = await _collectionService.RemoveArtworkFromCollectionAsync(collectionId, artworkId, user.Id);
        if (success)
        {
            TempData["SuccessMessage"] = "Artwork removed from collection successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to remove artwork from collection.";
        }
        return RedirectToAction(nameof(CollectionDetails), new { id = collectionId });
    }

    #region SEO-Friendly Collection Routes

    /// <summary>
    /// SEO-friendly collection details view accessible by slug for user's own collections
    /// </summary>
    /// <param name="slug">Collection slug</param>
    /// <returns>Collection details view</returns>
    [HttpGet]
    [Route("my/collection/{slug}")]
    public async Task<IActionResult> CollectionDetails(string slug)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        var model = await _collectionService.GetCollectionDetailsBySlugAsync(slug, userId);

        if (model == null)
        {
            return NotFound();
        }

        // Increment view count for public access or collection owner
        if (model.Collection.IsPublic || model.CanEdit)
        {
            await _collectionService.IncrementViewCountAsync(slug);
        }

        return View("CollectionDetails", model);
    }    /// <summary>
         /// SEO-friendly collection item details view for user's own collections
         /// </summary>
         /// <param name="collectionSlug">Collection slug</param>
         /// <param name="itemSlug">Item slug</param>
         /// <returns>Collection item details view</returns>
    [HttpGet]
    [Route("my/collection/{collectionSlug}/item/{itemSlug}")]
    public async Task<IActionResult> CollectionItemDetails(string collectionSlug, string itemSlug)
    {
        if (string.IsNullOrEmpty(collectionSlug) || string.IsNullOrEmpty(itemSlug))
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        var collection = await _collectionService.GetCollectionBySlugAsync(collectionSlug, userId);

        if (collection == null)
        {
            return NotFound();
        }

        var collectionArtwork = collection.Artworks.FirstOrDefault(a => a.Slug == itemSlug);
        if (collectionArtwork == null)
        {
            return NotFound();
        }

        // Increment view count for public collections
        if (collection.IsPublic || collection.UserId == userId)
        {
            await _collectionService.IncrementViewCountAsync(collectionSlug);
        }

        // Fetch artwork details from the API
        var artwork = await _artInstituteClient.GetArtworkAsync(collectionArtwork.ArtworkId);

        var model = new CollectionItemDetailsViewModel
        {
            // Basic item info from API
            Id = collectionArtwork.ArtworkId,
            Title = artwork?.Title ?? "Unknown Title",
            Artist = artwork?.ArtistDisplay ?? "Unknown Artist",
            Year = artwork?.DateStart,
            Medium = artwork?.MediumDisplay,
            Dimensions = artwork?.Dimensions,
            Description = artwork?.Description,
            ImageUrl = !string.IsNullOrEmpty(artwork?.ImageId)
                ? $"https://www.artic.edu/iiif/2/{artwork.ImageId}/full/843,/0/default.jpg"
                : null,
            ExternalUrl = artwork != null ? $"https://www.artic.edu/artworks/{artwork.Id}" : null,

            // Collection item specific info
            Slug = collectionArtwork.Slug ?? string.Empty,
            CustomTitle = collectionArtwork.CustomTitle,
            CustomDescription = collectionArtwork.CustomDescription,
            CuratorNotes = collectionArtwork.CuratorNotes,
            DisplayOrder = collectionArtwork.DisplayOrder,
            IsHighlighted = collectionArtwork.IsHighlighted,            // SEO fields for the collection item
            MetaTitle = collectionArtwork.MetaTitle,
            MetaDescription = collectionArtwork.MetaDescription,
            Keywords = $"{artwork?.ArtistDisplay}, {artwork?.Title}, {collection.Name}",
            SocialImageUrl = !string.IsNullOrEmpty(artwork?.ImageId)
                ? $"https://www.artic.edu/iiif/2/{artwork.ImageId}/full/1200,630/0/default.jpg"
                : null,            // Collection info
            CollectionId = collection.Id,
            CollectionSlug = collection.Slug,
            CollectionTitle = collection.Name,
            CollectionDescription = collection.Description,

            // Timestamps
            UpdatedAt = collectionArtwork.AddedAt, // Use AddedAt as fallback for UpdatedAt

            // Control flags
            CanEdit = userId != null && collection.UserId == userId
        };

        return View(model);
    }

    [Authorize]
    [HttpGet]
    [Route("Account/EditCollectionItem/{collectionId:int}/{artworkId:int}")]
    public async Task<IActionResult> EditCollectionItem(int collectionId, int artworkId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var collection = await _collectionService.GetCollectionByIdAsync(collectionId, user.Id);
        if (collection == null)
        {
            return NotFound();
        }

        var collectionArtwork = collection.Artworks.FirstOrDefault(a => a.ArtworkId == artworkId);
        if (collectionArtwork == null)
        {
            return NotFound();
        }

        // Fetch artwork details from the API
        var artwork = await _artInstituteClient.GetArtworkAsync(artworkId);

        var model = new EditCollectionItemViewModel
        {
            Id = collectionArtwork.Id,
            CollectionId = collectionId,
            ArtworkId = artworkId,
            AddedAt = collectionArtwork.AddedAt,
            Slug = collectionArtwork.Slug ?? string.Empty,
            CustomTitle = collectionArtwork.CustomTitle ?? string.Empty,
            CustomDescription = collectionArtwork.CustomDescription ?? string.Empty,
            CuratorNotes = collectionArtwork.CuratorNotes ?? string.Empty,
            DisplayOrder = collectionArtwork.DisplayOrder,
            IsHighlighted = collectionArtwork.IsHighlighted,
            MetaTitle = collectionArtwork.MetaTitle ?? string.Empty,
            MetaDescription = collectionArtwork.MetaDescription ?? string.Empty,

            // Additional properties for display
            CollectionName = collection.Name,
            CollectionSlug = collection.Slug,
            ArtworkTitle = artwork?.Title ?? "Unknown Title",
            ArtworkArtist = artwork?.ArtistDisplay ?? "Unknown Artist",
            ArtworkImageUrl = !string.IsNullOrEmpty(artwork?.ImageId)
                ? $"https://www.artic.edu/iiif/2/{artwork.ImageId}/full/400,/0/default.jpg"
                : null
        };

        return View(model);
    }
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Account/EditCollectionItem/{collectionId:int}/{artworkId:int}")]
    public async Task<IActionResult> EditCollectionItem(int collectionId, int artworkId, EditCollectionItemViewModel model)
    {
        if (collectionId != model.CollectionId || artworkId != model.ArtworkId)
        {
            return BadRequest();
        }

        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return NotFound();
        }

        // If there's no slug, generate one from the custom title or artwork title
        if (string.IsNullOrEmpty(model.Slug))
        {
            var slug = SlugGenerator.GenerateSlug(!string.IsNullOrEmpty(model.CustomTitle) ? model.CustomTitle : model.ArtworkTitle);
            model.Slug = slug;
            // If we modified the model, we need to revalidate
            ModelState.Clear();
            TryValidateModel(model);
        }

        if (!ModelState.IsValid)
        {
            // Re-populate display properties if validation fails
            var collection = await _collectionService.GetCollectionByIdAsync(collectionId, userId);
            var artwork = await _artInstituteClient.GetArtworkAsync(artworkId);

            model.CollectionName = collection?.Name ?? string.Empty;
            model.CollectionSlug = collection?.Slug ?? string.Empty;
            model.ArtworkTitle = artwork?.Title ?? "Unknown Title";
            model.ArtworkArtist = artwork?.ArtistDisplay ?? "Unknown Artist";
            model.ArtworkImageUrl = !string.IsNullOrEmpty(artwork?.ImageId)
                ? $"https://www.artic.edu/iiif/2/{artwork.ImageId}/full/400,/0/default.jpg"
                : null;

            return View(model);
        }

        // Create updated artwork object
        var updatedArtwork = new CollectionArtwork
        {
            Slug = model.Slug,
            CustomTitle = model.CustomTitle,
            CustomDescription = model.CustomDescription,
            CuratorNotes = model.CuratorNotes,
            DisplayOrder = model.DisplayOrder,
            IsHighlighted = model.IsHighlighted,
            MetaTitle = model.MetaTitle,
            MetaDescription = model.MetaDescription
        };

        // Update the collection artwork with the new values
        var success = await _collectionService.UpdateCollectionArtworkAsync(
            collectionId,
            artworkId,
            userId,
            updatedArtwork
        );

        if (success)
        {
            TempData["SuccessMessage"] = "Collection item updated successfully!";
            return RedirectToAction(nameof(CollectionDetails), new { id = collectionId });
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to update collection item. Please try again.";

            // Re-populate display properties for error case
            var collection = await _collectionService.GetCollectionByIdAsync(collectionId, userId);
            var artwork = await _artInstituteClient.GetArtworkAsync(artworkId);

            model.CollectionName = collection?.Name ?? string.Empty;
            model.CollectionSlug = collection?.Slug ?? string.Empty;
            model.ArtworkTitle = artwork?.Title ?? "Unknown Title";
            model.ArtworkArtist = artwork?.ArtistDisplay ?? "Unknown Artist";
            model.ArtworkImageUrl = !string.IsNullOrEmpty(artwork?.ImageId)
                ? $"https://www.artic.edu/iiif/2/{artwork.ImageId}/full/400,/0/default.jpg"
                : null;

            return View(model);
        }
    }

    #endregion
}
