<#
.SYNOPSIS
    npm package currency detection for WebSpark.ArtSpark
.DESCRIPTION
    Detects package.json manifests and collects npm outdated/audit results
#>

<#
.SYNOPSIS
    Get npm package currency information
.DESCRIPTION
    Scans for package.json files and reports outdated npm packages
#>
function Get-NpmCurrency {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$RepoRoot
    )

    $entries = @()

    Write-Host "  Checking npm packages..." -ForegroundColor Gray

    try {
        # Find all package.json files
        $packageFiles = Get-ChildItem -Path $RepoRoot -Filter "package.json" -Recurse | 
            Where-Object { $_.FullName -notmatch 'node_modules' }

        if ($packageFiles.Count -eq 0) {
            Write-Host "    No package.json files detected" -ForegroundColor DarkGray
            return $entries
        }

        foreach ($packageFile in $packageFiles) {
            Write-Host "    Scanning $($packageFile.Directory.Name)/package.json..." -ForegroundColor DarkGray
            
            $packageDir = $packageFile.Directory.FullName
            
            # Check if npm is available
            $npmVersion = & npm --version 2>&1
            if ($LASTEXITCODE -ne 0) {
                Write-Warning "npm not found - skipping npm currency checks"
                continue
            }

            # Run npm outdated
            Push-Location $packageDir
            try {
                $outdatedOutput = & npm outdated --json 2>&1 | ConvertFrom-Json
                
                foreach ($package in $outdatedOutput.PSObject.Properties) {
                    $packageName = $package.Name
                    $info = $package.Value
                    
                    $entry = [PSCustomObject]@{
                        SourceProject = $packageFile.Directory.Name
                        PackageName = $packageName
                        PackageManager = 'npm'
                        CurrentVersion = $info.current
                        LatestStableVersion = $info.latest
                        ReleaseAgeDays = 0
                        Severity = Get-NpmSeverity -Current $info.current -Latest $info.latest
                        CompatibilityRisk = 'Review package changelog'
                        RecommendedAction = "Consider upgrading $packageName"
                        Notes = "Wanted: $($info.wanted)"
                    }
                    
                    $entries += $entry
                }
            }
            finally {
                Pop-Location
            }
        }

        Write-Host "  Found $($entries.Count) outdated npm packages" -ForegroundColor Gray
    }
    catch {
        Write-Warning "npm currency check failed: $_"
    }

    return $entries
}

function Get-NpmSeverity {
    param(
        [string]$Current,
        [string]$Latest
    )

    # Simple version comparison
    if ($Current -ne $Latest) {
        # Check for major version difference
        $currentMajor = ($Current -split '\.')[0] -replace '[^0-9]',''
        $latestMajor = ($Latest -split '\.')[0] -replace '[^0-9]',''
        
        if ($latestMajor -and $currentMajor -and [int]$latestMajor -gt [int]$currentMajor) {
            return 'Major'
        }
        
        return 'Minor'
    }

    return 'UpToDate'
}
