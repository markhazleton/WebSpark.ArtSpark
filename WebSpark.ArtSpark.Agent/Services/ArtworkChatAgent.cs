using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Diagnostics;
using WebSpark.ArtSpark.Agent.Configuration;
using WebSpark.ArtSpark.Agent.Interfaces;
using WebSpark.ArtSpark.Agent.Models;
using WebSpark.ArtSpark.Agent.Personas;
namespace WebSpark.ArtSpark.Agent.Services;

/// <summary>
/// AI-powered chat agent that enables conversational interactions about artworks from multiple perspectives.
/// Supports different personas (artwork, artist, curator, historian) and provides vision analysis capabilities.
/// </summary>
/// <remarks>
/// This agent orchestrates conversations by:
/// - Creating persona-specific system prompts and responses
/// - Managing conversation history and memory
/// - Performing visual analysis of artwork images when requested
/// - Generating contextual follow-up questions
/// - Tracking analytics and performance metrics
/// </remarks>
public class ArtworkChatAgent : IArtworkChatAgent
{    /// <summary>
     /// Core kernel for AI operations and dependency injection
     /// </summary>
    private readonly Kernel _kernel;

    /// <summary>
    /// Chat completion service for generating AI responses
    /// </summary>
    private readonly IChatCompletionService _chatService;

    /// <summary>
    /// Factory for creating persona handlers based on chat context
    /// </summary>
    private readonly IPersonaFactory _personaFactory;

    /// <summary>
    /// Provider for retrieving and enriching artwork data
    /// </summary>
    private readonly IArtworkDataProvider _artworkProvider;

    /// <summary>
    /// Memory service for conversation persistence and caching
    /// </summary>
    private readonly IChatMemory _chatMemory;

    /// <summary>
    /// Logger for diagnostic and performance tracking
    /// </summary>
    private readonly ILogger<ArtworkChatAgent> _logger;
    /// <summary>
    /// Configuration settings for the agent behavior
    /// </summary>
    private readonly AgentConfiguration _config;

    /// <summary>
    /// Input validator for implementing guard rails and content filtering
    /// </summary>
    private readonly IChatInputValidator _inputValidator;

    /// <summary>
    /// Initializes a new instance of the ArtworkChatAgent with required dependencies.
    /// </summary>
    /// <param name="kernel">Semantic Kernel instance for AI operations</param>
    /// <param name="personaFactory">Factory for creating persona handlers</param>
    /// <param name="artworkProvider">Provider for artwork data operations</param>
    /// <param name="chatMemory">Memory service for conversation persistence</param>
    /// <param name="inputValidator">Input validator for content filtering and guard rails</param>
    /// <param name="logger">Logger for diagnostic information</param>
    /// <param name="config">Configuration options for agent behavior</param>
    public ArtworkChatAgent(
        Kernel kernel,
        IPersonaFactory personaFactory,
        IArtworkDataProvider artworkProvider,
        IChatMemory chatMemory,
        IChatInputValidator inputValidator,
        ILogger<ArtworkChatAgent> logger,
        IOptions<AgentConfiguration> config)
    {
        _kernel = kernel;
        _chatService = kernel.GetRequiredService<IChatCompletionService>();
        _personaFactory = personaFactory; _artworkProvider = artworkProvider;
        _chatMemory = chatMemory;
        _inputValidator = inputValidator;
        _logger = logger;
        _config = config.Value;
    }

    /// <summary>
    /// Processes a chat request and generates an AI response based on the specified persona and artwork context.
    /// </summary>
    /// <param name="request">The chat request containing message, artwork ID, persona, and conversation history</param>
    /// <param name="cancellationToken">Token for canceling the operation</param>
    /// <returns>A chat response with AI-generated content, suggested questions, and analytics</returns>
    /// <remarks>
    /// This method:
    /// 1. Retrieves and enriches artwork data
    /// 2. Creates the appropriate persona handler
    /// 3. Builds conversation history with system prompts
    /// 4. Optionally includes vision analysis for visual questions
    /// 5. Generates AI response using configured settings
    /// 6. Creates suggested follow-up questions
    /// 7. Saves conversation to memory if caching is enabled
    /// 8. Returns comprehensive response with analytics
    /// </remarks>
    public async Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew(); try
        {
            _logger.LogInformation("Starting chat for artwork {ArtworkId} with persona {Persona}",
                request.ArtworkId, request.Persona);            // Step 1: Validate input and check guard rails
            var validationResult = _inputValidator.ValidateRequest(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Chat request validation failed: {ErrorCode} - {ErrorMessage}",
                    validationResult.ErrorCode, validationResult.ErrorMessage);

                return ChatResponse.FromValidationError(validationResult);
            }

