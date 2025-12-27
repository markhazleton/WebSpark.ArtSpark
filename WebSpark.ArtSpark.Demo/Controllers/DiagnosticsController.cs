using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebSpark.ArtSpark.Demo.Options;

namespace WebSpark.ArtSpark.Demo.Controllers;

/// <summary>
/// Diagnostic controller for testing file upload configuration
/// </summary>
[Authorize(Roles = "Admin")]
public class DiagnosticsController : Controller
{
    private readonly FileUploadOptions _options;
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(
        IOptions<FileUploadOptions> options,
        ILogger<DiagnosticsController> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Test profile photo upload configuration and directory access
    /// </summary>
    [HttpGet]
    public IActionResult TestProfilePhotoSetup()
    {
        var results = new List<string>();
        
        try
        {
            // 1. Check configuration
            results.Add($"? Configuration loaded");
            results.Add($"  ProfilePhotoPath: {_options.ProfilePhotoPath}");
            results.Add($"  MaxProfilePhotoSize: {_options.MaxProfilePhotoSize:N0} bytes");
            results.Add($"  AllowedImageTypes: {_options.AllowedImageTypes}");
            
            // 2. Resolve full path
            var fullPath = Path.GetFullPath(_options.ProfilePhotoPath);
            results.Add($"? Full path resolved: {fullPath}");
            
            // 3. Check if directory exists
            if (Directory.Exists(fullPath))
            {
                results.Add($"? Directory exists");
                
                // 4. Count existing files
                var files = Directory.GetFiles(fullPath);
                results.Add($"  Files in directory: {files.Length}");
                foreach (var file in files.Take(10))
                {
                    var fi = new FileInfo(file);
                    results.Add($"    - {fi.Name} ({fi.Length:N0} bytes)");
                }
                if (files.Length > 10)
                {
                    results.Add($"    ... and {files.Length - 10} more files");
                }
            }
            else
            {
                results.Add($"? Directory does NOT exist");
                results.Add($"  Attempting to create...");
                
                // Try to create directory
                try
                {
                    Directory.CreateDirectory(fullPath);
                    results.Add($"? Directory created successfully");
                }
                catch (Exception ex)
                {
                    results.Add($"? Failed to create directory: {ex.Message}");
                }
            }
            
            // 5. Test write access
            var testFileName = $"test_{Guid.NewGuid()}.txt";
            var testFilePath = Path.Combine(fullPath, testFileName);
            
            try
            {
                // Write test file
                System.IO.File.WriteAllText(testFilePath, "Test file for diagnostics");
                results.Add($"? Write test: Successfully wrote test file");
                
                // Read test file
                var content = System.IO.File.ReadAllText(testFilePath);
                results.Add($"? Read test: Successfully read test file");
                
                // Delete test file
                System.IO.File.Delete(testFilePath);
                results.Add($"? Delete test: Successfully deleted test file");
                
                results.Add($"? ALL TESTS PASSED - Directory is fully accessible");
            }
            catch (UnauthorizedAccessException ex)
            {
                results.Add($"? Permission denied: {ex.Message}");
                results.Add($"  Solution: Grant write permissions to the application pool identity");
            }
            catch (Exception ex)
            {
                results.Add($"? Write test failed: {ex.Message}");
            }
            finally
            {
                // Clean up test file if it exists
                if (System.IO.File.Exists(testFilePath))
                {
                    try { System.IO.File.Delete(testFilePath); } catch { }
                }
            }
            
            // 6. Check static file middleware configuration
            results.Add("");
            results.Add("Static File Middleware Configuration:");
            results.Add($"  Request Path: /uploads/profiles");
            results.Add($"  Should map to: {fullPath}");
            results.Add("");
            results.Add("Test URL (replace with actual filename):");
            results.Add($"  https://localhost:7282/uploads/profiles/test.jpg");
        }
        catch (Exception ex)
        {
            results.Add($"? Fatal error: {ex.Message}");
            results.Add($"  Stack trace: {ex.StackTrace}");
            _logger.LogError(ex, "Error in profile photo diagnostics");
        }
        
        return Content(string.Join("\n", results), "text/plain");
    }
}
