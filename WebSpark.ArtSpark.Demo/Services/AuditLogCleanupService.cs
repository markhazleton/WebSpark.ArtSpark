using Microsoft.Extensions.Options;
using WebSpark.ArtSpark.Demo.Data;
using WebSpark.ArtSpark.Demo.Options;
using Microsoft.EntityFrameworkCore;

namespace WebSpark.ArtSpark.Demo.Services;

public class AuditLogCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditLogCleanupService> _logger;
    private readonly AuditLogOptions _options;

    public AuditLogCleanupService(
        IServiceProvider serviceProvider,
        ILogger<AuditLogCleanupService> logger,
        IOptions<AuditLogOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audit Log Cleanup Service started. Retention: {Days} days, Interval: {Hours} hours",
            _options.RetentionDays, _options.CleanupIntervalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromHours(_options.CleanupIntervalHours), stoppingToken);
                await CleanupOldLogsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Audit Log Cleanup Service is stopping.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during audit log cleanup");
            }
        }
    }

    private async Task CleanupOldLogsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ArtSparkDbContext>();

        var cutoffDate = DateTime.UtcNow.AddDays(-_options.RetentionDays);
        var deleted = await context.AuditLogs
            .Where(a => a.CreatedAtUtc < cutoffDate)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0)
        {
            _logger.LogInformation("Deleted {Count} audit log entries older than {Date}",
                deleted, cutoffDate.ToString("yyyy-MM-dd"));
        }
    }
}
