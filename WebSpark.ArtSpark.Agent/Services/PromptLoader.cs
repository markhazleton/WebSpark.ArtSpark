using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using WebSpark.ArtSpark.Agent.Configuration;
using WebSpark.ArtSpark.Agent.Models;
using WebSpark.ArtSpark.Agent.Personas;

namespace WebSpark.ArtSpark.Agent.Services;

public interface IPromptLoader
{
    ValueTask<PromptTemplate> GetPromptAsync(ChatPersona personaType, CancellationToken cancellationToken = default);
}

/// <summary>
/// Loads persona prompts from markdown files with YAML front matter, token validation, and hot reload support.
/// </summary>
public sealed class PromptLoader : IPromptLoader, IDisposable
{
    private readonly PromptOptions _options;
    private readonly IReadOnlyDictionary<ChatPersona, PersonaPromptConfiguration> _personaConfigs;
    private readonly PromptMetadataParser _metadataParser;
    private readonly ILogger<PromptLoader> _logger;
    private readonly ConcurrentDictionary<ChatPersona, PromptTemplate> _cache = new();
    private readonly PhysicalFileProvider? _fileProvider;
    private readonly ConcurrentDictionary<ChatPersona, IDisposable?> _changeTokens = new();

    private static readonly Regex TokenPattern = new(@"\{(\w+\.\w+)\}", RegexOptions.Compiled);
    private const long MaxFileSizeBytes = 100 * 1024; // 100 KB

    public PromptLoader(
        IOptions<PromptOptions> options,
        ILogger<PromptLoader> logger)
    {
        _options = options.Value;
        _metadataParser = new PromptMetadataParser();
        _logger = logger;
        _personaConfigs = PersonaPromptConfiguration.CreateDefaultMap();

        // Validate data path exists at startup (FR-009, T010a)
        if (!string.IsNullOrEmpty(_options.DataPath))
        {
            var absolutePath = Path.GetFullPath(_options.DataPath);
            if (!Directory.Exists(absolutePath))
            {
                _logger.LogWarning(
                    "Prompt data path does not exist: {DataPath}. Will use fallback prompts.",
                    absolutePath);
            }
        }

        if (_options.EnableHotReload && !string.IsNullOrEmpty(_options.DataPath))
        {
            var absolutePath = Path.GetFullPath(_options.DataPath);
            if (Directory.Exists(absolutePath))
            {
                _fileProvider = new PhysicalFileProvider(absolutePath);
            }
        }
    }

    public async ValueTask<PromptTemplate> GetPromptAsync(
        ChatPersona personaType,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(personaType, out var cached))
        {
            return cached;
        }

        var template = await LoadPromptAsync(personaType, cancellationToken);
        _cache[personaType] = template;

        if (_fileProvider is not null && !template.IsFallback)
        {
            RegisterHotReload(personaType);
        }

