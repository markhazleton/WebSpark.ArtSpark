using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebSpark.ArtSpark.Agent.Models;
using WebSpark.ArtSpark.Agent.Personas;
using WebSpark.ArtSpark.Agent.Services;

namespace WebSpark.ArtSpark.Agent.Extensions
{
    /// <summary>
    /// Example demonstrating guard rails and input validation features
    /// </summary>
    public class GuardRailsUsageExample
    {
        /// <summary>
        /// Demonstrates various validation scenarios and guard rail implementations
        /// </summary>
        public static async Task RunGuardRailsExample()
        {
            // This example shows how the guard rails system works
            Console.WriteLine("🛡️ ArtSpark Chat Agent - Guard Rails & Input Validation Demo");
            Console.WriteLine("========================================================\n");

            // Setup services (this would typically be done in your DI container)
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder.AddConsole());
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<ChatInputValidator>>();
            var validator = new ChatInputValidator(logger);

            // Test cases demonstrating different validation scenarios
            var testCases = new[]
            {
                // ✅ Valid art-related questions
                new TestCase("What colors do you see in this painting?", ChatPersona.Artwork, true, "Valid art question"),
                new TestCase("How did the artist create those textures?", ChatPersona.Artist, true, "Valid technique question"),
                new TestCase("What is the historical significance of this piece?", ChatPersona.Historian, true, "Valid historical question"),
                new TestCase("How does this fit into the museum's collection?", ChatPersona.Curator, true, "Valid curatorial question"),

                // 🚫 Invalid - Empty or too short messages
                new TestCase(string.Empty, ChatPersona.Artwork, false, "Empty message"),
                new TestCase("hi", ChatPersona.Artwork, false, "Too short (but greeting might be allowed)"),

                // 🚫 Invalid - Too long messages
                new TestCase(new string('a', 600), ChatPersona.Artwork, false, "Message too long"),

                // 🚫 Invalid - Off-topic content
                new TestCase("What's the weather like today?", ChatPersona.Artwork, false, "Completely off-topic"),
                new TestCase("Can you help me with my math homework?", ChatPersona.Artist, false, "Off-topic question"),
                new TestCase("What's your favorite restaurant?", ChatPersona.Curator, false, "Personal/off-topic"),

                // 🚫 Invalid - Inappropriate content
                new TestCase("This is spam! Buy now! Click here!", ChatPersona.Artwork, false, "Spam content"),
                new TestCase("HELP ME WITH MY BUSINESS!!!", ChatPersona.Artist, false, "Excessive caps and off-topic"),

                // ✅ Valid - Borderline cases that should pass
                new TestCase("Hello! Tell me about yourself.", ChatPersona.Artwork, true, "Polite greeting with art context"),
                new TestCase("Thank you for that explanation. What else?", ChatPersona.Historian, true, "Conversation continuation"),
                new TestCase("I don't understand art very well. Can you help?", ChatPersona.Curator, true, "Honest question about art"),

                // 🔄 Context-dependent cases
                new TestCase("How do you feel about being displayed here?", ChatPersona.Artwork, true, "Persona-appropriate question"),
                new TestCase("What inspired you when creating this?", ChatPersona.Artist, true, "Artist-specific question"),
                new TestCase("What research has been done on this piece?", ChatPersona.Curator, true, "Curator-specific question"),
                new TestCase("What period does this represent?", ChatPersona.Historian, true, "Historian-specific question")
            };

            Console.WriteLine("Running validation tests...\n");

            int passed = 0;
            int failed = 0;

            foreach (var testCase in testCases)
            {
                var request = new ChatRequest
                {
                    ArtworkId = 123456,
                    Message = testCase.Message,
                    Persona = testCase.Persona,
                    ConversationHistory = new List<ChatMessage>()
                };

                var result = validator.ValidateRequest(request);
                var actualValid = result.IsValid;
                var testPassed = actualValid == testCase.ExpectedValid;

                if (testPassed)
                {
                    Console.WriteLine($"✅ PASS: {testCase.Description}");
                    passed++;
                }
                else
                {
                    Console.WriteLine($"❌ FAIL: {testCase.Description}");
                    Console.WriteLine($"   Expected: {testCase.ExpectedValid}, Got: {actualValid}");
                    if (!result.IsValid)
                    {
                        Console.WriteLine($"   Error: {result.ErrorMessage}");
                        Console.WriteLine($"   Suggestion: {result.Suggestion}");
                    }
                    failed++;
                }

                // Show some validation details for interesting cases
                if (!result.IsValid && testCase.ExpectedValid == false)
                {
                    Console.WriteLine($"   📋 Validation Details:");
                    Console.WriteLine($"      Error Code: {result.ErrorCode}");
                    Console.WriteLine($"      Message: {result.ErrorMessage}");
                    if (!string.IsNullOrEmpty(result.Suggestion))
                    {
                        Console.WriteLine($"      Suggestion: {result.Suggestion}");
                    }
                }

                Console.WriteLine();
            }

