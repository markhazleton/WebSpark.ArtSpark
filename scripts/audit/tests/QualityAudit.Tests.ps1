<#
.SYNOPSIS
    Pester tests for WebSpark.ArtSpark Quality Audit
.DESCRIPTION
    Test harness for audit script functionality and report generation
#>

$ScriptRoot = Split-Path -Parent $PSCommandPath
$ModulePath = Join-Path (Split-Path -Parent $ScriptRoot) "QualityAudit.psm1"
Import-Module $ModulePath -Force

Describe "Quality Audit Module" {
    Context "Initialize-AuditContext" {
        It "Creates audit context with required properties" {
            $tempRoot = $TestDrive
            $context = Initialize-AuditContext -RepoRoot $tempRoot

            $context | Should Not BeNullOrEmpty
            $context.RepoRoot | Should Be $tempRoot.FullName
            $context.StartTime | Should Not BeNullOrEmpty
            $context.OutputPath | Should Match "quality-audit\.md$"
        }

        It "Creates output directory if it doesn't exist" {
            $tempRoot = $TestDrive
            $context = Initialize-AuditContext -RepoRoot $tempRoot

            Test-Path $context.OutputDirectory | Should Be $true
        }
    }

    Context "New-BuildHealthFinding" {
        It "Creates finding with mandatory properties" {
            $finding = New-BuildHealthFinding `
                -SourceProject "WebSpark.ArtSpark.Demo" `
                -DiagnosticId "CS8618" `
                -Severity "Warning" `
                -Message "Non-nullable property must contain a non-null value"

            $finding | Should Not BeNullOrEmpty
            $finding.SourceProject | Should Be "WebSpark.ArtSpark.Demo"
            $finding.DiagnosticId | Should Be "CS8618"
            $finding.Severity | Should Be "Warning"
            $finding.Status | Should Be "Open"
        }
    }

    Context "New-PackageCurrencyEntry" {
        It "Creates entry with NuGet package" {
            $entry = New-PackageCurrencyEntry `
                -SourceProject "WebSpark.ArtSpark.Demo" `
                -PackageName "Microsoft.AspNetCore.Mvc" `
                -PackageManager "NuGet" `
                -CurrentVersion "9.0.0" `
                -LatestStableVersion "9.0.1" `
                -Severity "Minor"

            $entry | Should Not BeNullOrEmpty
            $entry.PackageManager | Should Be "NuGet"
            $entry.Severity | Should Be "Minor"
        }
    }

    Context "New-SafeguardControlCheck" {
        It "Creates control check with outcome" {
            $check = New-SafeguardControlCheck `
                -ControlName "PersonaPromptIntegrity" `
                -Description "Verify AI persona definitions" `
                -Outcome "Pass" `
                -Evidence "All persona files validated"

            $check | Should Not BeNullOrEmpty
            $check.ControlName | Should Be "PersonaPromptIntegrity"
            $check.Outcome | Should Be "Pass"
        }
    }
}

Describe "Quality Audit Script" {
    Context "Script Execution" {
        It "Script file exists" {
            $scriptPath = Join-Path (Split-Path -Parent $PSScriptRoot) "run-quality-audit.ps1"
            Test-Path $scriptPath | Should Be $true
        }

        It "Module files exist" {
            $modulePath = Join-Path (Split-Path -Parent $PSScriptRoot) "QualityAudit.psm1"
            Test-Path $modulePath | Should Be $true
            
            $buildDiagPath = Join-Path (Split-Path -Parent $PSScriptRoot) "modules\BuildDiagnostics.ps1"
            Test-Path $buildDiagPath | Should Be $true
            
            $nugetPath = Join-Path (Split-Path -Parent $PSScriptRoot) "modules\NuGetCurrency.ps1"
            Test-Path $nugetPath | Should Be $true
            
            $npmPath = Join-Path (Split-Path -Parent $PSScriptRoot) "modules\NpmCurrency.ps1"
            Test-Path $npmPath | Should Be $true
        }

        It "Generates report with all sections" {
            # Test that output directory gets created and report is generated
            Push-Location (Split-Path -Parent $PSScriptRoot)
            try {
                $scriptPath = Join-Path (Split-Path -Parent $PSScriptRoot) "run-quality-audit.ps1"
                & $scriptPath -SkipSafeguards | Out-Null
                
                $repoRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
                $outputDir = Join-Path $repoRoot "docs\copilot\$(Get-Date -Format 'yyyy-MM-dd')"
                $reportPath = Join-Path $outputDir "quality-audit.md"
                
                Test-Path $reportPath | Should Be $true
                
                $content = Get-Content $reportPath -Raw
                $content | Should Match "Build Diagnostics"
                $content | Should Match "Dependency Currency"
                $content | Should Match "Backlog Recommendations"
            }
            finally {
                Pop-Location
            }
        }
    }
}