        return template;
    }

    private async Task<PromptTemplate> LoadPromptAsync(
        ChatPersona personaType,
        CancellationToken cancellationToken)
    {
        if (!_personaConfigs.TryGetValue(personaType, out var config))
        {
            _logger.LogError("No configuration found for persona {PersonaType}", personaType);
            return CreateFallbackTemplate(personaType, config?.FallbackPrompt ?? string.Empty);
        }

        var filePath = Path.Combine(Path.GetFullPath(_options.DataPath), config.PromptFileName);

        if (!File.Exists(filePath))
        {
            _logger.LogWarning(
                "Prompt file not found for {PersonaType} at {FilePath}. Using fallback.",
                personaType, filePath);
            return CreateFallbackTemplate(personaType, config.FallbackPrompt);
        }

        try
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > MaxFileSizeBytes)
            {
                _logger.LogWarning(
                    "Prompt file too large for {PersonaType}: {Size} bytes (max {MaxSize}). Using fallback.",
                    personaType, fileInfo.Length, MaxFileSizeBytes);
                return CreateFallbackTemplate(personaType, config.FallbackPrompt);
            }

            var rawContent = await File.ReadAllTextAsync(filePath, cancellationToken);
            var (metadataOverrides, contentBody) = _metadataParser.Parse(rawContent);

            var validationErrors = ValidatePrompt(contentBody, config);
            if (validationErrors.Count > 0)
            {
                _logger.LogError(
                    "Prompt validation failed for {PersonaType} at {FilePath}: {Errors}. Using fallback.",
                    personaType, filePath, string.Join("; ", validationErrors));

                foreach (var error in validationErrors)
                {
                    _logger.LogWarning("PromptTokenValidationFailed: {Error}", error);
                }

                return CreateFallbackTemplate(personaType, config.FallbackPrompt);
            }

            var hash = ComputeHash(contentBody);
            var template = new PromptTemplate
            {
                PersonaType = personaType,
                FilePath = filePath,
                FileName = config.PromptFileName,
                Content = contentBody,
                ContentHash = hash,
                LastModifiedUtc = fileInfo.LastWriteTimeUtc,
                FileSizeBytes = fileInfo.Length,
                IsFallback = false,
                MetadataOverrides = metadataOverrides
            };

            _logger.LogInformation(
                "PromptLoaded: {PersonaType} from {FilePath} ({Size} bytes, hash {Hash})",
                personaType, filePath, fileInfo.Length, hash[..8]);

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "PromptLoadFailed: Error loading prompt for {PersonaType} from {FilePath}. Using fallback.",
                personaType, filePath);
            return CreateFallbackTemplate(personaType, config.FallbackPrompt);
        }
    }

    private List<string> ValidatePrompt(string content, PersonaPromptConfiguration config)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(content))
        {
            errors.Add("Prompt content is empty");
            return errors;
        }

        // Check for required sections
        if (!content.Contains("## CULTURAL SENSITIVITY", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Missing required section: ## CULTURAL SENSITIVITY");
        }

        if (!content.Contains("## CONVERSATION GUIDELINES", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Missing required section: ## CONVERSATION GUIDELINES");
        }

        // Validate tokens against whitelist (strict mode)
        var matches = TokenPattern.Matches(content);
        foreach (Match match in matches)
        {
            var token = match.Groups[1].Value;
            if (!config.AllowedTokens.Contains(token))
            {
                errors.Add($"Invalid token '{{{token}}}' not in whitelist for {config.PersonaType}");
            }
        }

        return errors;
    }

    private PromptTemplate CreateFallbackTemplate(ChatPersona personaType, string fallbackContent)
    {
        _logger.LogWarning("PromptFallbackUsed: Using hardcoded fallback for {PersonaType}", personaType);

        return new PromptTemplate
        {
            PersonaType = personaType,
            FilePath = "[fallback]",
            FileName = "[fallback]",
            Content = fallbackContent,
            ContentHash = ComputeHash(fallbackContent),
            LastModifiedUtc = DateTime.UtcNow,
            FileSizeBytes = Encoding.UTF8.GetByteCount(fallbackContent),
            IsFallback = true,
            MetadataOverrides = null
        };
    }

    private void RegisterHotReload(ChatPersona personaType)
    {
        if (_fileProvider is null || !_personaConfigs.TryGetValue(personaType, out var config))
            return;

        var changeToken = _fileProvider.Watch(config.PromptFileName);
        _changeTokens[personaType] = changeToken.RegisterChangeCallback(_ =>
        {
            _cache.TryRemove(personaType, out PromptTemplate? _);
            _logger.LogInformation(
                "ConfigurationReloaded: Prompt file changed for {PersonaType}, cache invalidated",
                personaType);
            RegisterHotReload(personaType);
        }, null);
    }

    private static string ComputeHash(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = MD5.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public void Dispose()
    {
        foreach (var token in _changeTokens.Values)
        {
            token?.Dispose();
        }
        _changeTokens.Clear();
        _fileProvider?.Dispose();
    }
}
