using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using WebSpark.ArtSpark.Client.Clients;
using WebSpark.HttpClientUtility.ClientService;
using WebSpark.HttpClientUtility.RequestResult;
using WebSpark.HttpClientUtility.StringConverter;

namespace WebSpark.ArtSpark.Console
{
    /// <summary>
    /// Test the artist search functionality
    /// </summary>
    public class ArtistSearchTest
    {
        public static async Task RunArtistSearchTests()
        {
            System.Console.WriteLine("Testing Artist Search Functionality");
            System.Console.WriteLine("==================================");

            // Setup DI
            var services = new ServiceCollection();

            // Add HttpClient factory
            services.AddHttpClient();

            // Register WebSpark.HttpClientUtility services
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

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Resolve dependencies
            var httpRequestResultService = serviceProvider.GetRequiredService<IHttpRequestResultService>();

            // Initialize the client
            var client = new ArtInstituteClient(httpRequestResultService);

            // Test cases
            string[] testArtists = { "Van Gogh", "Picasso", "Monet", "Cezanne", "Renoir" }; foreach (var artist in testArtists)
            {
                System.Console.WriteLine($"\nTesting search for artist: {artist}");
                System.Console.WriteLine(new string('-', 40));

                try
                {
                    var response = await client.GetArtworksByArtistAsync(artist, limit: 5);

                    if (response?.Data != null && response.Data.Any())
                    {
                        System.Console.WriteLine($"✓ Found {response.Data.Count()} artworks");
                        System.Console.WriteLine($"  Total available: {response.Pagination?.Total ?? 0}");

                        // Show first result
                        var firstArtwork = response.Data.First();
                        System.Console.WriteLine($"  First result: {firstArtwork.Title}");
                        System.Console.WriteLine($"  Artist: {firstArtwork.ArtistDisplay}");

                        // Test pagination with artist parameter
                        if (response.Pagination?.Total > 5)
                        {
                            System.Console.WriteLine("  Testing pagination...");
                            var page2Response = await client.GetArtworksByArtistAsync(artist, limit: 5, page: 2);
                            if (page2Response?.Data != null && page2Response.Data.Any())
                            {
                                System.Console.WriteLine($"  ✓ Page 2 found {page2Response.Data.Count()} artworks");
                            }
                            else
                            {
                                System.Console.WriteLine("  ✗ Page 2 failed");
                            }
                        }
                    }
                    else
                    {
                        System.Console.WriteLine($"✗ No artworks found for {artist}");
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"✗ Error searching for {artist}: {ex.Message}");
                }

                await Task.Delay(500); // Rate limiting
            }

            System.Console.WriteLine("\n==================================");
            System.Console.WriteLine("Artist search tests completed!");
        }
    }
}
