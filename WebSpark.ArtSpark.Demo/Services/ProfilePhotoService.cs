using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using WebSpark.ArtSpark.Demo.Options;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace WebSpark.ArtSpark.Demo.Services;

public class ProfilePhotoService : IProfilePhotoService
{
    private readonly FileUploadOptions _options;
    private readonly ILogger<ProfilePhotoService> _logger;
    private readonly string _uploadPath;

    public ProfilePhotoService(
        IOptions<FileUploadOptions> options,
        ILogger<ProfilePhotoService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _uploadPath = Path.GetFullPath(_options.ProfilePhotoPath);

        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
            _logger.LogInformation("Created profile photo upload directory at {Path}", _uploadPath);
        }
    }

    public async Task<ProfilePhotoUploadResult> UploadPhotoAsync(
        IFormFile file,
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting profile photo upload for user {UserId}. Upload path: {UploadPath}", userId, _uploadPath);
            
            // Validate file
            if (!await ValidatePhotoAsync(file))
            {
                _logger.LogWarning("Profile photo validation failed for user {UserId}. File: {FileName}, Size: {Size}, Type: {ContentType}", 
                    userId, file.FileName, file.Length, file.ContentType);
                return new ProfilePhotoUploadResult
                {
                    Success = false,
                    ErrorMessage = "Invalid file. Please upload a JPEG, PNG, or WebP image under 5MB."
                };
            }

            _logger.LogInformation("Validation passed for user {UserId}. File: {FileName}, Size: {Size}, Type: {ContentType}", 
                userId, file.FileName, file.Length, file.ContentType);

            // Generate unique filename - use .jpg as default since most images will be saved as JPEG
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var baseFileName = $"{userId}_{Guid.NewGuid()}";
            var safeFileName = $"{baseFileName}{fileExtension}";
            var originalPath = Path.Combine(_uploadPath, safeFileName);

            _logger.LogInformation("Generated filename: {FileName}, Full path: {Path}", safeFileName, originalPath);

            // Load and process image
            using var stream = file.OpenReadStream();
            using var image = await ImageSharpImage.LoadAsync(stream, cancellationToken);

            _logger.LogInformation("Image loaded successfully. Dimensions: {Width}x{Height}", image.Width, image.Height);

            // Auto-orient based on EXIF data
            image.Mutate(x => x.AutoOrient());

            // Strip metadata for privacy
            image.Metadata.ExifProfile = null;
            image.Metadata.IptcProfile = null;
            image.Metadata.XmpProfile = null;

            _logger.LogInformation("About to save original image to: {Path}", originalPath);

            // Save original (optimized) and get actual saved filename
            var savedOriginalName = await SaveOptimizedImageAsync(image, originalPath, file.ContentType, cancellationToken);

            _logger.LogInformation("Original image saved as: {FileName}", savedOriginalName);

            // Generate thumbnails using the base filename
            _logger.LogInformation("Generating thumbnail 64x64");
            var thumbnail64 = await GenerateThumbnailAsync(image, 64, baseFileName, file.ContentType, cancellationToken);
            
            _logger.LogInformation("Generating thumbnail 128x128");
            var thumbnail128 = await GenerateThumbnailAsync(image, 128, baseFileName, file.ContentType, cancellationToken);
            
            _logger.LogInformation("Generating thumbnail 256x256");
            var thumbnail256 = await GenerateThumbnailAsync(image, 256, baseFileName, file.ContentType, cancellationToken);

            _logger.LogInformation(
                "Profile photo uploaded successfully for user {UserId}. Original: {FileName}, Thumbnails: {T64}, {T128}, {T256}",
                userId, savedOriginalName, thumbnail64, thumbnail128, thumbnail256);

            // Verify files exist
            var originalFullPath = Path.Combine(_uploadPath, savedOriginalName);
            var thumb64Path = Path.Combine(_uploadPath, thumbnail64);
            var thumb128Path = Path.Combine(_uploadPath, thumbnail128);
            var thumb256Path = Path.Combine(_uploadPath, thumbnail256);

            _logger.LogInformation("Verifying files exist:");
            _logger.LogInformation("  Original: {Path} - Exists: {Exists}", originalFullPath, File.Exists(originalFullPath));
            _logger.LogInformation("  Thumb64: {Path} - Exists: {Exists}", thumb64Path, File.Exists(thumb64Path));
            _logger.LogInformation("  Thumb128: {Path} - Exists: {Exists}", thumb128Path, File.Exists(thumb128Path));
            _logger.LogInformation("  Thumb256: {Path} - Exists: {Exists}", thumb256Path, File.Exists(thumb256Path));

            return new ProfilePhotoUploadResult
            {
                Success = true,
                FileName = savedOriginalName,
                Thumbnail64 = thumbnail64,
                Thumbnail128 = thumbnail128,
                Thumbnail256 = thumbnail256
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile photo for user {UserId}. Exception: {Message}, StackTrace: {StackTrace}", 
                userId, ex.Message, ex.StackTrace);
            return new ProfilePhotoUploadResult
            {
                Success = false,
                ErrorMessage = "An error occurred while uploading your photo. Please try again."
            };
        }
    }

    public async Task<bool> DeletePhotoAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find and delete all files for this user
            var userFiles = Directory.GetFiles(_uploadPath, $"{userId}_*");
            foreach (var file in userFiles)
            {
                File.Delete(file);
            }

            _logger.LogInformation("Deleted {Count} profile photo files for user {UserId}", userFiles.Length, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile photos for user {UserId}", userId);
            return false;
        }
    }

    public string? GetPhotoUrl(string? fileName, int? size = null)
    {
        if (string.IsNullOrEmpty(fileName))
            return null;

        if (size.HasValue)
        {
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            fileName = $"{baseName}_{size}x{size}{extension}";
        }

        return $"/uploads/profiles/{fileName}";
    }

    public async Task<bool> ValidatePhotoAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        // Check file size
        if (file.Length > _options.MaxProfilePhotoSize)
            return false;

        // Check file type
        var allowedTypes = _options.GetAllowedImageTypesArray();
        if (!allowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            return false;

        // Validate it's actually an image by trying to load it
        try
        {
            using var stream = file.OpenReadStream();
            var info = await ImageSharpImage.IdentifyAsync(stream);
            return info != null;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> GenerateThumbnailAsync(
        ImageSharpImage sourceImage,
        int size,
        string baseFileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GenerateThumbnailAsync called. Size: {Size}, BaseFileName: {BaseFileName}, ContentType: {ContentType}", 
            size, baseFileName, contentType);
        
        // Determine extension based on content type
        var extension = contentType.ToLowerInvariant() switch
        {
            "image/webp" => ".webp",
            "image/png" => ".png",
            _ => ".jpg"
        };
        
        var thumbnailFileName = $"{baseFileName}_{size}x{size}{extension}";
        var thumbnailPath = Path.Combine(_uploadPath, thumbnailFileName);

        _logger.LogInformation("Creating thumbnail at: {Path}", thumbnailPath);

        using var thumbnail = sourceImage.Clone(x => x.Resize(new ResizeOptions
        {
            Size = new Size(size, size),
            Mode = ResizeMode.Crop,
            Position = AnchorPositionMode.Center
        }));

        _logger.LogInformation("Thumbnail image cloned and resized to {Size}x{Size}", size, size);

        var savedFileName = await SaveOptimizedImageAsync(thumbnail, thumbnailPath, contentType, cancellationToken);
        
        _logger.LogInformation("Thumbnail saved as: {FileName}", savedFileName);
        
        return savedFileName;
    }

    private async Task<string> SaveOptimizedImageAsync(
        ImageSharpImage image,
        string path,
        string contentType,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("SaveOptimizedImageAsync called. Path: {Path}, ContentType: {ContentType}", path, contentType);
        
        if (contentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Saving as WebP: {Path}", path);
            await image.SaveAsync(path, new WebpEncoder { Quality = 90 }, cancellationToken);
            var exists = File.Exists(path);
            _logger.LogInformation("WebP saved. File exists: {Exists}, Size: {Size}", exists, exists ? new FileInfo(path).Length : 0);
            return Path.GetFileName(path);
        }
        else if (contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Saving as PNG: {Path}", path);
            // Save as PNG first
            await image.SaveAsync(path, cancellationToken);
            
            var exists = File.Exists(path);
            var fileInfo = new FileInfo(path);
            _logger.LogInformation("PNG saved. File exists: {Exists}, Size: {Size}", exists, exists ? fileInfo.Length : 0);
            
            // Check if file is too large and convert to JPEG if needed
            if (fileInfo.Exists && fileInfo.Length > 1048576)
            {
                _logger.LogInformation("PNG is large ({Size} bytes), converting to JPEG", fileInfo.Length);
                var jpegPath = Path.ChangeExtension(path, ".jpg");
                await image.SaveAsync(jpegPath, new JpegEncoder { Quality = 85 }, cancellationToken);
                _logger.LogInformation("JPEG saved: {Path}, Size: {Size}", jpegPath, new FileInfo(jpegPath).Length);
                // Delete the original PNG
                File.Delete(path);
                _logger.LogInformation("Original PNG deleted: {Path}", path);
                return Path.GetFileName(jpegPath);
            }
            return Path.GetFileName(path);
        }
        else
        {
            _logger.LogInformation("Converting to JPEG: {Path}", path);
            // Default to JPEG with 85% quality for all other formats
            // Force .jpg extension
            var jpegPath = Path.ChangeExtension(path, ".jpg");
            await image.SaveAsync(jpegPath, new JpegEncoder { Quality = 85 }, cancellationToken);
            var exists = File.Exists(jpegPath);
            _logger.LogInformation("JPEG saved: {Path}, File exists: {Exists}, Size: {Size}", 
                jpegPath, exists, exists ? new FileInfo(jpegPath).Length : 0);
            return Path.GetFileName(jpegPath);
        }
    }
}
