<#
.SYNOPSIS
    Run WebSpark.ArtSpark quality audit workflow
.DESCRIPTION
    Orchestrates build diagnostics, dependency currency checks, and safeguard audits.
    Generates Markdown reports in docs/copilot/YYYY-MM-DD/ with actionable findings.
.PARAMETER OutputDirectory
    Custom output directory for audit reports (default: docs/copilot/YYYY-MM-DD)
.PARAMETER Severity
    Filter backlog items by severity: All, Error, Warning, Info (default: All)
.PARAMETER MaxItems
    Maximum number of backlog items to include (default: unlimited)
.PARAMETER SkipBuild
    Skip build diagnostics collection
.PARAMETER SkipDependencies
    Skip dependency currency checks
.PARAMETER SkipSafeguards
    Skip safeguard audits
.EXAMPLE
    .\run-quality-audit.ps1
.EXAMPLE
    .\run-quality-audit.ps1 -Severity Warning -MaxItems 10
#>

[CmdletBinding()]
param(
    [string]$OutputDirectory,
    [ValidateSet('All', 'Error', 'Warning', 'Info')]
    [string]$Severity = 'All',
    [int]$MaxItems = 0,
    [switch]$SkipBuild,
    [switch]$SkipDependencies,
    [switch]$SkipSafeguards
)

$ErrorActionPreference = 'Stop'
$ScriptRoot = Split-Path -Parent $PSCommandPath
$RepoRoot = Split-Path -Parent (Split-Path -Parent $ScriptRoot)

# Import quality audit module and diagnostic modules
Import-Module "$ScriptRoot\QualityAudit.psm1" -Force
. "$ScriptRoot\modules\BuildDiagnostics.ps1"
. "$ScriptRoot\modules\NuGetCurrency.ps1"
. "$ScriptRoot\modules\NpmCurrency.ps1"
. "$ScriptRoot\modules\Safeguards.ps1"

