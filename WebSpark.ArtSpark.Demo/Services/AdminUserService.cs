using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebSpark.ArtSpark.Demo.Data;
using WebSpark.ArtSpark.Demo.Models;

namespace WebSpark.ArtSpark.Demo.Services;

public interface IAdminUserService
{
    Task<List<UserSummaryDto>> GetUsersAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<UserDetailDto?> GetUserDetailsAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> AssignRoleAsync(string userId, string roleName, string adminUserId, CancellationToken cancellationToken = default);
    Task<bool> RemoveRoleAsync(string userId, string roleName, string adminUserId, CancellationToken cancellationToken = default);
    Task<bool> ToggleLockoutAsync(string userId, bool lockout, string adminUserId, CancellationToken cancellationToken = default);
    Task<int> GetTotalUsersCountAsync(string? searchTerm = null, CancellationToken cancellationToken = default);
    Task<List<AdminUserCollectionDto>> GetUserCollectionsAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> MoveCollectionAsync(int collectionId, string fromUserId, string toUserId, string adminUserId, CancellationToken cancellationToken = default);
}

public class AdminUserService : IAdminUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ArtSparkDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AdminUserService> _logger;

    public AdminUserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ArtSparkDbContext context,
        IAuditLogService auditLogService,
        ILogger<AdminUserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<List<UserSummaryDto>> GetUsersAsync(
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearch = searchTerm.ToLower();
            query = query.Where(u =>
                u.Email!.ToLower().Contains(normalizedSearch) ||
                u.DisplayName.ToLower().Contains(normalizedSearch));
        }

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserSummaryDto
            {
                UserId = u.Id,
                Email = u.Email!,
                DisplayName = u.DisplayName,
                CreatedAt = u.CreatedAt,
                EmailVerified = u.EmailVerified,
                ProfilePhotoUrl = u.ProfilePhotoThumbnail64,
                LockoutEnd = u.LockoutEnd
            })
            .ToListAsync(cancellationToken);

        // Load roles for each user (separate queries due to EF limitations)
        foreach (var user in users)
        {
            var appUser = await _userManager.FindByIdAsync(user.UserId);
            if (appUser != null)
            {
                user.Roles = (await _userManager.GetRolesAsync(appUser)).ToList();
            }
        }

        return users;
    }

    public async Task<int> GetTotalUsersCountAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearch = searchTerm.ToLower();
            query = query.Where(u =>
                u.Email!.ToLower().Contains(normalizedSearch) ||
                u.DisplayName.ToLower().Contains(normalizedSearch));
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<UserDetailDto?> GetUserDetailsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Collections)
            .Include(u => u.Reviews)
            .Include(u => u.Favorites)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return null;

        var appUser = await _userManager.FindByIdAsync(userId);
        var roles = appUser != null ? await _userManager.GetRolesAsync(appUser) : new List<string>();

        var auditLogs = await _auditLogService.GetUserAuditLogsAsync(userId, 1, 20, cancellationToken);

        return new UserDetailDto
        {
            UserId = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            Bio = user.Bio,
            CreatedAt = user.CreatedAt,
            EmailVerified = user.EmailVerified,
            ProfilePhotoUrl = user.ProfilePhotoThumbnail128,
            Roles = roles.ToList(),
            CollectionsCount = user.Collections.Count,
            ReviewsCount = user.Reviews.Count,
            FavoritesCount = user.Favorites.Count,
            LockoutEnd = user.LockoutEnd,
            RecentAuditLogs = auditLogs,
            Collections = user.Collections.Select(c => new AdminUserCollectionDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsPublic = c.IsPublic,
                CreatedAt = c.CreatedAt,
                ArtworkCount = c.Artworks.Count,
                ViewCount = c.ViewCount,
                IsFeatured = c.IsFeatured
            }).ToList()
        };
    }

    public async Task<bool> AssignRoleAsync(string userId, string roleName, string adminUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for role assignment", userId);
                return false;
            }

            // Prevent self-demotion
            if (userId == adminUserId && roleName == "Admin")
            {
                _logger.LogWarning("Admin {AdminId} attempted to modify their own Admin role", adminUserId);
                return false;
            }

            // Ensure at least one admin exists
            if (roleName == "Admin")
            {
                var adminsInRole = await _userManager.GetUsersInRoleAsync("Admin");
                if (adminsInRole.Count == 1 && adminsInRole.Any(a => a.Id == userId))
                {
                    _logger.LogWarning("Cannot remove last admin. User {UserId} is the only admin.", userId);
                    return false;
                }
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                await _auditLogService.LogActionAsync("RoleAssigned", adminUserId, userId, new { Role = roleName });
                _logger.LogInformation("Role {Role} assigned to user {UserId} by admin {AdminId}", roleName, userId, adminUserId);
                return true;
            }

            _logger.LogWarning("Failed to assign role {Role} to user {UserId}: {Errors}",
                roleName, userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {Role} to user {UserId}", roleName, userId);
            return false;
        }
    }

    public async Task<bool> RemoveRoleAsync(string userId, string roleName, string adminUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for role removal", userId);
                return false;
            }

            // Prevent self-demotion from Admin
            if (userId == adminUserId && roleName == "Admin")
            {
                _logger.LogWarning("Admin {AdminId} attempted to remove their own Admin role", adminUserId);
                return false;
            }

            // Ensure at least one admin remains
            if (roleName == "Admin")
            {
                var adminsInRole = await _userManager.GetUsersInRoleAsync("Admin");
                if (adminsInRole.Count == 1 && adminsInRole.Any(a => a.Id == userId))
                {
                    _logger.LogWarning("Cannot remove Admin role from user {UserId} - they are the last admin", userId);
                    return false;
                }
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                await _auditLogService.LogActionAsync("RoleRevoked", adminUserId, userId, new { Role = roleName });
                _logger.LogInformation("Role {Role} removed from user {UserId} by admin {AdminId}", roleName, userId, adminUserId);
                return true;
            }

            _logger.LogWarning("Failed to remove role {Role} from user {UserId}: {Errors}",
                roleName, userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {Role} from user {UserId}", roleName, userId);
            return false;
        }
    }

    public async Task<bool> ToggleLockoutAsync(string userId, bool lockout, string adminUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for lockout toggle", userId);
                return false;
            }

            if (lockout)
            {
                var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                if (result.Succeeded)
                {
                    await _auditLogService.LogActionAsync("UserLocked", adminUserId, userId);
                    _logger.LogInformation("User {UserId} locked by admin {AdminId}", userId, adminUserId);
                    return true;
                }
            }
            else
            {
                var result = await _userManager.SetLockoutEndDateAsync(user, null);
                if (result.Succeeded)
                {
                    await _auditLogService.LogActionAsync("UserUnlocked", adminUserId, userId);
                    _logger.LogInformation("User {UserId} unlocked by admin {AdminId}", userId, adminUserId);
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling lockout for user {UserId}", userId);
            return false;
        }
    }

    public async Task<List<AdminUserCollectionDto>> GetUserCollectionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Collections
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new AdminUserCollectionDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsPublic = c.IsPublic,
                CreatedAt = c.CreatedAt,
                ArtworkCount = c.Artworks.Count,
                ViewCount = c.ViewCount,
                IsFeatured = c.IsFeatured
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> MoveCollectionAsync(int collectionId, string fromUserId, string toUserId, string adminUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = await _context.Collections
                .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == fromUserId, cancellationToken);

            if (collection == null)
            {
                _logger.LogWarning("Collection {CollectionId} not found or does not belong to user {FromUserId}", collectionId, fromUserId);
                return false;
            }

            var targetUser = await _userManager.FindByIdAsync(toUserId);
            if (targetUser == null)
            {
                _logger.LogWarning("Target user {ToUserId} not found", toUserId);
                return false;
            }

            var oldUserId = collection.UserId;
            collection.UserId = toUserId;
            collection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _auditLogService.LogActionAsync(
                "CollectionMoved",
                adminUserId,
                null,
                new { CollectionId = collectionId, CollectionName = collection.Name, FromUserId = oldUserId, ToUserId = toUserId });

            _logger.LogInformation(
                "Collection {CollectionId} moved from user {FromUserId} to user {ToUserId} by admin {AdminId}",
                collectionId, oldUserId, toUserId, adminUserId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving collection {CollectionId} from user {FromUserId} to user {ToUserId}",
                collectionId, fromUserId, toUserId);
            return false;
        }
    }
}

public class UserSummaryDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool EmailVerified { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTimeOffset? LockoutEnd { get; set; }
}

public class UserDetailDto : UserSummaryDto
{
    public string? Bio { get; set; }
    public int CollectionsCount { get; set; }
    public int ReviewsCount { get; set; }
    public int FavoritesCount { get; set; }
    public List<AuditLog> RecentAuditLogs { get; set; } = new();
    public List<AdminUserCollectionDto> Collections { get; set; } = new();
}

public class AdminUserCollectionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ArtworkCount { get; set; }
    public int ViewCount { get; set; }
    public bool IsFeatured { get; set; }
}
