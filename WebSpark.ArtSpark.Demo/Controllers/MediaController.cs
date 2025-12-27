using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebSpark.ArtSpark.Demo.Models;
using WebSpark.ArtSpark.Demo.Options;
using WebSpark.ArtSpark.Demo.Services;

namespace WebSpark.ArtSpark.Demo.Controllers;

[Authorize]
public class MediaController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICollectionService _collectionService;
    private readonly ILogger<MediaController> _logger;
    private readonly FileUploadOptions _fileUploadOptions;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".mp4", ".mov", ".avi", ".pdf", ".doc", ".docx" };
    private readonly string[] AllowedMimeTypes = {
        "image/jpeg", "image/png", "image/gif", "image/webp",
        "video/mp4", "video/quicktime", "video/x-msvideo",
        "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    public MediaController(
        UserManager<ApplicationUser> userManager,
        ICollectionService collectionService,
        ILogger<MediaController> logger,
        IOptions<FileUploadOptions> fileUploadOptions)
    {
        _userManager = userManager;
        _collectionService = collectionService;
        _logger = logger;
        _fileUploadOptions = fileUploadOptions.Value;
    }

    /// <summary>
    /// Upload media file to a collection
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadMedia(int collectionId, IFormFile file, string? title, string? description, string? altText)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, error = "No file selected" });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, error = "User not authenticated" });
            }

            // Validate file
            var validationResult = ValidateFile(file);
            if (!validationResult.IsValid)
            {
                return Json(new { success = false, error = validationResult.ErrorMessage });
            }

            // Check if user owns the collection
            var collection = await _collectionService.GetCollectionByIdAsync(collectionId, userId);
            if (collection == null)
            {
                return Json(new { success = false, error = "Collection not found or access denied" });
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var originalFileName = file.FileName;

            // Create uploads directory using configured path
            var uploadsPath = Path.Combine(Path.GetFullPath(_fileUploadOptions.CollectionMediaPath), collectionId.ToString());
            Directory.CreateDirectory(uploadsPath);

            // Save file
            var filePath = Path.Combine(uploadsPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Determine media type
            var mediaType = GetMediaType(file.ContentType);

            // Create collection media record
            var mediaItem = new CollectionMediaViewModel
            {
                CollectionId = collectionId,
                MediaUrl = $"/uploads/collections/{collectionId}/{fileName}",
                FileName = fileName,
                OriginalFileName = originalFileName,
                MediaType = mediaType,
                MimeType = file.ContentType,
                FileSize = file.Length,
                Title = title,
                Description = description,
                AltText = altText,
                DisplayOrder = 0
            };

            var media = await _collectionService.AddMediaAsync(collectionId, userId, mediaItem.MediaUrl, mediaItem.MediaType, mediaItem.Title, mediaItem.Description);
            var mediaId = media.Id;

            _logger.LogInformation("Media uploaded successfully for collection {CollectionId}. File: {FileName}, Size: {FileSize} bytes",
                collectionId, fileName, file.Length);

            return Json(new
            {
                success = true,
                mediaId = mediaId,
                fileName = fileName,
                mediaUrl = mediaItem.MediaUrl,
                mediaType = mediaType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading media file for collection {CollectionId}", collectionId);
            return Json(new { success = false, error = "Failed to upload file" });
        }
    }

    /// <summary>
    /// Delete media file from collection
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMedia(int mediaId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, error = "User not authenticated" });
            }

            var success = await _collectionService.DeleteMediaAsync(mediaId, userId);
            if (success)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, error = "Failed to delete media or access denied" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting media {MediaId}", mediaId);
            return Json(new { success = false, error = "Failed to delete media" });
        }
    }

    /// <summary>
    /// Update media item details
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateMedia(CollectionMediaViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, error = "Invalid data provided" });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, error = "User not authenticated" });
            }

            var success = await _collectionService.UpdateMediaAsync(model.Id, userId, model.Title, model.Description);
            if (success)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, error = "Failed to update media or access denied" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating media {MediaId}", model.Id);
            return Json(new { success = false, error = "Failed to update media" });
        }
    }

    /// <summary>
    /// Reorder media items in a collection
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReorderMedia(int collectionId, int[] mediaIds)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, error = "User not authenticated" });
            }

            var success = await _collectionService.ReorderMediaAsync(collectionId, mediaIds, userId);
            if (success)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, error = "Failed to reorder media or access denied" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering media for collection {CollectionId}", collectionId);
            return Json(new { success = false, error = "Failed to reorder media" });
        }
    }

    #region Private Methods

    private FileValidationResult ValidateFile(IFormFile file)
    {
        // Check file size
        if (file.Length > MaxFileSize)
        {
            return new FileValidationResult
            {
                IsValid = false,
                ErrorMessage = $"File size exceeds the maximum limit of {MaxFileSize / (1024 * 1024)}MB"
            };
        }

        // Check file extension
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(fileExtension))
        {
            return new FileValidationResult
            {
                IsValid = false,
                ErrorMessage = "File type not allowed. Supported formats: " + string.Join(", ", AllowedExtensions)
            };
        }

        // Check MIME type
        if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return new FileValidationResult
            {
                IsValid = false,
                ErrorMessage = "File MIME type not allowed"
            };
        }

        return new FileValidationResult { IsValid = true };
    }

    private string GetMediaType(string mimeType)
    {
        return mimeType.ToLowerInvariant() switch
        {
            var mime when mime.StartsWith("image/") => "image",
            var mime when mime.StartsWith("video/") => "video",
            var mime when mime.StartsWith("audio/") => "audio",
            "application/pdf" => "document",
            var mime when mime.Contains("word") => "document",
            _ => "document"
        };
    }

    #endregion
}

/// <summary>
/// File validation result
/// </summary>
public class FileValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}
