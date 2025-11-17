<#
.SYNOPSIS
    WebSpark.ArtSpark Quality Audit Module
.DESCRIPTION
    Shared context helpers and data model normalization for quality audit workflows
#>

<#
.SYNOPSIS
    Initialize audit execution context
.DESCRIPTION
    Creates audit run metadata and output directory structure
#>
function Initialize-AuditContext {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$RepoRoot,
        
        [string]$OutputDirectory
    )

    $startTime = Get-Date
    $dateStamp = $startTime.ToString('yyyy-MM-dd')
    
    if ([string]::IsNullOrEmpty($OutputDirectory)) {
        $OutputDirectory = Join-Path $RepoRoot "docs\copilot\$dateStamp"
    }

    # Ensure output directory exists
    if (-not (Test-Path $OutputDirectory)) {
        New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
    }

    $outputPath = Join-Path $OutputDirectory "quality-audit.md"

    return [PSCustomObject]@{
        RepoRoot = $RepoRoot
        StartTime = $startTime
        OutputDirectory = $OutputDirectory
        OutputPath = $outputPath
        DateStamp = $dateStamp
        Duration = New-TimeSpan -Start $startTime -End (Get-Date)
    }
}

<#
.SYNOPSIS
    Create a Build Health Finding object
.DESCRIPTION
    Normalizes build diagnostic data into standard model
#>
function New-BuildHealthFinding {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$SourceProject,
        
        [Parameter(Mandatory)]
        [string]$DiagnosticId,
        
        [Parameter(Mandatory)]
        [ValidateSet('Error', 'Warning', 'Info')]
        [string]$Severity,
        
        [Parameter(Mandatory)]
        [string]$Message,
        
        [string]$FilePath,
        [int]$LineNumber = 0,
        [ValidateSet('Compiler', 'Analyzer', 'Build', 'Test')]
        [string]$Category = 'Compiler',
        [string]$RecommendedFix,
        [string]$Owner,
        [string[]]$LinkedDependencies = @(),
        [ValidateSet('Open', 'Planned', 'Resolved', 'Deferred')]
        [string]$Status = 'Open'
    )

    return [PSCustomObject]@{
        SourceProject = $SourceProject
        FilePath = $FilePath
        LineNumber = $LineNumber
        DiagnosticId = $DiagnosticId
        Severity = $Severity
        Category = $Category
        Message = $Message
        RecommendedFix = $RecommendedFix
        Owner = $Owner
        LinkedDependencies = $LinkedDependencies
        Status = $Status
    }
}

<#
.SYNOPSIS
    Create a Package Currency Entry object
.DESCRIPTION
    Normalizes package dependency data into standard model
#>
function New-PackageCurrencyEntry {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$SourceProject,
        
        [Parameter(Mandatory)]
        [string]$PackageName,
        
        [Parameter(Mandatory)]
        [ValidateSet('NuGet', 'npm')]
        [string]$PackageManager,
        
        [Parameter(Mandatory)]
        [string]$CurrentVersion,
        
        [string]$LatestStableVersion,
        [int]$ReleaseAgeDays = 0,
        [ValidateSet('UpToDate', 'Minor', 'Major', 'Security')]
        [string]$Severity = 'UpToDate',
        [string]$CompatibilityRisk,
        [string]$RecommendedAction,
        [string]$Notes
    )

    return [PSCustomObject]@{
        SourceProject = $SourceProject
        PackageName = $PackageName
        PackageManager = $PackageManager
        CurrentVersion = $CurrentVersion
        LatestStableVersion = $LatestStableVersion
        ReleaseAgeDays = $ReleaseAgeDays
        Severity = $Severity
        CompatibilityRisk = $CompatibilityRisk
        RecommendedAction = $RecommendedAction
        Notes = $Notes
    }
}

<#
.SYNOPSIS
    Create a Safeguard Control Check object
.DESCRIPTION
    Normalizes safeguard audit data into standard model
