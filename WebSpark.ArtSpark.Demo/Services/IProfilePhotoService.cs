namespace WebSpark.ArtSpark.Demo.Services;

public interface IProfilePhotoService
{
    Task<ProfilePhotoUploadResult> UploadPhotoAsync(IFormFile file, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeletePhotoAsync(string userId, CancellationToken cancellationToken = default);
    string? GetPhotoUrl(string? fileName, int? size = null);
    Task<bool> ValidatePhotoAsync(IFormFile file);
}

public class ProfilePhotoUploadResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? FileName { get; set; }
    public string? Thumbnail64 { get; set; }
    public string? Thumbnail128 { get; set; }
    public string? Thumbnail256 { get; set; }
}
