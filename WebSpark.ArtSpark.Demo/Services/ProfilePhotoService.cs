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
            // Validate file
            if (!await ValidatePhotoAsync(file))
            {
                return new ProfilePhotoUploadResult
                {
                    Success = false,
                    ErrorMessage = "Invalid file. Please upload a JPEG, PNG, or WebP image under 5MB."
                };
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var safeFileName = $"{userId}_{Guid.NewGuid()}{fileExtension}";
            var originalPath = Path.Combine(_uploadPath, safeFileName);

            // Load and process image
            using var stream = file.OpenReadStream();
            using var image = await ImageSharpImage.LoadAsync(stream, cancellationToken);

            // Auto-orient based on EXIF data
            image.Mutate(x => x.AutoOrient());

            // Strip metadata for privacy
            image.Metadata.ExifProfile = null;
            image.Metadata.IptcProfile = null;
            image.Metadata.XmpProfile = null;

            // Save original (optimized)
            await SaveOptimizedImageAsync(image, originalPath, file.ContentType, cancellationToken);

            // Generate thumbnails
            var thumbnail64 = await GenerateThumbnailAsync(image, 64, safeFileName, file.ContentType, cancellationToken);
            var thumbnail128 = await GenerateThumbnailAsync(image, 128, safeFileName, file.ContentType, cancellationToken);
            var thumbnail256 = await GenerateThumbnailAsync(image, 256, safeFileName, file.ContentType, cancellationToken);

            _logger.LogInformation(
                "Profile photo uploaded successfully for user {UserId}. File: {FileName}, Size: {FileSize} bytes",
                userId, safeFileName, file.Length);

            return new ProfilePhotoUploadResult
            {
                Success = true,
                FileName = safeFileName,
                Thumbnail64 = thumbnail64,
                Thumbnail128 = thumbnail128,
                Thumbnail256 = thumbnail256
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile photo for user {UserId}", userId);
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
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var baseName = Path.GetFileNameWithoutExtension(originalFileName);
        var extension = Path.GetExtension(originalFileName);
        var thumbnailFileName = $"{baseName}_{size}x{size}{extension}";
        var thumbnailPath = Path.Combine(_uploadPath, thumbnailFileName);

        using var thumbnail = sourceImage.Clone(x => x.Resize(new ResizeOptions
        {
            Size = new Size(size, size),
            Mode = ResizeMode.Crop,
            Position = AnchorPositionMode.Center
        }));

        await SaveOptimizedImageAsync(thumbnail, thumbnailPath, contentType, cancellationToken);
        return thumbnailFileName;
    }

    private async Task SaveOptimizedImageAsync(
        ImageSharpImage image,
        string path,
        string contentType,
        CancellationToken cancellationToken)
    {
        if (contentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase))
        {
            await image.SaveAsync(path, new WebpEncoder { Quality = 90 }, cancellationToken);
        }
        else if (contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase) && new FileInfo(path).Length > 1048576)
        {
            // Convert large PNGs to JPEG
            var jpegPath = Path.ChangeExtension(path, ".jpg");
            await image.SaveAsync(jpegPath, new JpegEncoder { Quality = 85 }, cancellationToken);
        }
        else
        {
            // Default to JPEG with 85% quality
            await image.SaveAsync(path, new JpegEncoder { Quality = 85 }, cancellationToken);
        }
    }
}
