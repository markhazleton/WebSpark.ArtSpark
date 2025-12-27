
namespace WebSpark.ArtSpark.Agent.Extensions
{
    using Microsoft.Extensions.Configuration;
    // 10. Extensions/ServiceCollectionExtensions.cs
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.SemanticKernel;
    using WebSpark.ArtSpark.Agent.Configuration;
    using WebSpark.ArtSpark.Agent.Interfaces;
    using WebSpark.ArtSpark.Agent.Personas;
    using WebSpark.ArtSpark.Agent.Services;
    using WebSpark.HttpClientUtility.ClientService;
    using WebSpark.HttpClientUtility.RequestResult;
    using WebSpark.HttpClientUtility.StringConverter;
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds WebSpark.ArtSpark.Agent services to the dependency injection container
        /// </summary>
        public static IServiceCollection AddArtSparkAgent(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services.AddArtSparkAgent(configuration, null);
        }

        /// <summary>
        /// Adds WebSpark.ArtSpark.Agent services with custom configuration
        /// </summary>
        public static IServiceCollection AddArtSparkAgent(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<AgentConfiguration>? configureOptions)
        {
            // Register configuration
            services.Configure<AgentConfiguration>(configuration.GetSection("ArtSparkAgent"));

            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            // Register prompt management configuration and services
            services.Configure<PromptOptions>(configuration.GetSection("ArtSparkAgent:Prompts"));
            services.AddSingleton<IPromptLoader, PromptLoader>();            // Register HTTP client for API calls and required HttpClientUtility services
            services.AddHttpClient();
            services.AddSingleton<IStringConverter, SystemJsonStringConverter>();
            services.AddScoped<IHttpClientService, HttpClientService>();
            services.AddScoped<HttpRequestResultService>();

            // Register IHttpRequestResultService with telemetry decorator
            services.AddScoped<IHttpRequestResultService>(provider =>
            {
                IHttpRequestResultService service = provider.GetRequiredService<HttpRequestResultService>();

                // Add Telemetry (basic decorator for logging and monitoring)
                service = new HttpRequestResultServiceTelemetry(
                    provider.GetRequiredService<ILogger<HttpRequestResultServiceTelemetry>>(),
                    service
                );

                return service;
            });

            // Register Semantic Kernel
            services.AddScoped<Kernel>(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AgentConfiguration>>().Value;
                var logger = serviceProvider.GetRequiredService<ILogger<Kernel>>();

                var kernelBuilder = Kernel.CreateBuilder();

                // Add OpenAI chat completion service
                kernelBuilder.AddOpenAIChatCompletion(
                    modelId: config.OpenAI.ModelId,
                    apiKey: config.OpenAI.ApiKey);

                var kernel = kernelBuilder.Build();

                logger.LogInformation("Semantic Kernel initialized with model {ModelId}", config.OpenAI.ModelId);
                return kernel;
            });            // Register core services
            services.AddScoped<IArtworkChatAgent, ArtworkChatAgent>();
            services.AddScoped<IPersonaFactory, PersonaFactory>();
            services.AddSingleton<IChatMemory, InMemoryChatMemory>();
            services.AddScoped<IChatInputValidator, ChatInputValidator>();

            // Register data provider
            services.AddScoped<IArtworkDataProvider, ArtInstituteDataProvider>();

            return services;
        }

        /// <summary>
        /// Adds ArtSpark Agent with custom data provider
        /// </summary>
        public static IServiceCollection AddArtSparkAgent<TDataProvider>(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<AgentConfiguration>? configureOptions = null)
            where TDataProvider : class, IArtworkDataProvider
        {
            services.AddArtSparkAgent(configuration, configureOptions);

            // Replace the default data provider
            services.AddScoped<IArtworkDataProvider, TDataProvider>();

            return services;
        }

        /// <summary>
        /// Adds ArtSpark Agent with custom memory provider
        /// </summary>
        public static IServiceCollection AddArtSparkAgentWithMemory<TMemory>(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<AgentConfiguration>? configureOptions = null)
            where TMemory : class, IChatMemory
        {
            services.AddArtSparkAgent(configuration, configureOptions);

            // Replace the default memory provider
            services.AddScoped<IChatMemory, TMemory>();

            return services;
        }
    }
}

