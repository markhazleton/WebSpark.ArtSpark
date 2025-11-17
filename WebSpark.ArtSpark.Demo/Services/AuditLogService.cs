using WebSpark.ArtSpark.Demo.Data;
using WebSpark.ArtSpark.Demo.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace WebSpark.ArtSpark.Demo.Services;

public interface IAuditLogService
{
    Task LogActionAsync(string actionType, string adminUserId, string? targetUserId = null, object? details = null, CancellationToken cancellationToken = default);
    Task<List<AuditLog>> GetUserAuditLogsAsync(string userId, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<List<AuditLog>> GetAdminActionLogsAsync(string adminUserId, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
}

public class AuditLogService : IAuditLogService
{
    private readonly ArtSparkDbContext _context;
    private readonly ILogger<AuditLogService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(
        ArtSparkDbContext context,
        ILogger<AuditLogService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogActionAsync(
        string actionType,
        string adminUserId,
        string? targetUserId = null,
        object? details = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier;

            var detailsJson = details != null
                ? JsonSerializer.Serialize(details, new JsonSerializerOptions { WriteIndented = false })
                : null;

            // Truncate if too large
            if (detailsJson?.Length > 4000)
            {
                detailsJson = detailsJson.Substring(0, 3997) + "...";
                _logger.LogWarning("Audit log details truncated for action {ActionType}", actionType);
            }

            var auditLog = new AuditLog
            {
                ActionType = actionType,
                AdminUserId = adminUserId,
                TargetUserId = targetUserId,
                Details = detailsJson,
                CorrelationId = correlationId,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Audit log created: {ActionType} by {AdminUserId} on {TargetUserId} (Correlation: {CorrelationId})",
                actionType, adminUserId, targetUserId ?? "N/A", correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log for action {ActionType}", actionType);
            // Don't throw - audit logging failure shouldn't break the main operation
        }
    }

    public async Task<List<AuditLog>> GetUserAuditLogsAsync(
        string userId,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.TargetUserId == userId)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(a => a.AdminUser)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AuditLog>> GetAdminActionLogsAsync(
        string adminUserId,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.AdminUserId == adminUserId)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(a => a.TargetUser)
            .ToListAsync(cancellationToken);
    }
}
