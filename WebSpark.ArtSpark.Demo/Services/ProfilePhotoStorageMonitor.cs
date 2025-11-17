using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using WebSpark.ArtSpark.Demo.Options;

namespace WebSpark.ArtSpark.Demo.Services;

public class ProfilePhotoStorageMonitor : BackgroundService
{
    private readonly ILogger<ProfilePhotoStorageMonitor> _logger;
    private readonly FileUploadOptions _options;
    private readonly string _uploadPath;

    public ProfilePhotoStorageMonitor(
        ILogger<ProfilePhotoStorageMonitor> logger,
        IOptions<FileUploadOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _uploadPath = Path.GetFullPath(_options.ProfilePhotoPath);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Profile Photo Storage Monitor started. Threshold: {ThresholdMB} MB",
            _options.DiskUsageThresholdMB);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                await CheckStorageUsageAsync();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Profile Photo Storage Monitor is stopping.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during storage monitoring");
            }
        }
    }

    private async Task CheckStorageUsageAsync()
    {
        try
        {
            if (!Directory.Exists(_uploadPath))
                return;

            var files = Directory.GetFiles(_uploadPath, "*", SearchOption.AllDirectories);
            long totalBytes = 0;

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                totalBytes += fileInfo.Length;
            }

            var totalMB = totalBytes / 1048576.0;
            var thresholdMB = _options.DiskUsageThresholdMB;
            var percentageUsed = (totalMB / thresholdMB) * 100;

            if (percentageUsed >= 80)
            {
                _logger.LogWarning(
                    "Profile photo storage usage is at {Percentage:F1}% ({CurrentMB:F1} MB / {ThresholdMB} MB). " +
                    "Consider reviewing and cleaning up old photos.",
                    percentageUsed, totalMB, thresholdMB);
            }
            else
            {
                _logger.LogInformation(
                    "Profile photo storage: {CurrentMB:F1} MB / {ThresholdMB} MB ({Percentage:F1}%)",
                    totalMB, thresholdMB, percentageUsed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking storage usage");
        }
    }
}

public class ProfilePhotoStorageHealthCheck : IHealthCheck
{
    private readonly FileUploadOptions _options;
    private readonly ILogger<ProfilePhotoStorageHealthCheck> _logger;
    private readonly string _uploadPath;

    public ProfilePhotoStorageHealthCheck(
        IOptions<FileUploadOptions> options,
        ILogger<ProfilePhotoStorageHealthCheck> logger)
    {
        _options = options.Value;
        _logger = logger;
        _uploadPath = Path.GetFullPath(_options.ProfilePhotoPath);
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Directory.Exists(_uploadPath))
            {
                return HealthCheckResult.Degraded("Profile photo upload directory does not exist");
            }

            var files = Directory.GetFiles(_uploadPath, "*", SearchOption.AllDirectories);
            long totalBytes = 0;

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                totalBytes += fileInfo.Length;
            }

            var totalMB = totalBytes / 1048576.0;
            var thresholdMB = _options.DiskUsageThresholdMB;
            var percentageUsed = (totalMB / thresholdMB) * 100;

            var data = new Dictionary<string, object>
            {
                ["CurrentUsageMB"] = Math.Round(totalMB, 2),
                ["ThresholdMB"] = thresholdMB,
                ["PercentageUsed"] = Math.Round(percentageUsed, 2),
                ["FileCount"] = files.Length
            };

            if (percentageUsed >= 95)
            {
                return HealthCheckResult.Unhealthy(
                    $"Storage usage critical: {percentageUsed:F1}%", data: data);
            }
            else if (percentageUsed >= 80)
            {
                return HealthCheckResult.Degraded(
                    $"Storage usage high: {percentageUsed:F1}%", data: data);
            }

            return HealthCheckResult.Healthy(
                $"Storage usage normal: {percentageUsed:F1}%", data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return HealthCheckResult.Unhealthy("Failed to check storage", ex);
        }
    }
}
