using WebSpark.ArtSpark.Agent.Personas;

namespace WebSpark.ArtSpark.Agent.Configuration;

/// <summary>
/// Configuration for a specific persona's prompt loading and token validation.
/// </summary>
public sealed class PersonaPromptConfiguration
{
    /// <summary>
    /// The persona type this configuration applies to.
    /// </summary>
    public ChatPersona PersonaType { get; init; }

    /// <summary>
    /// File name for the persona prompt (e.g., "artspark.artwork.prompt.md").
    /// </summary>
    public string PromptFileName { get; init; } = string.Empty;

    /// <summary>
    /// Hardcoded fallback prompt when file loading fails.
    /// </summary>
    public string FallbackPrompt { get; init; } = string.Empty;

    /// <summary>
    /// Whitelist of allowed template tokens for this persona (e.g., "artwork.Title").
    /// </summary>
    public HashSet<string> AllowedTokens { get; init; } = new();

    /// <summary>
    /// Persona-specific metadata defaults.
    /// </summary>
    public PromptMetadata? DefaultMetadata { get; init; }

    /// <summary>
    /// Creates persona configuration map for all four personas.
    /// </summary>
    public static IReadOnlyDictionary<ChatPersona, PersonaPromptConfiguration> CreateDefaultMap()
    {
        return new Dictionary<ChatPersona, PersonaPromptConfiguration>
        {
            [ChatPersona.Artwork] = new()
            {
                PersonaType = ChatPersona.Artwork,
                PromptFileName = "artspark.artwork.prompt.md",
                FallbackPrompt = ArtworkPersona.DefaultSystemPrompt,
                AllowedTokens = new HashSet<string>
                {
                    "artwork.Title",
                    "artwork.ArtistDisplay",
                    "artwork.DateDisplay",
                    "artwork.Medium",
                    "artwork.Dimensions",
                    "artwork.StyleTitle",
                    "artwork.Classification",
                    "artwork.PlaceOfOrigin",
                    "artwork.CulturalContext",
                    "artwork.Description"
                }
            },
            [ChatPersona.Artist] = new()
            {
                PersonaType = ChatPersona.Artist,
                PromptFileName = "artspark.artist.prompt.md",
                FallbackPrompt = ArtistPersona.DefaultSystemPrompt,
                AllowedTokens = new HashSet<string>
                {
                    "artwork.ArtistDisplay",
                    "artwork.Title",
                    "artwork.DateDisplay",
                    "artwork.PlaceOfOrigin",
                    "artwork.Medium",
                    "artwork.CulturalContext"
                }
            },
            [ChatPersona.Curator] = new()
            {
                PersonaType = ChatPersona.Curator,
                PromptFileName = "artspark.curator.prompt.md",
                FallbackPrompt = CuratorPersona.DefaultSystemPrompt,
                AllowedTokens = new HashSet<string>
                {
                    "artwork.Title",
                    "artwork.ArtistDisplay",
                    "artwork.DateDisplay",
                    "artwork.PlaceOfOrigin",
                    "artwork.CulturalContext",
                    "artwork.Description"
                }
            },
            [ChatPersona.Historian] = new()
            {
                PersonaType = ChatPersona.Historian,
                PromptFileName = "artspark.historian.prompt.md",
                FallbackPrompt = HistorianPersona.DefaultSystemPrompt,
                AllowedTokens = new HashSet<string>
                {
                    "artwork.Title",
                    "artwork.ArtistDisplay",
                    "artwork.DateDisplay",
                    "artwork.StyleTitle",
                    "artwork.PlaceOfOrigin",
                    "artwork.Classification",
                    "artwork.CulturalContext"
                }
            }
        };
    }
}
