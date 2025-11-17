namespace WebSpark.ArtSpark.Demo.Services;

public interface IPasswordStrengthService
{
    PasswordStrengthResult CheckStrength(string password);
    bool IsCommonPassword(string password);
}

public class PasswordStrengthService : IPasswordStrengthService
{
    private readonly HashSet<string> _commonPasswords;
    private readonly ILogger<PasswordStrengthService> _logger;

    public PasswordStrengthService(ILogger<PasswordStrengthService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _commonPasswords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        // Load common passwords from file
        var filePath = Path.Combine(environment.ContentRootPath, "Data", "common-passwords.txt");
        if (File.Exists(filePath))
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        _commonPasswords.Add(line.Trim());
                    }
                }
                _logger.LogInformation("Loaded {Count} common passwords for validation", _commonPasswords.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load common passwords list");
            }
        }
        else
        {
            _logger.LogWarning("Common passwords file not found at {Path}", filePath);
        }
    }

    public PasswordStrengthResult CheckStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return new PasswordStrengthResult
            {
                Score = 0,
                Strength = PasswordStrength.Weak,
                Feedback = new List<string> { "Password is required" }
            };
        }

        var score = 0;
        var feedback = new List<string>();

        // Length check
        if (password.Length >= 8) score += 20;
        else feedback.Add("Password should be at least 8 characters long");

        if (password.Length >= 12) score += 10;
        if (password.Length >= 16) score += 10;

        // Character variety
        if (password.Any(char.IsLower)) score += 15;
        else feedback.Add("Include lowercase letters");

        if (password.Any(char.IsUpper)) score += 15;
        else feedback.Add("Include uppercase letters");

        if (password.Any(char.IsDigit)) score += 15;
        else feedback.Add("Include numbers");

        if (password.Any(c => !char.IsLetterOrDigit(c))) score += 15;
        else feedback.Add("Include special characters (!@#$%^&*)");

        // Common password check
        if (IsCommonPassword(password))
        {
            score = Math.Max(0, score - 50);
            feedback.Add("This is a commonly used password");
        }

        // Determine strength
        var strength = score switch
        {
            >= 80 => PasswordStrength.Strong,
            >= 60 => PasswordStrength.Medium,
            _ => PasswordStrength.Weak
        };

        return new PasswordStrengthResult
        {
            Score = score,
            Strength = strength,
            Feedback = feedback
        };
    }

    public bool IsCommonPassword(string password)
    {
        return _commonPasswords.Contains(password);
    }
}

public class PasswordStrengthResult
{
    public int Score { get; set; }
    public PasswordStrength Strength { get; set; }
    public List<string> Feedback { get; set; } = new();
}

public enum PasswordStrength
{
    Weak,
    Medium,
    Strong
}
