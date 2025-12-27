namespace WebSpark.ArtSpark.Demo.Options;

public class FileUploadOptions
{
    public const string SectionName = "FileUpload";

    public long MaxProfilePhotoSize { get; set; } = 5242880; // 5MB default
    public string AllowedImageTypes { get; set; } = "image/jpeg,image/png,image/webp";
    public string ProfilePhotoPath { get; set; } = "wwwroot/uploads/profiles";
    public string CollectionMediaPath { get; set; } = "wwwroot/uploads/collections";
    public int[] ThumbnailSizes { get; set; } = [64, 128, 256];
    public int DiskUsageThresholdMB { get; set; } = 5120; // 5GB default

    public string[] GetAllowedImageTypesArray() =>
        AllowedImageTypes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
