<#
.SYNOPSIS
    AI & Content Safeguard audits for WebSpark.ArtSpark
.DESCRIPTION
    Validates AI persona definitions, moderation hooks, and cultural safeguards
#>

<#
.SYNOPSIS
    Invoke safeguard audits
.DESCRIPTION
    Reviews AI persona files and Demo moderation hooks for compliance
#>
function Invoke-SafeguardAudit {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$RepoRoot
    )

    $checks = @()

    Write-Host "  Auditing AI persona definitions..." -ForegroundColor Gray

    # Check 1: Persona files exist
    $personaDir = Join-Path $RepoRoot "WebSpark.ArtSpark.Agent\Personas"
    $personaCheck = Test-Path $personaDir
    
    $checks += [PSCustomObject]@{
        ControlName = 'PersonaDirectoryExists'
        Description = 'Verify AI persona directory structure'
        Evidence = if ($personaCheck) { "Directory found at $personaDir" } else { "Directory not found" }
        Outcome = if ($personaCheck) { 'Pass' } else { 'Fail' }
        FollowUpAction = if (-not $personaCheck) { 'Create Personas directory and add persona definitions' } else { '' }
        Owner = 'AI Team'
        DueDate = $null
        RelatedComponents = @('WebSpark.ArtSpark.Agent')
    }

    # Check 2: Persona files contain required fields
    if ($personaCheck) {
        $personaFiles = Get-ChildItem -Path $personaDir -Filter "*.cs" -ErrorAction SilentlyContinue
        
        if ($personaFiles.Count -gt 0) {
            $allPersonasValid = $true
            $personaNames = @()
            
            foreach ($file in $personaFiles) {
                $content = Get-Content $file.FullName -Raw
                $personaNames += $file.BaseName
                
                # Check for essential persona elements (simplified validation)
                if ($content -notmatch 'SystemMessage|Prompt|Description') {
                    $allPersonasValid = $false
                }
            }
            
            $checks += [PSCustomObject]@{
                ControlName = 'PersonaDefinitionIntegrity'
                Description = 'Verify persona files contain required prompt structures'
                Evidence = "Found $($personaFiles.Count) persona files: $($personaNames -join ', ')"
                Outcome = if ($allPersonasValid) { 'Pass' } else { 'NeedsFollowUp' }
                FollowUpAction = if (-not $allPersonasValid) { 'Review persona definitions for completeness' } else { '' }
                Owner = 'AI Team'
                DueDate = $null
                RelatedComponents = @('WebSpark.ArtSpark.Agent')
            }
        } else {
            $checks += [PSCustomObject]@{
                ControlName = 'PersonaDefinitionIntegrity'
                Description = 'Verify persona files exist'
                Evidence = 'No persona files found'
                Outcome = 'Fail'
                FollowUpAction = 'Add persona definition files'
                Owner = 'AI Team'
                DueDate = $null
                RelatedComponents = @('WebSpark.ArtSpark.Agent')
            }
        }
    }

    # Check 3: Moderation hooks in Agent services
    Write-Host "  Checking moderation hooks..." -ForegroundColor Gray
    
    $agentServicesDir = Join-Path $RepoRoot "WebSpark.ArtSpark.Agent\Services"
    if (Test-Path $agentServicesDir) {
        $serviceFiles = Get-ChildItem -Path $agentServicesDir -Filter "*.cs" -Recurse
        $moderationFound = $false
        
        foreach ($file in $serviceFiles) {
            $content = Get-Content $file.FullName -Raw
            if ($content -match 'moderation|content\s*filter|guardrail') {
                $moderationFound = $true
                break
            }
        }
        
        $checks += [PSCustomObject]@{
            ControlName = 'ModerationHooksPresent'
            Description = 'Verify AI responses include content moderation'
            Evidence = if ($moderationFound) { 'Moderation code found in Agent services' } else { 'No explicit moderation found' }
            Outcome = if ($moderationFound) { 'Pass' } else { 'NeedsFollowUp' }
            FollowUpAction = if (-not $moderationFound) { 'Implement or document moderation strategy' } else { '' }
            Owner = 'AI Team'
            DueDate = $null
            RelatedComponents = @('WebSpark.ArtSpark.Agent')
        }
    }

    # Check 4: Cultural sensitivity markers
    Write-Host "  Checking cultural safeguards..." -ForegroundColor Gray
    
    $docsDir = Join-Path $RepoRoot "docs"
    $culturalDocFound = $false
    
    if (Test-Path $docsDir) {
        $docFiles = Get-ChildItem -Path $docsDir -Filter "*persona*.md" -Recurse -ErrorAction SilentlyContinue
        foreach ($file in $docFiles) {
            $content = Get-Content $file.FullName -Raw
            if ($content -match 'cultural|sensitivity|responsible\s+AI') {
                $culturalDocFound = $true
                break
            }
        }
    }
    
    $checks += [PSCustomObject]@{
        ControlName = 'CulturalSafeguardsDocumented'
        Description = 'Verify cultural sensitivity guidelines are documented'
        Evidence = if ($culturalDocFound) { 'Cultural guidance found in documentation' } else { 'No explicit cultural guidance found' }
        Outcome = if ($culturalDocFound) { 'Pass' } else { 'NeedsFollowUp' }
        FollowUpAction = if (-not $culturalDocFound) { 'Document cultural sensitivity guidelines' } else { '' }
        Owner = 'Documentation Team'
        DueDate = $null
        RelatedComponents = @('docs/', 'WebSpark.ArtSpark.Agent')
    }

    Write-Host "  Found $($checks.Count) safeguard checks" -ForegroundColor Gray

    return $checks
}
