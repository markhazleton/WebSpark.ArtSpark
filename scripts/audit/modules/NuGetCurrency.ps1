<#
.SYNOPSIS
    NuGet package currency detection for WebSpark.ArtSpark
.DESCRIPTION
    Runs dotnet list package --outdated to identify stale NuGet dependencies
#>

<#
.SYNOPSIS
    Get NuGet package currency information
.DESCRIPTION
    Scans all .csproj files and reports outdated packages
#>
function Get-NuGetCurrency {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$RepoRoot,
        
        [string]$SolutionPath = "WebSpark.ArtSpark.sln"
    )

    $entries = @()
    $fullSolutionPath = Join-Path $RepoRoot $SolutionPath

    if (-not (Test-Path $fullSolutionPath)) {
        Write-Warning "Solution file not found: $fullSolutionPath"
        return $entries
    }

    Write-Host "  Checking NuGet packages..." -ForegroundColor Gray

    try {
        # Find all project files
        $projects = Get-ChildItem -Path $RepoRoot -Filter "*.csproj" -Recurse | 
            Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' }

        foreach ($project in $projects) {
            Write-Host "    Scanning $($project.Name)..." -ForegroundColor DarkGray
            
            # Run dotnet list package --outdated
            $output = & dotnet list $project.FullName package --outdated 2>&1 | Out-String
            
            # Parse output for outdated packages
            $lines = $output -split "`n"
            $inOutdatedSection = $false
            
            foreach ($line in $lines) {
                if ($line -match '^\s*>\s+(.+?)\s+(\S+)\s+(\S+)\s+(\S+)') {
                    # Format: > PackageName CurrentVersion ResolvedVersion LatestVersion
                    $packageName = $matches[1].Trim()
                    $currentVersion = $matches[2]
                    $latestVersion = $matches[4]
                    
                    $severity = Get-PackageSeverity -Current $currentVersion -Latest $latestVersion
                    
                    $entry = [PSCustomObject]@{
                        SourceProject = $project.BaseName
                        PackageName = $packageName
                        PackageManager = 'NuGet'
                        CurrentVersion = $currentVersion
                        LatestStableVersion = $latestVersion
                        ReleaseAgeDays = 0
                        Severity = $severity
                        CompatibilityRisk = Get-CompatibilityRisk -Severity $severity
                        RecommendedAction = Get-RecommendedAction -PackageName $packageName -Severity $severity
                        Notes = ''
                    }
                    
                    $entries += $entry
                }
            }
        }

        Write-Host "  Found $($entries.Count) outdated NuGet packages" -ForegroundColor Gray
    }
    catch {
        Write-Warning "NuGet currency check failed: $_"
    }

    return $entries
}

function Get-PackageSeverity {
    param(
        [string]$Current,
        [string]$Latest
    )

    try {
        $currentParts = $Current -split '\.' | ForEach-Object { [int]$_ }
        $latestParts = $Latest -split '\.' | ForEach-Object { [int]$_ }

        # Major version change
        if ($latestParts[0] -gt $currentParts[0]) {
            return 'Major'
        }

        # Minor version change
        if ($latestParts.Length -gt 1 -and $currentParts.Length -gt 1) {
            if ($latestParts[1] -gt $currentParts[1]) {
                return 'Minor'
            }
        }

        # Patch version change
        return 'Minor'
    }
    catch {
        return 'Minor'
    }
}

function Get-CompatibilityRisk {
    param([string]$Severity)

    switch ($Severity) {
        'Major' { return 'High - may contain breaking changes' }
        'Minor' { return 'Low - generally backward compatible' }
        'Security' { return 'Critical - contains security fixes' }
        default { return 'Minimal' }
    }
}

function Get-RecommendedAction {
    param(
        [string]$PackageName,
        [string]$Severity
    )

    switch ($Severity) {
        'Major' { return "Review release notes before upgrading $PackageName" }
        'Security' { return "Upgrade $PackageName immediately" }
        default { return "Consider upgrading $PackageName in next maintenance cycle" }
    }
}