            _logger.LogDebug("Chat request validation passed for artwork {ArtworkId}", request.ArtworkId);

            // Step 2: Get artwork data
            var artwork = await _artworkProvider.GetArtworkAsync(request.ArtworkId, cancellationToken);
            if (artwork == null)
            {
                return new ChatResponse
                {
                    Success = false,
                    Error = "Artwork not found"
                };
            }

            // Enrich artwork data if needed
            artwork = await _artworkProvider.EnrichArtworkDataAsync(artwork, cancellationToken);

            // Create persona handler
            var persona = _personaFactory.CreatePersona(request.Persona, artwork);            // Build chat history
            var chatHistory = BuildChatHistory(request, artwork, persona);

            // Get response from AI
            var settings = request.Settings ?? _config.DefaultChatSettings;
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = settings.MaxTokens,
                Temperature = settings.Temperature,
                TopP = settings.TopP
            };

            var response = await _chatService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                cancellationToken: cancellationToken);            // Generate suggested questions
            var suggestedQuestions = await GenerateSuggestedQuestionsAsync(
                artwork, request.Message, request.Persona, cancellationToken);

            // Save conversation if memory is enabled
            if (_config.Cache.Enabled)
            {
                var updatedHistory = request.ConversationHistory.ToList();
                updatedHistory.Add(new ChatMessage { Role = MessageRole.User, Content = request.Message });
                updatedHistory.Add(new ChatMessage { Role = MessageRole.Assistant, Content = response.Content ?? string.Empty });

                await _chatMemory.SaveConversationAsync(
                    request.ArtworkId, request.Persona, updatedHistory, cancellationToken);
            }

            stopwatch.Stop();

            var chatResponse = new ChatResponse
            {
                Response = response.Content ?? string.Empty,
                Success = true,
                SuggestedQuestions = suggestedQuestions ?? new List<string>(),
                Analytics = new ChatAnalytics
                {
                    TokensUsed = response.Metadata?.ContainsKey("Usage") == true
                        ? ExtractTokenUsage(response.Metadata["Usage"])
                        : 0,
                    ResponseTime = stopwatch.Elapsed,
                    ModelUsed = settings.ModelId,
                    VisionUsed = request.IncludeVisualAnalysis && !string.IsNullOrEmpty(artwork.ImageUrl)
                }
            };

            _logger.LogInformation("Chat completed for artwork {ArtworkId} in {ElapsedMs}ms",
                request.ArtworkId, stopwatch.ElapsedMilliseconds);

            return chatResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ChatAsync for artwork {ArtworkId}", request.ArtworkId); return new ChatResponse
            {
                Success = false,
                Error = "An error occurred while processing your request",
                Analytics = new ChatAnalytics { ResponseTime = stopwatch.Elapsed }
            };
        }
    }    /// <summary>
         /// Builds a complete chat history including system prompt, conversation history, and current message.
         /// </summary>
         /// <param name="request">The chat request containing conversation context</param>
         /// <param name="artwork">The artwork data for context</param>
         /// <param name="persona">The persona handler for generating appropriate prompts</param>
         /// <returns>A ChatHistory object ready for AI processing</returns>
    private ChatHistory BuildChatHistory(
        ChatRequest request,
        ArtworkData artwork,
        IPersonaHandler persona)
    {
        var chatHistory = new ChatHistory();

        // Add system prompt
        var systemPrompt = persona.GenerateSystemPrompt(artwork);
        chatHistory.AddSystemMessage(systemPrompt);

        // Add conversation history
        foreach (var msg in request.ConversationHistory)
        {
            switch (msg.Role)
            {
                case MessageRole.User:
                    chatHistory.AddUserMessage(msg.Content);
                    break;
                case MessageRole.Assistant:
                    chatHistory.AddAssistantMessage(msg.Content);
                    break;
            }
        }        // Add current message with vision if enabled
        AddUserMessageWithVision(chatHistory, request, artwork); return chatHistory;
    }    /// <summary>
         /// Adds a user message to the chat history, optionally including vision analysis for image-based questions.
         /// </summary>
         /// <param name="chatHistory">The chat history to add the message to</param>
         /// <param name="request">The chat request containing the user message and settings</param>
         /// <param name="artwork">The artwork data including image URL for vision analysis</param>
         /// <remarks>
         /// Vision analysis is included when:
         /// - Visual analysis is requested in the chat request
         /// - The artwork has an available image URL
         /// - The message contains visual keywords or it's the first message in conversation
         /// - The configured model supports vision (contains "vision" or "4o")
         /// </remarks>
    private void AddUserMessageWithVision(
        ChatHistory chatHistory,
        ChatRequest request,
        ArtworkData artwork)
    {
        var needsVision = request.IncludeVisualAnalysis &&
                         !string.IsNullOrEmpty(artwork.ImageUrl) &&
                         (IsVisualAnalysisRequest(request.Message) || request.ConversationHistory.Count == 0);

        if (needsVision && _config.OpenAI.VisionModelId.Contains("vision") || _config.OpenAI.VisionModelId.Contains("4o"))
        {
            try
            {
                var messageContent = new ChatMessageContentItemCollection
                {
                    new TextContent(request.Message),
                    new ImageContent(new Uri(artwork.ImageUrl))
                };

                chatHistory.Add(new ChatMessageContent(AuthorRole.User, messageContent));
                _logger.LogDebug("Added vision-enabled message for artwork {ArtworkId}", artwork.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add vision content, falling back to text-only");
                chatHistory.AddUserMessage(request.Message);
            }
        }
        else
        {
            chatHistory.AddUserMessage(request.Message);
        }
    }

    /// <summary>
    /// Determines if a user message contains keywords that suggest they want visual analysis of the artwork.
    /// </summary>
    /// <param name="message">The user message to analyze</param>
    /// <returns>True if the message contains visual analysis keywords, false otherwise</returns>
    private bool IsVisualAnalysisRequest(string message)
    {
        var visualKeywords = new[]
        {
            "see", "look", "color", "shape", "pattern", "design", "visual", "appearance",
            "texture", "carving", "details", "surface", "decoration", "style", "form",
            "composition", "describe", "show", "appears", "seems", "looks like"
        };

        return visualKeywords.Any(keyword => message.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Extracts token usage information from AI response metadata.
    /// </summary>
    /// <param name="usage">The usage metadata object from the AI response</param>
    /// <returns>The number of tokens used, or 0 if extraction fails</returns>
    /// <remarks>
    /// This method needs to be implemented based on the specific format of usage metadata
    /// returned by the AI service. Currently returns 0 as a placeholder.
    /// </remarks>
    private int ExtractTokenUsage(object? usage)
    {        // Implementation depends on the specific format of usage metadata
        // This is a placeholder - adjust based on actual metadata structure
        return 0;
    }

    /// <summary>
    /// Generates conversation starter questions for a specific artwork and persona combination.
    /// </summary>
    /// <param name="artwork">The artwork to generate conversation starters for</param>
    /// <param name="persona">The persona perspective for the conversation</param>
    /// <param name="cancellationToken">Token for canceling the operation</param>
    /// <returns>A list of engaging conversation starter questions</returns>
    /// <remarks>
    /// Falls back to generic questions if the persona-specific generation fails.
    /// </remarks>
    public async Task<List<string>> GenerateConversationStartersAsync(
        ArtworkData artwork,
        ChatPersona persona,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var personaHandler = _personaFactory.CreatePersona(persona, artwork);
            return await personaHandler.GenerateConversationStartersAsync(artwork);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating conversation starters for artwork {ArtworkId}", artwork.Id);
            return new List<string>
            {
                "Tell me about yourself",
                "What's your story?",
                "What makes you special?"
            };
        }
    }

    /// <summary>
    /// Generates contextual follow-up questions based on the conversation and artwork context.
    /// </summary>
    /// <param name="artwork">The artwork being discussed</param>
    /// <param name="lastMessage">The user's most recent message</param>
    /// <param name="persona">The current conversation persona</param>
    /// <param name="cancellationToken">Token for canceling the operation</param>
    /// <returns>A list of suggested follow-up questions</returns>
    /// <remarks>
    /// Uses AI to generate contextually relevant questions based on the artwork details,
    /// user's last message, and the active persona. Falls back to default questions if generation fails.
    /// </remarks>
    public async Task<List<string>> GenerateSuggestedQuestionsAsync(
        ArtworkData artwork,
        string lastMessage,
        ChatPersona persona,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = BuildSuggestedQuestionsPrompt(artwork, lastMessage, persona);
            var response = await _chatService.GetChatMessageContentAsync(prompt, cancellationToken: cancellationToken);

            return ParseSuggestedQuestions(response.Content ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating suggested questions for artwork {ArtworkId}", artwork.Id);
            return GetDefaultSuggestedQuestions(persona);
        }
    }

    /// <summary>
    /// Builds a prompt for generating suggested follow-up questions based on conversation context.
    /// </summary>
    /// <param name="artwork">The artwork being discussed</param>
    /// <param name="lastMessage">The user's most recent message</param>
    /// <param name="persona">The current conversation persona</param>
    /// <returns>A formatted prompt for the AI to generate suggested questions</returns>
    private string BuildSuggestedQuestionsPrompt(ArtworkData artwork, string lastMessage, ChatPersona persona)
    {
        var personaContext = persona switch
        {
            ChatPersona.Artwork => "as the artwork itself",
            ChatPersona.Artist => "as the artist who created this work",
            ChatPersona.Curator => "from a curatorial perspective",
            ChatPersona.Historian => "from a historical context",
            _ => string.Empty
        };

        return $@"
Based on this conversation about '{artwork.Title}' {personaContext}, suggest 3 engaging follow-up questions.

Artwork: {artwork.Title} by {artwork.ArtistDisplay}
Origin: {artwork.PlaceOfOrigin}, {artwork.DateDisplay}
Cultural Context: {artwork.CulturalContext}
User's last message: {lastMessage}

Generate 3 natural, conversational questions that would help deepen the user's understanding.
Consider the {persona.ToString().ToLower()} perspective and include questions about:
- Visual elements and artistic techniques
- Cultural significance and historical context  
- Personal stories and emotional connections
- Contemporary relevance and interpretation

Format as a simple numbered list.
";
    }

    /// <summary>
    /// Parses AI-generated suggested questions from response text into a structured list.
    /// </summary>
    /// <param name="response">The AI response containing suggested questions</param>
    /// <returns>A list of parsed questions, limited to 3 items</returns>
    private List<string> ParseSuggestedQuestions(string response)
    {
        return response.Split('\n')
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Trim().TrimStart('-', '*', '1', '2', '3', '.', ' '))
            .Where(line => line.Length > 10) // Filter out too-short responses
            .Take(3).ToList();
    }

    /// <summary>
    /// Provides default suggested questions for each persona type when AI generation fails.
    /// </summary>
    /// <param name="persona">The persona to get default questions for</param>
    /// <returns>A list of persona-appropriate default questions</returns>
    private List<string> GetDefaultSuggestedQuestions(ChatPersona persona)
    {
        return persona switch
        {
            ChatPersona.Artwork => new List<string>
            {
                "What do your colors and patterns mean?",
                "Tell me about your cultural significance",
                "How do you feel about being in a museum?"
            },
            ChatPersona.Artist => new List<string>
            {
                "What inspired you to create this piece?",
                "What techniques did you use?",
                "What does this work mean to your community?"
            },
            ChatPersona.Curator => new List<string>
            {
                "How did this artwork come to the museum?",
                "What research has been done on this piece?",
                "How does this compare to similar works?"
            },
            ChatPersona.Historian => new List<string>
            {
                "What was happening historically when this was created?",
                "How did cultural exchanges influence this work?",
                "What can this teach us about its time period?"
            },
            _ => new List<string>
            {
                "Tell me more about this artwork",
                "What makes this piece special?",
                "What should I know about its history?"
            }
        };
    }

    /// <summary>
    /// Performs detailed visual analysis of an artwork image using AI vision capabilities.
    /// </summary>
    /// <param name="artwork">The artwork to analyze (must have an ImageUrl)</param>
    /// <param name="specificQuestion">Optional specific question about the visual aspects</param>
    /// <param name="cancellationToken">Token for canceling the operation</param>
    /// <returns>A detailed visual analysis description or error message</returns>
    /// <remarks>
    /// This method uses vision-capable AI models to analyze artwork images and provide
    /// detailed descriptions of visual elements, colors, patterns, textures, and composition.
    /// Requires the artwork to have a valid image URL.
    /// </remarks>
    public async Task<string> AnalyzeArtworkVisuallyAsync(
        ArtworkData artwork,
        string? specificQuestion = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(artwork.ImageUrl))
            {
                return "I don't have access to a visual representation of this artwork.";
            }

            var prompt = specificQuestion ?? "Provide a detailed visual analysis of this artwork, describing its colors, patterns, textures, composition, and notable artistic elements.";

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage("You are an expert art analyst. Provide detailed, accurate visual descriptions based on what you can see in the image.");

            var messageContent = new ChatMessageContentItemCollection
            {
                new TextContent(prompt),
                new ImageContent(new Uri(artwork.ImageUrl))
            };

            chatHistory.Add(new ChatMessageContent(AuthorRole.User, messageContent));

            var response = await _chatService.GetChatMessageContentAsync(
                chatHistory,
                new OpenAIPromptExecutionSettings { MaxTokens = 500, Temperature = 0.3 }, cancellationToken: cancellationToken);

            return response.Content ?? "I'm unable to provide visual analysis at this time.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in visual analysis for artwork {ArtworkId}", artwork.Id);
            return "I'm unable to perform visual analysis at this time.";
        }
    }
}
