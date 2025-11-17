# WebSpark.ArtSpark Development Guidelines

Auto-generated from all feature plans. Last updated: 2025-11-16

## Active Technologies
- C# 13 / .NET 10 (Preview) + ASP.NET Core Identity, Entity Framework Core, Serilog, ImageSharp (or equivalent for resizing), WebSpark.ArtSpark shared libraries (002-user-profile)
- SQLite via EF Core migrations (production-ready for single-node Demo; local file system for profile photos and thumbnails) (002-user-profile)
- C# / .NET 10.0 (Preview) + ASP.NET Core, Microsoft Semantic Kernel, Serilog, Polly, WebSpark.ArtSpark.Agent, WebSpark.ArtSpark.Client (003-prompt-management)
- SQLite via EF Core migrations (no schema changes; prompt files stored on web server file system) (003-prompt-management)

- C# / .NET 9.0 (consistent with solution) + .NET SDK CLI, NuGet CLI APIs, npm CLI, Serilog instrumentation hooks, existing WebSpark.ArtSpark solution build pipelines (001-quality-audit)

## Project Structure

```text
backend/
frontend/
tests/
```

## Commands

# Add commands for C# / .NET 9.0 (consistent with solution)

## Code Style

C# / .NET 9.0 (consistent with solution): Follow standard conventions

## Recent Changes
- 003-prompt-management: Added C# / .NET 10.0 (Preview) + ASP.NET Core, Microsoft Semantic Kernel, Serilog, Polly, WebSpark.ArtSpark.Agent, WebSpark.ArtSpark.Client
- 002-user-profile: Added C# 13 / .NET 10 (Preview) + ASP.NET Core Identity, Entity Framework Core, Serilog, ImageSharp (or equivalent for resizing), WebSpark.ArtSpark shared libraries

- 001-quality-audit: Added C# / .NET 9.0 (consistent with solution) + .NET SDK CLI, NuGet CLI APIs, npm CLI, Serilog instrumentation hooks, existing WebSpark.ArtSpark solution build pipelines

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
