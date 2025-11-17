namespace WebSpark.ArtSpark.Demo.Options;

public class AuditLogOptions
{
    public const string SectionName = "AuditLog";
    
    public int RetentionDays { get; set; } = 365;
    public int CleanupIntervalHours { get; set; } = 24;
}