try {
    Write-Host "=== WebSpark.ArtSpark Quality Audit ===" -ForegroundColor Cyan
    Write-Host "Repository: $RepoRoot" -ForegroundColor Gray
    Write-Host "Started: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
    Write-Host ""

    # Initialize audit context
    $auditContext = Initialize-AuditContext -RepoRoot $RepoRoot -OutputDirectory $OutputDirectory
    Write-Host "Output: $($auditContext.OutputPath)" -ForegroundColor Gray
    Write-Host ""

    # Initialize collections
    $diagnostics = @()
    $dependencies = @()
    $safeguards = @()

    # Phase 1: Build Diagnostics
    if (-not $SkipBuild) {
        Write-Host "[1/3] Collecting build diagnostics..." -ForegroundColor Yellow
        $diagnostics = Invoke-BuildDiagnostics -RepoRoot $RepoRoot
    }

    # Phase 2: Dependency Currency
    if (-not $SkipDependencies) {
        Write-Host "[2/3] Checking dependency currency..." -ForegroundColor Yellow
        $nugetPackages = Get-NuGetCurrency -RepoRoot $RepoRoot
        $npmPackages = Get-NpmCurrency -RepoRoot $RepoRoot
        $dependencies = $nugetPackages + $npmPackages
    }

    # Phase 3: Safeguard Audits
    if (-not $SkipSafeguards) {
        Write-Host "[3/3] Auditing safeguards..." -ForegroundColor Yellow
        $safeguards = Invoke-SafeguardAudit -RepoRoot $RepoRoot
    }

    # Update context duration
    $auditContext.Duration = New-TimeSpan -Start $auditContext.StartTime -End (Get-Date)

    Write-Host ""
    Write-Host "=== Audit Complete ===" -ForegroundColor Green
    Write-Host "Duration: $($auditContext.Duration.TotalSeconds.ToString('F2'))s" -ForegroundColor Gray
    Write-Host "Report: $($auditContext.OutputPath)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Summary:" -ForegroundColor Cyan
    Write-Host "  Build Diagnostics: $($diagnostics.Count) findings" -ForegroundColor Gray
    Write-Host "  Outdated Packages: $($dependencies.Count) packages" -ForegroundColor Gray
    Write-Host "  Safeguard Controls: $($safeguards.Count) checks" -ForegroundColor Gray

    # Generate report with real data
    $reportContent = New-Object System.Text.StringBuilder
    [void]$reportContent.AppendLine("# WebSpark.ArtSpark Quality Audit Report")
    [void]$reportContent.AppendLine("")
    [void]$reportContent.AppendLine("**Generated**: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
    [void]$reportContent.AppendLine("**Duration**: $($auditContext.Duration.TotalSeconds.ToString('F2'))s")
    [void]$reportContent.AppendLine("**Repository**: $RepoRoot")
    [void]$reportContent.AppendLine("")
    
    [void]$reportContent.AppendLine("## Summary")
    [void]$reportContent.AppendLine("")
    [void]$reportContent.AppendLine("| Category | Count | Status |")
    [void]$reportContent.AppendLine("|----------|-------|--------|")
    [void]$reportContent.AppendLine("| Build Diagnostics | $($diagnostics.Count) | $(if ($diagnostics.Count -eq 0) { '✓ Pass' } else { '⚠ Review' }) |")
    [void]$reportContent.AppendLine("| Outdated Packages | $($dependencies.Count) | $(if ($dependencies.Count -eq 0) { '✓ Up-to-date' } else { '⚠ Updates Available' }) |")
    [void]$reportContent.AppendLine("| Safeguard Controls | $($safeguards.Count) | Pending |")
    [void]$reportContent.AppendLine("")

    # Build Diagnostics Section
    [void]$reportContent.AppendLine("## Build Diagnostics")
    [void]$reportContent.AppendLine("")
    if ($diagnostics.Count -eq 0) {
        [void]$reportContent.AppendLine("✓ No build diagnostics found")
    }
    else {
        [void]$reportContent.AppendLine("**Total**: $($diagnostics.Count) findings")
        [void]$reportContent.AppendLine("")
        
        # Group by severity
        $errors = $diagnostics | Where-Object { $_.Severity -eq 'Error' }
        $warnings = $diagnostics | Where-Object { $_.Severity -eq 'Warning' }
        
        if ($errors.Count -gt 0) {
            [void]$reportContent.AppendLine("### Errors ($($errors.Count))")
            [void]$reportContent.AppendLine("")
            foreach ($error in $errors | Select-Object -First 10) {
                [void]$reportContent.AppendLine("- **$($error.DiagnosticId)** in ``$($error.SourceProject)``")
                [void]$reportContent.AppendLine("  - $($error.Message)")
                if ($error.RecommendedFix) {
                    [void]$reportContent.AppendLine("  - Fix: $($error.RecommendedFix)")
                }
                [void]$reportContent.AppendLine("")
            }
        }
        
        if ($warnings.Count -gt 0) {
            [void]$reportContent.AppendLine("### Warnings ($($warnings.Count))")
            [void]$reportContent.AppendLine("")
            # Group warnings by diagnostic ID
            $warningGroups = $warnings | Group-Object DiagnosticId | Sort-Object Count -Descending
            foreach ($group in $warningGroups | Select-Object -First 5) {
                [void]$reportContent.AppendLine("- **$($group.Name)**: $($group.Count) occurrences")
                $sample = $group.Group | Select-Object -First 1
                [void]$reportContent.AppendLine("  - Example: $($sample.Message)")
                if ($sample.RecommendedFix) {
                    [void]$reportContent.AppendLine("  - Fix: $($sample.RecommendedFix)")
                }
                [void]$reportContent.AppendLine("")
            }
        }
    }
    [void]$reportContent.AppendLine("")

    # Dependency Currency Section
    [void]$reportContent.AppendLine("## Dependency Currency")
    [void]$reportContent.AppendLine("")
    if ($dependencies.Count -eq 0) {
        [void]$reportContent.AppendLine("✓ All packages are up-to-date")
    }
    else {
        [void]$reportContent.AppendLine("**Total**: $($dependencies.Count) outdated packages")
        [void]$reportContent.AppendLine("")
        
        # Group by severity
        $major = $dependencies | Where-Object { $_.Severity -eq 'Major' }
        $minor = $dependencies | Where-Object { $_.Severity -eq 'Minor' }
        $security = $dependencies | Where-Object { $_.Severity -eq 'Security' }
        
        if ($security.Count -gt 0) {
            [void]$reportContent.AppendLine("### 🔴 Security Updates ($($security.Count))")
            [void]$reportContent.AppendLine("")
            foreach ($pkg in $security) {
                [void]$reportContent.AppendLine("- **$($pkg.PackageName)** in ``$($pkg.SourceProject)``")
                [void]$reportContent.AppendLine("  - Current: $($pkg.CurrentVersion) → Latest: $($pkg.LatestStableVersion)")
                [void]$reportContent.AppendLine("  - Action: $($pkg.RecommendedAction)")
                [void]$reportContent.AppendLine("")
            }
        }
        
        if ($major.Count -gt 0) {
            [void]$reportContent.AppendLine("### 🟠 Major Updates ($($major.Count))")
            [void]$reportContent.AppendLine("")
            foreach ($pkg in $major | Select-Object -First 10) {
                [void]$reportContent.AppendLine("- **$($pkg.PackageName)** in ``$($pkg.SourceProject)``")
                [void]$reportContent.AppendLine("  - Current: $($pkg.CurrentVersion) → Latest: $($pkg.LatestStableVersion)")
                [void]$reportContent.AppendLine("  - Risk: $($pkg.CompatibilityRisk)")
                [void]$reportContent.AppendLine("")
            }
        }
        
        if ($minor.Count -gt 0) {
            [void]$reportContent.AppendLine("### 🟡 Minor Updates ($($minor.Count))")
            [void]$reportContent.AppendLine("")
            [void]$reportContent.AppendLine("| Package | Project | Current | Latest |")
            [void]$reportContent.AppendLine("|---------|---------|---------|--------|")
            foreach ($pkg in $minor | Select-Object -First 10) {
                [void]$reportContent.AppendLine("| $($pkg.PackageName) | $($pkg.SourceProject) | $($pkg.CurrentVersion) | $($pkg.LatestStableVersion) |")
            }
            [void]$reportContent.AppendLine("")
        }
    }
    [void]$reportContent.AppendLine("")

    # Safeguard Controls Section
    [void]$reportContent.AppendLine("## Safeguard Controls")
    [void]$reportContent.AppendLine("")
    if ($safeguards.Count -eq 0) {
        [void]$reportContent.AppendLine("_No safeguard checks performed_")
    }
    else {
        [void]$reportContent.AppendLine("**Total**: $($safeguards.Count) control checks")
        [void]$reportContent.AppendLine("")
        
        # Group by outcome
        $passed = $safeguards | Where-Object { $_.Outcome -eq 'Pass' }
        $failed = $safeguards | Where-Object { $_.Outcome -eq 'Fail' }
        $needsFollowUp = $safeguards | Where-Object { $_.Outcome -eq 'NeedsFollowUp' }
        
        [void]$reportContent.AppendLine("| Status | Count |")
        [void]$reportContent.AppendLine("|--------|-------|")
        [void]$reportContent.AppendLine("| ✓ Pass | $($passed.Count) |")
        [void]$reportContent.AppendLine("| ⚠ Needs Follow-Up | $($needsFollowUp.Count) |")
        [void]$reportContent.AppendLine("| ✗ Fail | $($failed.Count) |")
        [void]$reportContent.AppendLine("")
        
        if ($failed.Count -gt 0) {
            [void]$reportContent.AppendLine("### ✗ Failed Controls")
            [void]$reportContent.AppendLine("")
            foreach ($check in $failed) {
                [void]$reportContent.AppendLine("#### $($check.ControlName)")
                [void]$reportContent.AppendLine("- **Description**: $($check.Description)")
                [void]$reportContent.AppendLine("- **Evidence**: $($check.Evidence)")
                [void]$reportContent.AppendLine("- **Action**: $($check.FollowUpAction)")
                [void]$reportContent.AppendLine("- **Owner**: $($check.Owner)")
                [void]$reportContent.AppendLine("")
            }
        }
        
        if ($needsFollowUp.Count -gt 0) {
            [void]$reportContent.AppendLine("### ⚠ Needs Follow-Up")
            [void]$reportContent.AppendLine("")
            foreach ($check in $needsFollowUp) {
                [void]$reportContent.AppendLine("#### $($check.ControlName)")
                [void]$reportContent.AppendLine("- **Description**: $($check.Description)")
                [void]$reportContent.AppendLine("- **Evidence**: $($check.Evidence)")
                [void]$reportContent.AppendLine("- **Action**: $($check.FollowUpAction)")
                [void]$reportContent.AppendLine("")
            }
        }
        
        if ($passed.Count -gt 0) {
            [void]$reportContent.AppendLine("### ✓ Passed Controls")
            [void]$reportContent.AppendLine("")
            foreach ($check in $passed) {
                [void]$reportContent.AppendLine("- **$($check.ControlName)**: $($check.Description)")
            }
            [void]$reportContent.AppendLine("")
        }
    }
    [void]$reportContent.AppendLine("")

    # Backlog Recommendations Section
    [void]$reportContent.AppendLine("## Backlog Recommendations")
    [void]$reportContent.AppendLine("")
    
    # Generate filtered backlog
    $backlogItems = Get-FilteredBacklog -Diagnostics $diagnostics -Dependencies $dependencies -Severity $Severity -MaxItems $MaxItems
    
    if ($backlogItems.Count -eq 0) {
        [void]$reportContent.AppendLine("✓ No backlog items match the specified criteria")
    }
    else {
        [void]$reportContent.AppendLine("**Filtered Backlog**: $($backlogItems.Count) items (Severity: $Severity, MaxItems: $(if ($MaxItems -gt 0) { $MaxItems } else { 'Unlimited' }))")
        [void]$reportContent.AppendLine("")
        [void]$reportContent.AppendLine("| Priority | Type | Title | Source | Severity | Effort | Action |")
        [void]$reportContent.AppendLine("|----------|------|-------|--------|----------|--------|--------|")
        
        $priority = 1
        foreach ($item in $backlogItems) {
            $titleShort = if ($item.Title.Length -gt 50) { $item.Title.Substring(0, 47) + "..." } else { $item.Title }
            [void]$reportContent.AppendLine("| $priority | $($item.Type) | $titleShort | $($item.Source) | $($item.Severity) | $($item.Effort) | $($item.Action) |")
            $priority++
        }
        [void]$reportContent.AppendLine("")
        
        # Generate separate backlog file if requested
        if ($backlogItems.Count -gt 0) {
            $backlogPath = Join-Path $auditContext.OutputDirectory "quality-audit-backlog.md"
            $backlogContent = New-Object System.Text.StringBuilder
            [void]$backlogContent.AppendLine("# Quality Audit Remediation Backlog")
            [void]$backlogContent.AppendLine("")
            [void]$backlogContent.AppendLine("**Generated**: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
            [void]$backlogContent.AppendLine("**Filter**: Severity=$Severity, MaxItems=$(if ($MaxItems -gt 0) { $MaxItems } else { 'Unlimited' })")
            [void]$backlogContent.AppendLine("")
            
            foreach ($item in $backlogItems) {
                [void]$backlogContent.AppendLine("## [$($item.Severity)] $($item.Title)")
                [void]$backlogContent.AppendLine("")
                [void]$backlogContent.AppendLine("- **Type**: $($item.Type)")
                [void]$backlogContent.AppendLine("- **Source**: $($item.Source)")
                [void]$backlogContent.AppendLine("- **Effort**: $($item.Effort)")
                [void]$backlogContent.AppendLine("- **Action**: $($item.Action)")
                [void]$backlogContent.AppendLine("- **Owner**: $(if ($item.Owner) { $item.Owner } else { '_Unassigned_' })")
                [void]$backlogContent.AppendLine("- **Priority Score**: $($item.Score)")
                [void]$backlogContent.AppendLine("")
            }
            
            $backlogContent.ToString() | Out-File -FilePath $backlogPath -Encoding UTF8
            Write-Host "Backlog file: $backlogPath" -ForegroundColor Gray
        }
    }
    [void]$reportContent.AppendLine("")

    [void]$reportContent.AppendLine("---")
    [void]$reportContent.AppendLine("*Generated by run-quality-audit.ps1*")

    $reportContent.ToString() | Out-File -FilePath $auditContext.OutputPath -Encoding UTF8
    Write-Host "Report written successfully" -ForegroundColor Green

    exit 0
}
catch {
    Write-Error "Audit failed: $_"
    exit 1
}