            // Summary
            Console.WriteLine("========================================================");
            Console.WriteLine($"Validation Test Results: {passed} passed, {failed} failed");
            Console.WriteLine($"Success Rate: {(double)passed / (passed + failed):P1}");

            // Demonstrate individual validation methods
            Console.WriteLine("\n🔍 Individual Validation Method Examples:");
            Console.WriteLine("==========================================");

            // Format validation
            Console.WriteLine("\n📏 Format Validation:");
            TestFormatValidation(validator, "Valid message");
            TestFormatValidation(validator, string.Empty);
            TestFormatValidation(validator, new string('x', 600));
            TestFormatValidation(validator, "!!!!!!!!!");

            // Art-related content detection
            Console.WriteLine("\n🎨 Art Content Detection:");
            TestArtRelated(validator, "What colors are in this painting?");
            TestArtRelated(validator, "Tell me about the brushstrokes");
            TestArtRelated(validator, "What's for lunch today?");
            TestArtRelated(validator, "Hello, how are you?"); // Should pass as greeting

            // Inappropriate content detection
            Console.WriteLine("\n🚫 Inappropriate Content Detection:");
            TestInappropriate(validator, "This artwork is beautiful");
            TestInappropriate(validator, "BUY NOW! CLICK HERE!");
            TestInappropriate(validator, "HELP ME!!!! URGENT!!!!");

            // Persona appropriateness
            Console.WriteLine("\n👤 Persona Appropriateness:");
            TestPersonaAppropriate(validator, "How do you feel about being viewed?", ChatPersona.Artwork);
            TestPersonaAppropriate(validator, "What techniques did you use?", ChatPersona.Artist);
            TestPersonaAppropriate(validator, "What's the historical context?", ChatPersona.Historian);
            TestPersonaAppropriate(validator, "How was this acquired?", ChatPersona.Curator);

            Console.WriteLine("\n✨ Guard Rails Demo Complete!");
            Console.WriteLine("The validation system helps ensure appropriate, on-topic conversations about art.");
        }

        private static void TestFormatValidation(ChatInputValidator validator, string message)
        {
            var isValid = validator.IsValidFormat(message);
            var preview = message.Length > 30 ? message.Substring(0, 30) + "..." : message;
            if (string.IsNullOrEmpty(message)) preview = "[empty]";
            Console.WriteLine($"   {(isValid ? "✅" : "❌")} Format: \"{preview}\"");
        }

        private static void TestArtRelated(ChatInputValidator validator, string message)
        {
            var isArtRelated = validator.IsArtRelated(message);
            Console.WriteLine($"   {(isArtRelated ? "✅" : "❌")} Art-related: \"{message}\"");
        }

        private static void TestInappropriate(ChatInputValidator validator, string message)
        {
            var isInappropriate = validator.ContainsInappropriateContent(message);
            Console.WriteLine($"   {(isInappropriate ? "🚫" : "✅")} Content check: \"{message}\"");
        }

        private static void TestPersonaAppropriate(ChatInputValidator validator, string message, ChatPersona persona)
        {
            var isAppropriate = validator.IsPersonaAppropriate(message, persona);
            Console.WriteLine($"   {(isAppropriate ? "✅" : "⚠️")} {persona}: \"{message}\"");
        }

        private class TestCase
        {
            public string Message { get; }
            public ChatPersona Persona { get; }
            public bool ExpectedValid { get; }
            public string Description { get; }

            public TestCase(string message, ChatPersona persona, bool expectedValid, string description)
            {
                Message = message;
                Persona = persona;
                ExpectedValid = expectedValid;
                Description = description;
            }
        }
    }
}
