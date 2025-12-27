using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using WebSpark.ArtSpark.Agent.Personas;
using WebSpark.ArtSpark.Agent.Services;

namespace WebSpark.ArtSpark.Demo.Services;

public interface IBuildInfoService
{
    string GetVersion();
    DateTime GetBuildDate();
    string GetFormattedBuildInfo();
    Task<PromptMetadataInfo> GetPromptMetadataAsync();
}

public class PromptMetadataInfo
{
    public Dictionary<string, PersonaPromptInfo> Personas { get; set; } = new();
    public int TotalPrompts => Personas.Count;
    public int FallbackCount => Personas.Values.Count(p => p.IsFallback);
    public bool HasFallbacks => FallbackCount > 0;
}

public class PersonaPromptInfo
{
    public string PersonaName { get; set; } = string.Empty;
    public string? ContentHash { get; set; }
    public bool IsFallback { get; set; }
    public string? ModelId { get; set; }
    public double? Temperature { get; set; }
}

public class BuildInfoService : IBuildInfoService
{
    private readonly string _version;
    private readonly DateTime _buildDate;
    private readonly IPromptLoader? _promptLoader;

    public BuildInfoService(IPromptLoader? promptLoader = null)
    {
        _promptLoader = promptLoader;
        var assembly = Assembly.GetExecutingAssembly();

        // Get version from assembly
        _version = assembly.GetName().Version?.ToString() ?? "1.0.0";

        // Get build date from assembly last write time (more accurate for builds)
        var assemblyLocation = assembly.Location;
        if (!string.IsNullOrEmpty(assemblyLocation) && File.Exists(assemblyLocation))
        {
            _buildDate = File.GetLastWriteTime(assemblyLocation);
        }
        else
        {
            // Fallback: Try to get from entry assembly
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly?.Location != null && File.Exists(entryAssembly.Location))
            {
                _buildDate = File.GetLastWriteTime(entryAssembly.Location);
            }
            else
            {
                // Final fallback to current time
                _buildDate = DateTime.Now;
            }
        }
    }

    public string GetVersion()
    {
        return _version;
    }

    public DateTime GetBuildDate()
    {
        return _buildDate;
    }

    public string GetFormattedBuildInfo()
    {
        return $"v{_version} - Built {_buildDate:MM/dd/yyyy}";
    }

    public async Task<PromptMetadataInfo> GetPromptMetadataAsync()
    {
        var metadata = new PromptMetadataInfo();

        if (_promptLoader == null)
        {
            return metadata; // No prompt loader available
        }

        var personas = new[]
        {
            ChatPersona.Artwork,
            ChatPersona.Artist,
            ChatPersona.Curator,
            ChatPersona.Historian
        };

        foreach (var persona in personas)
        {
            try
            {
                var template = await _promptLoader.GetPromptAsync(persona);

                var personaInfo = new PersonaPromptInfo
                {
                    PersonaName = persona.ToString(),
                    IsFallback = template.IsFallback,
                    ModelId = template.MetadataOverrides?.ModelId,
                    Temperature = template.MetadataOverrides?.Temperature
                };

                // Generate content hash for audit trail
                if (!string.IsNullOrEmpty(template.Content))
                {
                    using var md5 = MD5.Create();
                    var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(template.Content));
                    personaInfo.ContentHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }

                metadata.Personas[persona.ToString()] = personaInfo;
            }
            catch
            {
                // If prompt loading fails, mark as fallback
                metadata.Personas[persona.ToString()] = new PersonaPromptInfo
                {
                    PersonaName = persona.ToString(),
                    IsFallback = true
                };
            }
        }

        return metadata;
    }
}
