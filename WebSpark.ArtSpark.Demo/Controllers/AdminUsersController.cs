using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSpark.ArtSpark.Demo.Services;

namespace WebSpark.ArtSpark.Demo.Controllers;

[Authorize(Roles = "Admin")]
public class AdminUsersController : Controller
{
    private readonly IAdminUserService _adminUserService;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(
        IAdminUserService adminUserService,
        ILogger<AdminUsersController> logger)
    {
        _adminUserService = adminUserService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search = null, int page = 1, int pageSize = 50)
    {
        ViewData["CurrentSearch"] = search;
        ViewData["CurrentPage"] = page;

        var users = await _adminUserService.GetUsersAsync(search, page, pageSize);
        var totalCount = await _adminUserService.GetTotalUsersCountAsync(search);

        ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewData["TotalUsers"] = totalCount;

        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var user = await _adminUserService.GetUserDetailsAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRole(string userId, string roleName)
    {
        var adminUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminUserId))
        {
            return Unauthorized();
        }

        // Check for self-modification guard
        if (userId == adminUserId && roleName == "Admin")
        {
            TempData["ErrorMessage"] = "You cannot modify your own Admin role.";
            return RedirectToAction(nameof(Details), new { id = userId });
        }

        var result = await _adminUserService.AssignRoleAsync(userId, roleName, adminUserId);
        if (result)
        {
            TempData["SuccessMessage"] = $"Role '{roleName}' assigned successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to assign role. You cannot remove the last admin or modify your own admin status.";
        }

        return RedirectToAction(nameof(Details), new { id = userId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveRole(string userId, string roleName)
    {
        var adminUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminUserId))
        {
            return Unauthorized();
        }

        // Check for self-modification guard
        if (userId == adminUserId && roleName == "Admin")
        {
            TempData["ErrorMessage"] = "You cannot remove your own Admin role.";
            return RedirectToAction(nameof(Details), new { id = userId });
        }

        var result = await _adminUserService.RemoveRoleAsync(userId, roleName, adminUserId);
        if (result)
        {
            TempData["SuccessMessage"] = $"Role '{roleName}' removed successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to remove role. You cannot remove the last admin or modify your own admin status.";
        }

        return RedirectToAction(nameof(Details), new { id = userId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLockout(string userId, bool lockout)
    {
        var adminUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminUserId))
        {
            return Unauthorized();
        }

        var result = await _adminUserService.ToggleLockoutAsync(userId, lockout, adminUserId);
        if (result)
        {
            TempData["SuccessMessage"] = lockout ? "User locked successfully." : "User unlocked successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to toggle user lockout status.";
        }

        return RedirectToAction(nameof(Details), new { id = userId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveCollection(int collectionId, string fromUserId, string toUserId)
    {
        var adminUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminUserId))
        {
            return Unauthorized();
        }

        if (string.IsNullOrEmpty(toUserId))
        {
            TempData["ErrorMessage"] = "Target user ID is required.";
            return RedirectToAction(nameof(Details), new { id = fromUserId });
        }

        var result = await _adminUserService.MoveCollectionAsync(collectionId, fromUserId, toUserId, adminUserId);
        if (result)
        {
            TempData["SuccessMessage"] = "Collection moved successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to move collection. Please verify the collection and target user exist.";
        }

        return RedirectToAction(nameof(Details), new { id = fromUserId });
    }
}
