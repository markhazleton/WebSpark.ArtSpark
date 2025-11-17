using WebSpark.ArtSpark.Agent.Configuration;
using WebSpark.ArtSpark.Agent.Personas;

namespace WebSpark.ArtSpark.Agent.Models;

/// <summary>
/// Represents a loaded persona prompt template with metadata and validation state.
/// </summary>
public sealed class PromptTemplate
{
    /// <summary>
    /// The persona this template applies to.
    /// </summary>
    public required ChatPersona PersonaType { get; init; }

    /// <summary>
    /// Absolute path to the prompt file on disk.
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// File name only (e.g., "artspark.artwork.prompt.md").
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Raw markdown content without YAML front matter.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// MD5 hash of the content for versioning/auditing.
    /// </summary>
    public required string ContentHash { get; init; }

    /// <summary>
    /// Last modified timestamp from file system.
    /// </summary>
    public DateTime LastModifiedUtc { get; init; }

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; init; }

    /// <summary>
    /// True if this template is a fallback (hardcoded default), false if loaded from file.
    /// </summary>
    public bool IsFallback { get; init; }

    /// <summary>
    /// Metadata overrides parsed from YAML front matter (null if none or using fallback).
    /// </summary>
    public PromptMetadata? MetadataOverrides { get; init; }

    /// <summary>
    /// Validation errors encountered during loading (empty if valid).
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; init; } = Array.Empty<string>();

    /// <summary>
    /// True if the template passed all validation checks.
    /// </summary>
    public bool IsValid => ValidationErrors.Count == 0;
}
