using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebSpark.ArtSpark.Agent.Configuration;
using WebSpark.ArtSpark.Agent.Interfaces;
using WebSpark.ArtSpark.Agent.Models;
using WebSpark.ArtSpark.Agent.Personas;
using WebSpark.ArtSpark.Agent.Services;

namespace WebSpark.ArtSpark.Agent.Personas;

/// <summary>
/// Decorator that wraps an IPersonaHandler to use file-backed prompts when available.
/// </summary>
public sealed class FileBackedPersonaHandler : IPersonaHandler
{
    private readonly IPersonaHandler _inner;
    private readonly IPromptLoader _promptLoader;
    private readonly PromptOptions _options;
    private readonly ILogger<FileBackedPersonaHandler> _logger;

    public FileBackedPersonaHandler(
        IPersonaHandler inner,
        IPromptLoader promptLoader,
        IOptions<PromptOptions> options,
        ILogger<FileBackedPersonaHandler> logger)
    {
        _inner = inner;
        _promptLoader = promptLoader;
        _options = options.Value;
        _logger = logger;
    }

    public ChatPersona PersonaType => _inner.PersonaType;

    public string GenerateSystemPrompt(ArtworkData artwork)
    {
        try
        {
            var template = _promptLoader.GetPromptAsync(PersonaType).GetAwaiter().GetResult();

            if (template.IsFallback || !_options.FallbackToDefault)
            {
                return _inner.GenerateSystemPrompt(artwork);
            }

            // Replace tokens in the prompt template
            var prompt = ReplaceTokens(template.Content, artwork);
            return prompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error generating file-backed prompt for {PersonaType}, using fallback",
                PersonaType);
            return _inner.GenerateSystemPrompt(artwork);
        }
    }

    public Task<string> ProcessMessageAsync(string message, ArtworkData artwork, List<ChatMessage> history)
    {
        return _inner.ProcessMessageAsync(message, artwork, history);
    }

    public Task<List<string>> GenerateConversationStartersAsync(ArtworkData artwork)
    {
        return _inner.GenerateConversationStartersAsync(artwork);
    }

    private static string ReplaceTokens(string template, ArtworkData artwork)
    {
        // Simple token replacement using string.Replace for allowed tokens
        var result = template
            .Replace("{artwork.Title}", artwork.Title ?? string.Empty)
            .Replace("{artwork.ArtistDisplay}", artwork.ArtistDisplay ?? string.Empty)
            .Replace("{artwork.DateDisplay}", artwork.DateDisplay ?? string.Empty)
            .Replace("{artwork.Medium}", artwork.Medium ?? string.Empty)
            .Replace("{artwork.Dimensions}", artwork.Dimensions ?? string.Empty)
            .Replace("{artwork.StyleTitle}", artwork.StyleTitle ?? string.Empty)
            .Replace("{artwork.Classification}", artwork.Classification ?? string.Empty)
            .Replace("{artwork.PlaceOfOrigin}", artwork.PlaceOfOrigin ?? string.Empty)
            .Replace("{artwork.CulturalContext}", artwork.CulturalContext ?? string.Empty)
            .Replace("{artwork.Description}", artwork.Description ?? string.Empty);

        return result;
    }
}
