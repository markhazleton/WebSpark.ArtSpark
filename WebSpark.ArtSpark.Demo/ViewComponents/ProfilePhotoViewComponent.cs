using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebSpark.ArtSpark.Demo.Models;
using WebSpark.ArtSpark.Demo.Services;

namespace WebSpark.ArtSpark.Demo.ViewComponents;

public class ProfilePhotoViewComponent : ViewComponent
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IProfilePhotoService _profilePhotoService;

    public ProfilePhotoViewComponent(
        UserManager<ApplicationUser> userManager,
        IProfilePhotoService profilePhotoService)
    {
        _userManager = userManager;
        _profilePhotoService = profilePhotoService;
    }

    public async Task<IViewComponentResult> InvokeAsync(int size = 32, string? cssClass = null)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        
        string? photoUrl = null;
        if (user != null)
        {
            // Select appropriate thumbnail based on requested size
            string? thumbnailFileName = size switch
            {
                <= 64 => user.ProfilePhotoThumbnail64,
                <= 128 => user.ProfilePhotoThumbnail128 ?? user.ProfilePhotoThumbnail64,
                _ => user.ProfilePhotoThumbnail256 ?? user.ProfilePhotoThumbnail128 ?? user.ProfilePhotoThumbnail64
            };

            if (!string.IsNullOrEmpty(thumbnailFileName))
            {
                photoUrl = _profilePhotoService.GetPhotoUrl(thumbnailFileName);
            }
        }
        
        var model = new ProfilePhotoViewModel
        {
            Size = size,
            CssClass = cssClass ?? string.Empty,
            DisplayName = user?.DisplayName ?? "User",
            PhotoUrl = photoUrl,
            Initial = user?.DisplayName?.FirstOrDefault().ToString().ToUpper() ?? "U"
        };

        return View(model);
    }
}

public class ProfilePhotoViewModel
{
    public int Size { get; set; }
    public string CssClass { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string Initial { get; set; } = "U";
}
