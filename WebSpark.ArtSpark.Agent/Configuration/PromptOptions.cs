namespace WebSpark.ArtSpark.Agent.Configuration;

/// <summary>
/// Configuration options for the prompt loading system.
/// </summary>
public sealed class PromptOptions
{
    /// <summary>
    /// Root-relative or absolute path to the persona prompt markdown directory.
    /// </summary>
    public string DataPath { get; set; } = "./prompts/agents";

    /// <summary>
    /// When true, monitor prompt files for changes using PhysicalFileProvider.
    /// </summary>
    public bool EnableHotReload { get; set; } = false;

    /// <summary>
    /// When true, fall back to hardcoded prompts if the file system prompt fails validation.
    /// </summary>
    public bool FallbackToDefault { get; set; } = true;

    /// <summary>
    /// Optional directory that contains alternate prompt variants (used in development/testing).
    /// </summary>
    public string? VariantsPath { get; set; }

    /// <summary>
    /// Baseline metadata applied to each persona unless overridden in front matter.
    /// </summary>
    public PromptMetadata DefaultMetadata { get; set; } = new();
}

/// <summary>
/// Metadata controlling AI model behavior for persona prompts.
/// </summary>
public sealed class PromptMetadata
{
    /// <summary>
    /// OpenAI model identifier (e.g., "gpt-4o").
    /// </summary>
    public string ModelId { get; set; } = "gpt-4o";

    /// <summary>
    /// Sampling temperature (0.0 to 1.0).
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Top-p nucleus sampling parameter (0.0 to 1.0).
    /// </summary>
    public double? TopP { get; set; }

    /// <summary>
    /// Maximum tokens in the response.
    /// </summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// Frequency penalty (-2.0 to 2.0).
    /// </summary>
    public double? FrequencyPenalty { get; set; }

    /// <summary>
    /// Presence penalty (-2.0 to 2.0).
    /// </summary>
    public double? PresencePenalty { get; set; }

    /// <summary>
    /// Merges this metadata with overrides, preferring override values when present.
    /// </summary>
    public PromptMetadata MergeWith(PromptMetadata? overrides)
    {
        if (overrides is null) return this;

        return new PromptMetadata
        {
            ModelId = overrides.ModelId ?? ModelId,
            Temperature = overrides.Temperature,
            TopP = overrides.TopP ?? TopP,
            MaxOutputTokens = overrides.MaxOutputTokens ?? MaxOutputTokens,
            FrequencyPenalty = overrides.FrequencyPenalty ?? FrequencyPenalty,
            PresencePenalty = overrides.PresencePenalty ?? PresencePenalty
        };
    }
}
