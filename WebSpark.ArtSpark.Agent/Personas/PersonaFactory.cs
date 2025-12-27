namespace WebSpark.ArtSpark.Agent.Personas
{
    // 6. Personas/PersonaFactory.cs
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using WebSpark.ArtSpark.Agent.Configuration;
    using WebSpark.ArtSpark.Agent.Interfaces;
    using WebSpark.ArtSpark.Agent.Models;
    using WebSpark.ArtSpark.Agent.Services;

    public class PersonaFactory : IPersonaFactory
    {
        private readonly IPromptLoader _promptLoader;
        private readonly IOptions<PromptOptions> _promptOptions;
        private readonly ILogger<FileBackedPersonaHandler> _logger;

        public PersonaFactory(
            IPromptLoader promptLoader,
            IOptions<PromptOptions> promptOptions,
            ILogger<FileBackedPersonaHandler> logger)
        {
            _promptLoader = promptLoader;
            _promptOptions = promptOptions;
            _logger = logger;
        }

        public IPersonaHandler CreatePersona(ChatPersona persona, ArtworkData artwork)
        {
            IPersonaHandler baseHandler = persona switch
            {
                ChatPersona.Artwork => new ArtworkPersona(),
                ChatPersona.Artist => new ArtistPersona(),
                ChatPersona.Curator => new CuratorPersona(),
                ChatPersona.Historian => new HistorianPersona(),
                _ => throw new ArgumentException($"Unknown persona type: {persona}")
            };

            // Wrap with file-backed decorator
            return new FileBackedPersonaHandler(baseHandler, _promptLoader, _promptOptions, _logger);
        }
    }
}