#>
function New-SafeguardControlCheck {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$ControlName,
        
        [Parameter(Mandatory)]
        [string]$Description,
        
        [Parameter(Mandatory)]
        [ValidateSet('Pass', 'Fail', 'NeedsFollowUp')]
        [string]$Outcome,
        
        [string]$Evidence,
        [string]$FollowUpAction,
        [string]$Owner,
        [datetime]$DueDate,
        [string[]]$RelatedComponents = @()
    )

    return [PSCustomObject]@{
        ControlName = $ControlName
        Description = $Description
        Evidence = $Evidence
        Outcome = $Outcome
        FollowUpAction = $FollowUpAction
        Owner = $Owner
        DueDate = $DueDate
        RelatedComponents = $RelatedComponents
    }
}

<#
.SYNOPSIS
    Filter and score backlog items
.DESCRIPTION
    Orders findings by severity, risk, and effort for remediation planning
#>
function Get-FilteredBacklog {
    [CmdletBinding()]
    param(
        [array]$Diagnostics = @(),
        [array]$Dependencies = @(),
        
        [ValidateSet('All', 'Error', 'Warning', 'Info')]
        [string]$Severity = 'All',
        
        [int]$MaxItems = 0
    )

    $backlogItems = @()

    # Ensure inputs are arrays
    if ($null -eq $Diagnostics) { $Diagnostics = @() }
    if ($null -eq $Dependencies) { $Dependencies = @() }

    # Convert diagnostics to backlog items
    foreach ($diagnostic in $Diagnostics) {
        if ($Severity -ne 'All' -and $diagnostic.Severity -ne $Severity) {
            continue
        }

        $score = Get-BacklogScore -Item $diagnostic -Type 'Diagnostic'
        
        $backlogItems += [PSCustomObject]@{
            Type = 'Build Diagnostic'
            Title = "$($diagnostic.DiagnosticId): $($diagnostic.Message)"
            Source = $diagnostic.SourceProject
            Severity = $diagnostic.Severity
            Score = $score
            Action = $diagnostic.RecommendedFix
            Owner = $diagnostic.Owner
            Effort = Get-EstimatedEffort -Item $diagnostic
        }
    }

    # Convert dependencies to backlog items
    foreach ($dependency in $Dependencies) {
        $depSeverity = switch ($dependency.Severity) {
            'Security' { 'Error' }
            'Major' { 'Warning' }
            default { 'Info' }
        }

        if ($Severity -ne 'All' -and $depSeverity -ne $Severity) {
            continue
        }

        $score = Get-BacklogScore -Item $dependency -Type 'Dependency'
        
        $backlogItems += [PSCustomObject]@{
            Type = 'Package Update'
            Title = "$($dependency.PackageName) ($($dependency.CurrentVersion) → $($dependency.LatestStableVersion))"
            Source = $dependency.SourceProject
            Severity = $depSeverity
            Score = $score
            Action = $dependency.RecommendedAction
            Owner = ''
            Effort = Get-EstimatedEffort -Item $dependency
        }
    }

    # Sort by score (highest priority first)
    $backlogItems = $backlogItems | Sort-Object Score -Descending

    # Apply max items limit
    if ($MaxItems -gt 0) {
        $backlogItems = $backlogItems | Select-Object -First $MaxItems
    }

    return $backlogItems
}

function Get-BacklogScore {
    param(
        [Parameter(Mandatory)]
        $Item,
        
        [Parameter(Mandatory)]
        [ValidateSet('Diagnostic', 'Dependency')]
        [string]$Type
    )

    $score = 0

    if ($Type -eq 'Diagnostic') {
        # Higher score for errors
        switch ($Item.Severity) {
            'Error' { $score += 100 }
            'Warning' { $score += 50 }
            'Info' { $score += 10 }
        }
    }
    else {
        # Dependency scoring
        switch ($Item.Severity) {
            'Security' { $score += 200 }
            'Major' { $score += 75 }
            'Minor' { $score += 25 }
            default { $score += 10 }
        }
    }

    return $score
}

function Get-EstimatedEffort {
    param($Item)

    # Simple effort estimation
    if ($Item.PSObject.Properties['DiagnosticId']) {
        # Build diagnostic
        return 'Medium'
    }
    elseif ($Item.PSObject.Properties['Severity']) {
        # Dependency
        switch ($Item.Severity) {
            'Security' { return 'High' }
            'Major' { return 'High' }
            default { return 'Low' }
        }
    }

    return 'Unknown'
}

Export-ModuleMember -Function Initialize-AuditContext, New-BuildHealthFinding, New-PackageCurrencyEntry, New-SafeguardControlCheck, Get-FilteredBacklog
