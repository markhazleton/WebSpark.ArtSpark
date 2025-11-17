using System.Text.RegularExpressions;
using WebSpark.ArtSpark.Agent.Configuration;

namespace WebSpark.ArtSpark.Agent.Services;

/// <summary>
/// Parses YAML front matter from prompt markdown files to extract metadata overrides.
/// </summary>
public sealed class PromptMetadataParser
{
    private static readonly Regex FrontMatterRegex = new(
        @"^---\s*\n(.*?)\n---\s*\n",
        RegexOptions.Singleline | RegexOptions.Compiled
    );

    private static readonly Regex KeyValueRegex = new(
        @"^\s*(\w+):\s*(.+?)\s*$",
        RegexOptions.Compiled | RegexOptions.Multiline
    );

    /// <summary>
    /// Extracts YAML front matter and returns the metadata overrides plus the content body.
    /// </summary>
    public (PromptMetadata? Metadata, string ContentBody) Parse(string rawMarkdown)
    {
        var match = FrontMatterRegex.Match(rawMarkdown);
        if (!match.Success)
        {
            return (null, rawMarkdown);
        }

        var yamlContent = match.Groups[1].Value;
        var contentBody = rawMarkdown.Substring(match.Length);

        var metadata = ParseYamlMetadata(yamlContent);
        return (metadata, contentBody);
    }

    private PromptMetadata ParseYamlMetadata(string yaml)
    {
        var metadata = new PromptMetadata();
        var matches = KeyValueRegex.Matches(yaml);

        foreach (Match match in matches)
        {
            var key = match.Groups[1].Value.ToLowerInvariant();
            var value = match.Groups[2].Value.Trim();

            switch (key)
            {
                case "model":
                case "modelid":
                    metadata.ModelId = value;
                    break;

                case "temperature":
                    if (double.TryParse(value, out var temp))
                        metadata.Temperature = temp;
                    break;

                case "top_p":
                case "topp":
                    if (double.TryParse(value, out var topP))
                        metadata.TopP = topP;
                    break;

                case "max_output_tokens":
                case "maxoutputtokens":
                    if (int.TryParse(value, out var maxTokens))
                        metadata.MaxOutputTokens = maxTokens;
                    break;

                case "frequency_penalty":
                case "frequencypenalty":
                    if (double.TryParse(value, out var freqPenalty))
                        metadata.FrequencyPenalty = freqPenalty;
                    break;

                case "presence_penalty":
                case "presencepenalty":
                    if (double.TryParse(value, out var presPenalty))
                        metadata.PresencePenalty = presPenalty;
                    break;
            }
        }

        return metadata;
    }
}
